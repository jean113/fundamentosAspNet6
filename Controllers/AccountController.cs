using System.Text.RegularExpressions;
using Blog.Data;
using Blog.Extensions;
using Blog.Models;
using Blog.Services;
using Blog.ViewModels;
using Blog.ViewModels.Accounts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SecureIdentity.Password;

//Health Check
namespace Blog.Controllers;

[Authorize] //pode colocar somente nas rotas também
[ApiController]
public class AccountController : ControllerBase
{
    private readonly TokenService _tokenService;
    public AccountController(TokenService tokenService) //com isso, foi criado uma injeção de dependencia; isso é o mesmo que usa FromServices
    {
        _tokenService = tokenService;
    }

    [HttpPost(template:"v1/accounts")]
    public async Task<IActionResult> Post([FromBody] RegisterViewModel model,
                                          [FromServices] BlogDataContext context,
                                          [FromServices] EmailService emailService)
    {
        if(!ModelState.IsValid)
        {
            return BadRequest(new ResultViewModel<string>(ModelState.GetErrors()));
        }

        var user = new User
        {
            Name = model.Name,
            Email = model.Email,
            Slug = model.Email.Replace("@", "-").Replace(".", "-"),
        };

        var password = PasswordGenerator.Generate(25, includeSpecialChars: true, upperCase: false);
        user.PasswordHash = PasswordHasher.Hash(password);

        try
        {
            await context.Users.AddAsync(user);
            await context.SaveChangesAsync();

            emailService.Send(user.Name, user.Email, "Bem vindo ao blog", $"Sua senha é <strong>{password}</strong>");

            //dynamic no lugar de um viewport especifico
            // return Ok(new ResultViewModel<dynamic>(new 
            // {
            //     user = user.Email, password
            // }));
        }
         catch(DbUpdateException e)
        {
                return StatusCode(500, value: new ResultViewModel<User>("05XE15 - Não foi possível incluir o usuário"));
        }
        catch(Exception e)
        {
                return StatusCode(500, value: new ResultViewModel<User>("05XE16 - Falha interna no servidor"));
        }

        return Ok(new ResultViewModel<string>("user criado com sucesso", null));

    }

    [HttpPost(template:"v1/accounts/login")]
    public async Task<IActionResult> Login([FromBody]LoginViewModel model, [FromServices] TokenService tokenService, BlogDataContext context)
    {
        if(!ModelState.IsValid)
        {
            return BadRequest(new ResultViewModel<string>(ModelState.GetErrors()));
        }

          var user = await context
            .Users
            .AsNoTracking()
            .Include(x => x.Roles)
            .FirstOrDefaultAsync(x => x.Email == model.Email);

        if(user == null)
            return StatusCode(401, new ResultViewModel<string>("Usuário ou senha inválida"));

        if(!PasswordHasher.Verify(user.PasswordHash, model.Password))
            return StatusCode(401, new ResultViewModel<string>("Usuário ou senha inválida"));

        try
        {
            var token = tokenService.GenerateToken(user);
            return Ok(new ResultViewModel<string>(token, null));
        }
        catch (Exception e)
        {
                return StatusCode(500, value: new ResultViewModel<User>("05XE18 - Falha interna no servidor"));
        }

    }



    [AllowAnonymous] //permite que seja acessado sem estar autorizado
    [HttpPost(template:"v1/login")]
    public IActionResult Login()
    {
        var token = _tokenService.GenerateToken(user:null);
        return Ok(token);
    }

    [Authorize]
    [HttpPost("v1/accounts/upload-image")]
    public async Task<IActionResult> UploadImage([FromBody] UploadImageViewModel model, [FromServices] BlogDataContext context)
    {
        var fileName = $"{Guid.NewGuid().ToString()}.jpg"; //sempre inventa um nome novo, é bom que evita cache
        var data = new Regex(@"^data:imageV[a-z]+;base64,").Replace(model.Base64Image, "");
        var bytes = Convert.FromBase64String(data);  

        try
        {
            await System.IO.File.WriteAllBytesAsync($"wwwroot/images/{fileName}", bytes);
        }
        catch (Exception e)
        {
                return StatusCode(500, value: new ResultViewModel<string>("05XE20 - Falha interna no servidor"));
        }

        var user = await context.Users.FirstOrDefaultAsync(x => x.Email == User.Identity.Name);

        if(user == null)
            return NotFound(new ResultViewModel<User>("Usuário não encontrado"));

        user.Image = $"https://localhost:0000/images/{fileName}";

        try
        {
            context.Users.Update(user);
            await context.SaveChangesAsync();
        }
        catch (Exception e)
        {
            return StatusCode(500, value: new ResultViewModel<string>("05XE22 - Falha interna no servidor"));
        }

        return Ok(new ResultViewModel<string>("Imagem alterada com sucesso", null));
    }

        // É para testar a parte de autorização de usuários
//     [Authorize(Roles = "user")] //pode ter mais de um authorize, basta duplicar a linha e colocar a role
//     [HttpGet(template:"v1/user")]
//     public IActionResult GetUser() => Ok(User.Identity.Name);

//     [Authorize(Roles = "author")]
//     [HttpGet(template:"v1/author")]
//     public IActionResult GetAuthor() => Ok(User.Identity.Name);

//     [Authorize(Roles = "admin")]
//     [HttpGet(template:"v1/admin")]
//     public IActionResult GetAdmin() => Ok(User.Identity.Name);

}