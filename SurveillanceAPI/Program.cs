using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using SurveillanceAPI.Applications.Authentication;
using SurveillanceAPI.Applications.Service;
using SurveillanceAPI.Applications.Service.Auditoria;
using SurveillanceAPI.Infraestructure.Persistence;
using SurveillanceAPI.Infraestructure.Persistence.Repositories;
using SurveillanceAPI.Inteface;
using SurveillanceAPI.Interface;
using SurveillanceAPI.Infraestructure.OpenApi;
using SurveillanceAPI.Infrastructure.ExceptionHandling;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));


var jwt = builder.Configuration.GetSection(ConfiguracoesJwt.SectionName).Get<ConfiguracoesJwt>() ?? new ConfiguracoesJwt();
jwt.Validar();
builder.Services.AddSingleton(jwt);

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.Key)),
            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddAuthorization();

builder.Services.AddSingleton<IHashSenha, HashSenha>();
builder.Services.AddSingleton<IGeradorTokenJwt, GeradorTokenJwt>();
builder.Services.AddScoped<IUsuarioRepository, UsuarioRepository>();
builder.Services.AddScoped<AutenticacaoService>();
builder.Services.AddScoped<UsuarioService>();
builder.Services.AddScoped<LeitorArquivosService>(); ;


builder.Services.AddExceptionHandler<DomainExceptionHandler>();
builder.Services.AddProblemDetails();


builder.Services.AddControllers();
builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer<BearerSecuritySchemeTransformer>();
});

var app = builder.Build();

app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/openapi/v1.json", "SurveillanceAPI v1");
    });
    await DbSeeder.SeedAdminAsync(app.Services, app.Configuration);
}

app.UseHttpsRedirection();

app.UseAuthentication();  

app.Use(async (context, next) =>
{
    var pendente = context.User.HasClaim(GeradorTokenJwt.ClaimDeveTrocarSenha, "true");

    if (pendente && !RotaPermitidaComSenhaPendente(context.Request))
    {
        context.Response.StatusCode = StatusCodes.Status403Forbidden;
        await context.Response.WriteAsJsonAsync(new ProblemDetails
        {
            Status = StatusCodes.Status403Forbidden,
            Title = "Troca de senha obrigatória",
            Detail = "Você está com uma senha temporária. Altere-a em PUT /api/usuarios/me/senha para continuar."
        });
        return;
    }

    await next();
});


app.UseAuthorization();

app.MapControllers();

app.Run();


static bool RotaPermitidaComSenhaPendente(HttpRequest r)
{
    var caminho = r.Path.Value?.TrimEnd('/') ?? "";

    return (HttpMethods.IsPut(r.Method) && caminho.Equals("/api/usuarios/me/senha", StringComparison.OrdinalIgnoreCase))
           || (HttpMethods.IsGet(r.Method) && caminho.Equals("/api/usuarios/me", StringComparison.OrdinalIgnoreCase));
}