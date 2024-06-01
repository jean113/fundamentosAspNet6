using System.IO.Compression;
using System.Text;
using System.Text.Json.Serialization;
using Blog;
using Blog.Data;
using Blog.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);
ConfigureAuthentication(builder);
ConfigureMvc(builder);
ConfigureServices(builder);

builder.Services.AddEndpointsApiExplorer(); //faz o swagger ser adicionado
builder.Services.AddSwaggerGen(); //responsável por gerar o código html do Swagger


var app = builder.Build();
LoadConfiguration(app);


//sempre nesse ordem
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.UseStaticFiles(); //renderiza arquivos estaticos como jpg, imagens, html e o servidor vai sempre procurar na pasta wwwroot - não é recomendado manter arquivos estaticos no servidor
app.UseResponseCompression();

if(app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}


app.Run();

//separando em funções para organizar o código
void LoadConfiguration(WebApplication app)
{
    // app.Configuration.GetSection(); //obtém um sessão do apssetings
    Configuration.JwtKey = app.Configuration.GetValue<string>("JwtKey");
    Configuration.ApiKeyName = app.Configuration.GetValue<string>("ApiKeyName");
    Configuration.ApiKey = app.Configuration.GetValue<string>("ApiKey");

    var smtp = new Configuration.SmtpConfiguration();
    app.Configuration.GetSection("Smtp").Bind(smtp); // o bind converte p Json e preenche os campos da classe a ser populada
    Configuration.Stmp = smtp;
}

void ConfigureAuthentication(WebApplicationBuilder builder)
{
    //configurações para autenticar e autorizar a api
    var key = Encoding.ASCII.GetBytes(Configuration.JwtKey);
    builder.Services.AddAuthentication(x => 
    {
        x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    }).AddJwtBearer(x => 
    {
        x.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(key),
            ValidateIssuer = false,
            ValidateAudience = false,
        };
    });
}

void ConfigureMvc(WebApplicationBuilder builder)
{
    builder.Services.AddMemoryCache(); // suporte a cache em memoria

    builder.Services.AddResponseCompression(options => // comprimir os dados
    {
        options.Providers.Add<GzipCompressionProvider>();
        // options.Providers.Add<BrotliCompressionProvider>();
        // options.Providers.Add<CustomCompressionProvider>();
    });
    builder.Services.Configure<GzipCompressionProviderOptions>(options =>
    {
        options.Level = CompressionLevel.Optimal;
        // options.Providers.Add<CustomCompressionProvider>();
    });

    builder
        .Services
        .AddControllers()
        .ConfigureApiBehaviorOptions(Options => { Options.SuppressModelStateInvalidFilter = true; })
        .AddJsonOptions( x => {
            x.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles; //ignore ciclos subsequentes dos objetos
            x.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault; //quando o objeto for nulo, ele não será renderizado na tela
        });
}

void ConfigureServices(WebApplicationBuilder builder)
{
    builder.Services.AddDbContext<BlogDataContext>(); // se vc estiver usando dbcontext sempre use esta função
    builder.Services.AddTransient<TokenService>(); //sempre vai criar uma nova instancia
    builder.Services.AddTransient<EmailService>(); 
    // builder.Services.AddScoped(); // vai durar por transação - sempre que a rota é usada, vai criar uma instancia, ao finalizar a rota, a instancia será destruída
    // builder.Services.AddSingleton(); //1 por app!
}
