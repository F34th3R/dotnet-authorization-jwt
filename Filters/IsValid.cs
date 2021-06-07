using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace TodoApp.Filters
{
    public class IsValid : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            if (context.ModelState.IsValid) return;
            
            context.ModelState.AddModelError("Error", "Something went wrong");
            context.Result = new BadRequestObjectResult(context.ModelState);
        }
    }
}
