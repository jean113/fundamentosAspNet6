using Blog.Data;
using Blog.Extensions;
using Blog.Models;
using Blog.ViewModels;
using Blog.ViewModels.Categories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

//Health Check
namespace Blog.Controllers;

[ApiController]
public class CategoryController : ControllerBase
{
    //[HttpGet("categories")] Padrao de nomenclatura de endpoint - sempre nome do modelo minusculo e no plural 
    [HttpGet("v1/categories")] //Para versionar basta colocar v1, v2, v3 e etc na rota
    public async Task<IActionResult> GetAsync([FromServices] BlogDataContext context, [FromServices] IMemoryCache cache)
    {
        try
        {
            var categories = cache.GetOrCreate("CategoriesCache", entry =>{
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1);
                return GetCategories(context);
            });

            return Ok(new ResultViewModel<List<Category>>(categories));
        }
        catch(Exception e)
        {
            return StatusCode(500, "05XE16 - Falha interna no servidor");
        }
    }

    [HttpGet("v1/categories/{id:int}")]
    public async Task<IActionResult> GetByIdAsync([FromRoute]int id,[FromServices] BlogDataContext context)
    {
        try
        {
            var category = await context.Categories.FirstOrDefaultAsync(x => x.Id == id);
            if(category == null)
                return NotFound(new ResultViewModel<Category>(error:"Conteúdo não encontrado"));

            return Ok(new ResultViewModel<Category>(category));
        }
        catch(Exception e)
        {
            return StatusCode(500, value: new ResultViewModel<Category>(error:"Falha Interna no servidor"));
        }
    }

    [HttpPost("v1/categories")]
    public async Task<IActionResult> PostAsync([FromBody] EditorCategoryViewModel model, [FromServices] BlogDataContext context)
    {
       //Essa validação não é obrigatória, pois, já é feita automaticamente, mas, caso precise acessar é desta forma abaixo 
       if(!ModelState.IsValid)
        return BadRequest(error: ModelState.GetErrors());

       try
       {
            var category = new Category{ Id = 0,Posts = [],Name = model.Name,Slug = model.Slug.ToLower()};

            await context.Categories.AddAsync(category);
            await context.SaveChangesAsync();

            return Created($"v1/categories/{category.Id}", value: new ResultViewModel<Category>(category));
       }
       catch(DbUpdateException e)
       {
            return StatusCode(500, value: new ResultViewModel<Category>("05XE9 - Não foi possível incluir a categoria"));
       }
       catch(Exception e)
       {
            return StatusCode(500, value: new ResultViewModel<Category>("05XE10 - Falha interna no servidor"));
       }
    }   

    [HttpPut("v1/categories/{id:int}")]
    public async Task<IActionResult> PutAsync([FromRoute]int id,[FromBody] EditorCategoryViewModel model, [FromServices] BlogDataContext context)
    {
        var category = await context.Categories.FirstOrDefaultAsync(x => x.Id == id);

        if(category == null)
            return NotFound(value: new ResultViewModel<Category>("Conteúdo não encontrado"));

        category.Name = model.Name;
        category.Slug = model.Slug;

        try
        {
            context.Categories.Update(category);
            await context.SaveChangesAsync();
        }
        catch(DbUpdateException e)
        {
                return StatusCode(500, value: new ResultViewModel<Category>("05XE11 - Não foi possível incluir a categoria"));
        }
        catch(Exception e)
        {
                return StatusCode(500, value: new ResultViewModel<Category>("05XE12 - Falha interna no servidor"));
        }

        return Ok(value: new ResultViewModel<Category>(category));
    } 

    [HttpDelete("v1/categories/{id:int}")]
    public async Task<IActionResult> DeleteAsync([FromRoute]int id, [FromServices] BlogDataContext context)
    {
        var category = await context.Categories.FirstOrDefaultAsync(x => x.Id == id);

        if(category == null)
            return NotFound(value: new ResultViewModel<Category>("Conteúdo não encontrado"));

        try
        {
            context.Categories.Remove(category);
            await context.SaveChangesAsync();
        }
        catch(DbUpdateException e)
        {
                return StatusCode(500,value: new ResultViewModel<Category>("05XE13 - Não foi possível incluir a categoria"));
        }
        catch(Exception e)
        {
                return StatusCode(500, value: new ResultViewModel<Category>("05XE14 - Falha interna no servidor"));
        }

        return Ok(category);
    } 

    private List<Category> GetCategories(BlogDataContext context)
    {
        return context.Categories.ToList();
    }
}