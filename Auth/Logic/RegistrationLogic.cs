using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TodoApp.Auth.Utils;
using TodoApp.Models.DTOs.Requests;
using TodoApp.Models.DTOs.Responses;

namespace TodoApp.Auth.Logic
{
    // TODO 
    public class RegistrationLogic : ControllerBase
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly GenerateToken _generateToken;
        
        public RegistrationLogic(
            UserManager<IdentityUser> userManager, GenerateToken generateToken)
        {
            _userManager = userManager;
            _generateToken = generateToken;
        }
        
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

            var jwtToken = await _generateToken.GenerateJwtTokens(newUser);

            // Return the token to the API
            return Ok(jwtToken);
        }
    }
}
