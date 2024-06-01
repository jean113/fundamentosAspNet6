using Blog.Data;
using Blog.Models;
using Blog.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Blog.Controllers;

[ApiController]
[Route("")]
public class PostController : ControllerBase
{
    [HttpGet("v1/posts")]
    public async Task<IActionResult> GetAsync(
        [FromServices] BlogDataContext context,
        [FromQuery] int page = 0,
        [FromQuery] int pageSize = 25)
    {
        //1 forma
        // var posts = await context.Posts
        //     .AsNoTracking()
        //     .Select(x => new { x.Id, x.Title})
        //     .ToListAsync();

        //2 forma
        try
        {
            var count = await context.Posts.AsNoTracking().CountAsync(); //para paginação no frontend
            var posts = await context.Posts
                .AsNoTracking()
                .Include(x => x.Category)
                .Include(x => x.Author)
                .Select(x => new ListPostsViewModel
                {
                    Id = x.Id,
                    Title = x.Title,
                    Slug = x.Slug,
                    LastUpdateDate = x.LastUpdateDate,
                    Category = x.Category.Name,
                    Author = $"{x.Author.Name} ({x.Author.Email})"
                })
                //paginação
                .Skip(page * pageSize)
                .Take(pageSize)
                //paginação
                .OrderByDescending(x => x.LastUpdateDate)
                .ToListAsync();

            // return Ok(posts);
            return Ok(new ResultViewModel<dynamic>(new 
            {
                total = count,
                page,
                pageSize,
                posts
            }));
        }
       catch(Exception e)
       {
            return StatusCode(500, value: new ResultViewModel<List<Post>>("05XE25 - Falha interna no servidor"));
       }
    }

    [HttpGet("v1/posts/{id:int}")]
    public async Task<IActionResult> DetailsAsync(
        [FromServices] BlogDataContext context,
        [FromRoute] int id)
    {
        try
        {
            var post = await context.Posts
                .AsNoTracking()
                .Include(x => x.Category)
                .Include(x => x.Author)
                .ThenInclude(x => x.Roles)
               .FirstOrDefaultAsync(x => x.Id == id);

            if (post == null)
                return NotFound(new ResultViewModel<Post>("Counteúdo não encontrado"));

            return Ok(new ResultViewModel<Post>(post));      
        }
       catch(Exception e)
       {
            return StatusCode(500, value: new ResultViewModel<List<Post>>("05XE26 - Falha interna no servidor"));
       }
    }


    [HttpGet("v1/posts/category/{category}")]
    public async Task<IActionResult> GetByCategoryAsync(
        [FromServices] BlogDataContext context,
        [FromRoute] string category,
        [FromQuery] int page = 0,
        [FromQuery] int pageSize = 25)
    {
        try
        {
            var count = await context.Posts.AsNoTracking().CountAsync(); //para paginação no frontend
            var posts = await context.Posts
                .AsNoTracking()
                .Include(x => x.Author)
                .Include(x => x.Category)
                .Where(x => x.Category.Slug == category)
                .Select(x => new ListPostsViewModel
                {
                    Id = x.Id,
                    Title = x.Title,
                    Slug = x.Slug,
                    LastUpdateDate = x.LastUpdateDate,
                    Category = x.Category.Name,
                    Author = $"{x.Author.Name} ({x.Author.Email})"
                })
                //paginação
                .Skip(page * pageSize)
                .Take(pageSize)
                //paginação
                .OrderByDescending(x => x.LastUpdateDate)
                .ToListAsync();

            return Ok(new ResultViewModel<dynamic>(new 
            {
                total = count,
                page,
                pageSize,
                posts
            }));   
        }
       catch(Exception e)
       {
            return StatusCode(500, value: new ResultViewModel<List<Post>>("05XE27 - Falha interna no servidor"));
       }
    }
}