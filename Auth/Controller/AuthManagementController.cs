using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using TodoApp.Auth.Configuration;
using TodoApp.Database;
using TodoApp.Filters;
using TodoApp.Models;
using TodoApp.Models.DTOs.Requests;
using TodoApp.Models.DTOs.Responses;
using TodoApp.Utils;

namespace TodoApp.Auth.Controller
{
    [Route("api/auth")]
    [ApiController]
    public class AuthManagementController : ControllerBase
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly JwtConfig _jwtConfig;
        private readonly TokenValidationParameters _tokenValidationParams;
        private readonly ApiDbContext _apiDbContext;

        public AuthManagementController(
            UserManager<IdentityUser> userManager,
            IOptionsMonitor<JwtConfig> optionsMonitor,
            TokenValidationParameters tokenValidationParams,
            ApiDbContext apiDbContext
        )
        {
            _userManager = userManager;
            _jwtConfig = optionsMonitor.CurrentValue;
            _tokenValidationParams = tokenValidationParams;
            _apiDbContext = apiDbContext;
        }

        [HttpPost]
        [Route("register")]
        [IsValidRegister]
        public async Task<IActionResult> Register([FromBody] UserRegistrationDto user)
        {
            var existingUser = await _userManager.FindByEmailAsync(user.Email);
            if (existingUser == null)
                return BadRequest(new RegistrationResponse()
                {
                    Errors = new List<string>()
                    {
                        "Email already in use"
                    },
                    Success = false
                });

            var newUser = new IdentityUser() {Email = user.Email, UserName = user.Username};
            var isCreated = await _userManager.CreateAsync(newUser, user.Password);

            if (!isCreated.Succeeded)
                return BadRequest(new RegistrationResponse()
                {
                    Errors = isCreated.Errors.Select(x => x.Description).ToList(),
                    Success = false
                });

            var jwtToken = await GenerateJwtTokens(newUser);

            // Return the token to the API
            return Ok(jwtToken);
        }

        [HttpPost]
        [Route("login")]
        [IsValidRegister]
        public async Task<IActionResult> Login([FromBody] UserLoginRequest user)
        {
            var existingUser = await _userManager.FindByEmailAsync(user.Email);
            if (existingUser == null)
                return BadRequest(new RegistrationResponse()
                {
                    Errors = new List<string>()
                    {
                        "Wrong Email."
                    },
                    Success = false
                });

            var isCorrectPassword = await _userManager.CheckPasswordAsync(existingUser, user.Password);
            if (!isCorrectPassword)
                return BadRequest(new RegistrationResponse()
                {
                    Errors = new List<string>()
                    {
                        "Wrong Password."
                    },
                    Success = false
                });

            var jwtToken = await GenerateJwtTokens(existingUser);

            return Ok(jwtToken);
        }

        [HttpPost]
        [Route("refreshToken")]
        [IsValid]
        public async Task<IActionResult> RefreshToken([FromBody] TokenRequest tokenRequest)
        {
            var result = await VerifyAndGenerateToken(tokenRequest);

            if (result == null)
            {
                return BadRequest(new RegistrationResponse()
                {
                    Errors = new List<string>()
                    {
                        "Invalid token"
                    },
                    Success = false
                });
            }

            return Ok(result);
        }

