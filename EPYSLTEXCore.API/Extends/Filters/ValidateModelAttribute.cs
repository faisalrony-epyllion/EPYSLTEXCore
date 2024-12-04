using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Net;

namespace EPYSLTEXCore.API.Extends.Filters
{
    public class ValidateModelAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            if (!context.ModelState.IsValid)
            {
                string messages = string.Join("<br>", context.ModelState.Values
                                            .SelectMany(x => x.Errors)
                                            .Select(x => string.IsNullOrEmpty(x.ErrorMessage) ? x.Exception?.Message : x.ErrorMessage));

                context.Result = new BadRequestObjectResult(new { Error = messages });
            }
        }
    }
}
