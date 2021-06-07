using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using TodoApp.Models.DTOs.Responses;

namespace TodoApp.Filters
{
    public class IsValidRegister : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            if (context.ModelState.IsValid) return;

            context.Result = new BadRequestObjectResult(new RegistrationResponse()
            {
                Errors = new List<string>()
                {
                    "Invalid payload."
                },
                Success = false
            });
        }
    }
}
