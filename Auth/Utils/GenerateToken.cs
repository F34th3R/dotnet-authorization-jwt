using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using TodoApp.Auth.Configuration;
using TodoApp.Database;
using TodoApp.Models;
using TodoApp.Utils;

namespace TodoApp.Auth.Utils
{
    public abstract class GenerateToken
    {
        private readonly JwtConfig _jwtConfig;
        private readonly ApiDbContext _apiDbContext;

        protected GenerateToken(JwtConfig jwtConfig, ApiDbContext apiDbContext)
        {
            _jwtConfig = jwtConfig;
            _apiDbContext = apiDbContext;
        }

        public async Task<AuthResult> GenerateJwtTokens(IdentityUser user)
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
    }
}
