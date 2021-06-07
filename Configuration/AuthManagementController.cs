using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using TodoApp.Filters;
using TodoApp.Models.DTOs.Requests;
using TodoApp.Models.DTOs.Responses;

namespace TodoApp.Configuration
{
    [Route("api/auth")]
    [ApiController]
    public class AuthManagementController : ControllerBase
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly JwtConfig _jwtConfig;

        public AuthManagementController(UserManager<IdentityUser> userManager,
            IOptionsMonitor<JwtConfig> optionsMonitor)
        {
            _userManager = userManager;
            _jwtConfig = optionsMonitor.CurrentValue;
        }

        private string GenerateJwtTokens(IdentityUser user)
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
                Expires = DateTime.UtcNow.AddHours(6),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature)
            };

            var token = jwtTokenHandler.CreateToken(tokenDescriptor);
            var jwtToken = jwtTokenHandler.WriteToken(token);

            return jwtToken;
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

            var jwtToken = GenerateJwtTokens(newUser);

            // Return the token to the API
            return Ok(new RegistrationResponse()
            {
                Success = true,
                Token = jwtToken
            });
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
            if (! isCorrectPassword)
                return BadRequest(new RegistrationResponse()
                {
                    Errors = new List<string>()
                    {
                        "Wrong Password."
                    },
                    Success = false
                });

            var jwtToken = GenerateJwtTokens(existingUser);

            return Ok(new RegistrationResponse()
            {
                Success = true,
                Token = jwtToken
            });
        }
    }
}
