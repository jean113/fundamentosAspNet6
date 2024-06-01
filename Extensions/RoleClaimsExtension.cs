using System.Security.Claims;
using Blog.Models;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Blog.Extensions
{
    //as extensões no dotnet devem ser estáticas
    public static class RoleClaimExtension
    {
        
        public static IEnumerable<Claim> GetClaims(this User user)
        {
            var result = new List<Claim>
            {
                new Claim(type:ClaimTypes.Name, value: user.Email), //vira User.Identity.Email
            };

            result.AddRange(
                user.Roles.Select(role => new Claim(ClaimTypes.Role, role.Slug))
            );

            return result;
        }
    }
}