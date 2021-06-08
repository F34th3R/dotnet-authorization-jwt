using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TodoApp.Auth.Utils;
using TodoApp.Models.DTOs.Requests;
using TodoApp.Models.DTOs.Responses;

namespace TodoApp.Auth.Logic
{
    // TODO 
    public class LoginLogic : ControllerBase
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly GenerateToken _generateToken;
        
        protected LoginLogic(UserManager<IdentityUser> userManager, GenerateToken generateToken)
        {
            _userManager = userManager;
            _generateToken = generateToken;
        }

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

            var jwtToken = await _generateToken.GenerateJwtTokens(existingUser);

            return Ok(jwtToken);
        }
    }
}
