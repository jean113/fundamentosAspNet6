using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Blog.Extensions;
using Blog.Models;
using Microsoft.IdentityModel.Tokens;

namespace Blog.Services;

public class TokenService
{
    public string GenerateToken(User user)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(Configuration.JwtKey); //retorna para key um valor o tipo byte[]
        var claims = user.GetClaims();
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            //exemplos
            // Subject = new ClaimsIdentity(new Claim[]
            // {
            //     new Claim(type:ClaimTypes.Name, value: "andre"),
            //     new Claim(type:ClaimTypes.Role, value: "user"),
            //     new Claim(type:ClaimTypes.Role, value: "admin"),
            //     // new Claim(type:"fruta", value: "banana"), // exemplo
            // }),

            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddHours(8), //recomendação: token duração de 2 a 8 horas
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(key),
                SecurityAlgorithms.HmacSha256Signature
            )
        };
        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }
}