using System.Security.Claims;
using System.Text;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using SurveillanceAPI.Applications.Authentication;
using SurveillanceAPI.Applications.Service;
using SurveillanceAPI.Domains;
using SurveillanceAPI.Infraestructure.Persistence;
using SurveillanceAPI.Infraestructure.Persistence.Repositories;
using SurveillanceAPI.Inteface;
using SurveillanceAPI.Interface;
using SurveillanceAPI.Interfaces;
using SurveillanceAPI.Services.Regras;
using SurveillanceAPI.Infraestructure.OpenApi;
using SurveillanceAPI.Infrastructure.ExceptionHandling;
using SurveillanceAPI.Infrastructure.Persistence.Repository;
using SurveillanceAPI.Services.Email;

var builder = WebApplication.CreateBuilder(args);

// Em hospedagem na nuvem a API fica atrás de proxy/load balancer. Sem isto, RemoteIpAddress é o IP do proxy
// e o rate limit passa a valer para TODOS os usuários juntos. Só habilite se a API NÃO for exposta direto à internet.
var usarForwardedHeaders = builder.Configuration.GetValue<bool>("ForwardedHeaders:Habilitado");
if (usarForwardedHeaders)
{
    builder.Services.Configure<ForwardedHeadersOptions>(o =>
    {
        o.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
        o.ForwardLimit = 1;
        o.KnownNetworks.Clear();
        o.KnownProxies.Clear();
    });
}

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
        options.Events = new JwtBearerEvents
        {
            OnTokenValidated = async ctx =>
            {
                var p = ctx.Principal!;
                var repo = ctx.HttpContext.RequestServices.GetRequiredService<IUsuarioRepository>();

                var u = Guid.TryParse(p.FindFirstValue(ClaimTypes.NameIdentifier), out var id)
                    ? await repo.ObterPorIdAsync(id, ctx.HttpContext.RequestAborted)
                    : null;

                var pendenteNoToken = p.HasClaim(GeradorTokenJwt.ClaimDeveTrocarSenha, "true");
                var emitidoEm = (ctx.SecurityToken as JsonWebToken)?.IssuedAt;

                if (u is null
                    || !u.StatusUsuario
                    || u.TipoUsuario.NomeTipoUsuario != p.FindFirstValue(ClaimTypes.Role)
                    || u.DeveTrocarSenha != pendenteNoToken
                    || SenhaAlteradaDepoisDoToken(u, emitidoEm))
                {
                    ctx.Fail("Token não é mais válido.");
                }
            }
        };
    });

builder.Services.AddAuthorization();

builder.Services.AddSingleton<IHashSenha, HashSenha>();
builder.Services.AddSingleton<IGeradorTokenJwt, GeradorTokenJwt>();
builder.Services.AddScoped<IUsuarioRepository, UsuarioRepository>();
builder.Services.AddScoped<ILojaRepository, LojaRepository>();
builder.Services.AddEmailSender(builder.Configuration, builder.Environment);

if (!builder.Environment.IsDevelopment()
    && string.IsNullOrWhiteSpace(builder.Configuration[$"{RedefinicaoSenhaOptions.SectionName}:UrlBase"]))
    throw new InvalidOperationException("RedefinicaoSenha:UrlBase não configurado.");

builder.Services.AddScoped<AutenticacaoService>();
builder.Services.AddScoped<UsuarioService>();
builder.Services.AddScoped<LojaService>();


builder.Services.AddScoped<IConciliacaoRegra, ValorUnitarioRegra>();
builder.Services.AddScoped<IConciliacaoRegra, QuantidadeRegra>();
builder.Services.AddScoped<IConciliacaoRegra, NcmRegra>();
builder.Services.AddScoped<IConciliacaoRegra, UnidadeMedidaRegra>();
builder.Services.AddScoped<IConciliacaoRegra, CfopRegra>();
builder.Services.AddScoped<IConciliacaoRegra, CodigoProdutoRegra>();


builder.Services.AddScoped<ConciliacaoEngine>();
builder.Services.AddExceptionHandler<DomainExceptionHandler>();
builder.Services.AddProblemDetails();


builder.Services.AddControllers();

var origens = builder.Configuration.GetSection("Cors:Origins").Get<string[]>() ?? [];
if (!builder.Environment.IsDevelopment() && origens.Length == 0)
    throw new InvalidOperationException("Cors:Origins não configurado.");

builder.Services.AddCors(o => o.AddDefaultPolicy(p => p
    .WithOrigins(origens)
    .WithHeaders("Authorization", "Content-Type")
    .WithMethods("GET", "POST", "PUT", "DELETE")
    .SetPreflightMaxAge(TimeSpan.FromMinutes(10))));


builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer<BearerSecuritySchemeTransformer>();
});

builder.Services.AddRateLimiter(o =>
{
    o.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    // Uma política por endpoint: cada uma tem o seu próprio balde por IP.
    AdicionarPoliticaPorIp(o, "login", permitir: 5, janela: TimeSpan.FromMinutes(1));
    AdicionarPoliticaPorIp(o, "esqueci-senha", permitir: 3, janela: TimeSpan.FromMinutes(10));
    AdicionarPoliticaPorIp(o, "redefinir-senha", permitir: 10, janela: TimeSpan.FromMinutes(10));
});

var app = builder.Build();

if (usarForwardedHeaders)
    app.UseForwardedHeaders();

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

app.UseCors();

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

app.UseRateLimiter();

app.MapControllers();

app.Run();


static void AdicionarPoliticaPorIp(RateLimiterOptions o, string nome, int permitir, TimeSpan janela) =>
    o.AddPolicy(nome, ctx => RateLimitPartition.GetFixedWindowLimiter(
        ctx.Connection.RemoteIpAddress?.ToString() ?? "anon",
        _ => new FixedWindowRateLimiterOptions
        {
            PermitLimit = permitir,
            Window = janela
        }));

// Tokens emitidos antes da última troca de senha deixam de valer.
static bool SenhaAlteradaDepoisDoToken(Usuario u, DateTime? emitidoEm)
{
    if (u.SenhaAlteradaEm is not { } alteradaEm)
        return false;

    // Sem 'iat' não há como provar que o token é posterior à troca: nega.
    if (emitidoEm is null)
        return true;

    // O 'iat' tem precisão de segundos; trunca a data da troca para não derrubar
    // um token emitido no mesmo segundo, logo depois da troca.
    var limite = alteradaEm.Ticks - alteradaEm.Ticks % TimeSpan.TicksPerSecond;
    return emitidoEm.Value.Ticks < limite;
}

static bool RotaPermitidaComSenhaPendente(HttpRequest r)
{
    var caminho = r.Path.Value?.TrimEnd('/') ?? "";

    return (HttpMethods.IsPut(r.Method) && caminho.Equals("/api/usuarios/me/senha", StringComparison.OrdinalIgnoreCase))
           || (HttpMethods.IsGet(r.Method) && caminho.Equals("/api/usuarios/me", StringComparison.OrdinalIgnoreCase));
}