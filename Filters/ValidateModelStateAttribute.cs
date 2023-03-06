using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using OtusSocialNetwork.DataClasses.Responses;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Text.Json;

namespace OtusSocialNetwork.Filters;

public class ValidateModelStateAttribute : ActionFilterAttribute
{
    /// <summary>
    /// Validates Model automaticaly 
    /// </summary>
    /// <param name="context"></param>
    /// <inheritdoc />
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        if (!context.ModelState.IsValid)
        {
            var errors = new List<string>();
            foreach (var item in context.ModelState)
            {
                errors.Add($"[{item.Key}]:{item.Value}");
            }
            var result = JsonSerializer.Serialize(new ErrorRes(errors.Aggregate((x,y)=> x + "; " + y)), new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
            context.Result = new BadRequestObjectResult(new ErrorRes(errors.Aggregate((x, y) => x + "; " + y)));
        }
    }
}