        // Generate a token TODO passing to a new file.cs into auth.utils
        private async Task<AuthResult> GenerateJwtTokens(IdentityUser user)
        {
            var jwtTokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_jwtConfig.Secret);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim("Id", user.Id),
                    new Claim(JwtRegisteredClaimNames.Email, user.Email),
                    new Claim(JwtRegisteredClaimNames.Sub, user.Email),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
                }),
                // Expiration time goes here!! :D
                Expires = DateTime.UtcNow.AddHours(6),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature)
            };

            var token = jwtTokenHandler.CreateToken(tokenDescriptor);
            var jwtToken = jwtTokenHandler.WriteToken(token);

            var refreshToken = new RefreshToken()
            {
                JwtId = token.Id,
                IsUsed = false,
                IsRevoked = false,
                UserId = user.Id,
                AddedDate = DateTime.UtcNow,
                ExpirationDate = DateTime.UtcNow.AddMonths(1),
                Token = RandomString.GenerateRandomString(35) + Guid.NewGuid()
            };

            await _apiDbContext.RefreshTokens.AddAsync(refreshToken);
            await _apiDbContext.SaveChangesAsync();

            return new AuthResult()
            {
                Token = jwtToken,
                Success = true,
                RefreshTokeRefrestn = refreshToken.Token
            };
        }

        private async Task<AuthResult> VerifyAndGenerateToken(TokenRequest tokenRequest)
        {
            var jwtTokenHandler = new JwtSecurityTokenHandler();
            try
            {
                // Validation 1 - Validation JWT token format
                var tokenInVerification = jwtTokenHandler.ValidateToken(tokenRequest.Token, _tokenValidationParams,
                    out var validatedToken);

                // Validation 2 - Validate encryption algorithm
                if (validatedToken is JwtSecurityToken jwtSecurityToken)
                {
                    var result = jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256,
                        StringComparison.InvariantCultureIgnoreCase);
                    if (result) return null;
                }

                // Validation 3 - validate expiration date
                var utcExpiryDate = long.Parse(tokenInVerification.Claims
                    .FirstOrDefault(x => x.Type == JwtRegisteredClaimNames.Exp)
                    ?.Value ?? string.Empty);

                var expiryDate = UnixTimeStampToDatetime.Convert(utcExpiryDate);
                if (expiryDate > DateTime.UtcNow)
                {
                    return new AuthResult()
                    {
                        Success = false,
                        Errors = new List<string>()
                        {
                            "Token has not yet expired"
                        }
                    };
                }

                // Validation 4 - validate existence of the token
                var storedToken =
                    await _apiDbContext.RefreshTokens.FirstOrDefaultAsync(x => x.Token == tokenRequest.RefreshToken);
                if (storedToken == null)
                {
                    return new AuthResult()
                    {
                        Success = false,
                        Errors = new List<string>()
                        {
                            "Token does not exist"
                        }
                    };
                }

                // Validation 5 - validate if used
                if (storedToken.IsUsed)
                {
                    return new AuthResult()
                    {
                        Success = false,
                        Errors = new List<string>()
                        {
                            "Token has been used"
                        }
                    };
                }

                // Validation 6 - validate if revoked
                if (storedToken.IsRevoked)
                {
                    return new AuthResult()
                    {
                        Success = false,
                        Errors = new List<string>()
                        {
                            "Token has been revoked"
                        }
                    };
                }

                // Validation 7 - validate the id
                var jti = tokenInVerification.Claims.FirstOrDefault(x => x.Type == JwtRegisteredClaimNames.Jti)?.Value;

                if (storedToken.JwtId != jti)
                {
                    return new AuthResult()
                    {
                        Success = false,
                        Errors = new List<string>()
                        {
                            "Token doesn't match"
                        }
                    };
                }

                // update current token
                storedToken.IsUsed = true;
                _apiDbContext.RefreshTokens.Update(storedToken);
                await _apiDbContext.SaveChangesAsync();

                //generate a new token
                var dbUser = await _userManager.FindByIdAsync(storedToken.UserId);
                return await GenerateJwtTokens(dbUser);
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("Lifetime validation failed. The token is expired."))
                {
                    return new AuthResult()
                    {
                        Success = false,
                        Errors = new List<string>()
                        {
                            "Token has expired please re-login."
                        }
                    };
                }
                else
                {
                    return new AuthResult()
                    {
                        Success = false,
                        Errors = new List<string>()
                        {
                            "Something wet wrong."
                        }
                    };
                }
            }
        }
    }
}
