using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Blog.Attributes;


[AttributeUsage(validOn: AttributeTargets.Method | AttributeTargets.Class)] //localhost:5001?api_key=CHAVE
public class ApiKeyAttributes : Attribute, IAsyncActionFilter
{
    async Task IAsyncActionFilter.OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        if(!context.HttpContext.Request.Query.TryGetValue(Configuration.ApiKeyName, out var extractedApiKey))
        {
            context.Result = new ContentResult()
            {
                StatusCode = 401,
                Content = "ApiKey não encontrada"
            };

            return;
        }

        if (!Configuration.ApiKey.Equals(extractedApiKey))
        {
           context.Result = new ContentResult()
            {
                StatusCode = 403,
                Content = "Acesso não autorizado"
            };

            return;
        }

        await next();    
    }

}