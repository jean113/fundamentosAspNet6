using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Blog.Extensions
{
    //as extensões no dotnet devem ser estáticas
    public static class ModelStateExtension
    {
        //isso é feito para extender as possibilidades no Model States
        //usar a palavra this dentro dos paramentros vai tornar este função, um metodo de extensão ou seja
        //vai adicionar o GetErros em todos o ModelStateDictionary
        public static List<string> GetErrors(this ModelStateDictionary modelState)
        {
            var result = new List<string>();
            foreach (var item in modelState.Values)
                result.AddRange(item.Errors.Select(error => error.ErrorMessage));

            return result;
        }
    }
}