using Microsoft.EntityFrameworkCore;
using SurveillanceAPI.Domains;
using SurveillanceAPI.Inteface;
using SurveillanceAPI.Interface;

namespace SurveillanceAPI.Infraestructure.Persistence;

public static class DbSeeder
{
    /// <summary>
    /// Cria um administrador inicial (só em Development) a partir de Seed:AdminEmail / Seed:AdminSenha.
    /// Os valores vêm do user-secrets, nunca de arquivo versionado.
    /// </summary>
    public static async Task SeedAdminAsync(IServiceProvider services, IConfiguration config)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var hasher = scope.ServiceProvider.GetRequiredService<IHashSenha>();
        var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("DbSeeder");

        var email = config["Seed:AdminEmail"];
        var senha = config["Seed:AdminSenha"];

        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(senha))
        {
            logger.LogInformation("Seed do administrador ignorado: Seed:AdminEmail / Seed:AdminSenha não configurados.");
            return;
        }

        if ((await db.Database.GetPendingMigrationsAsync()).Any())
        {
            logger.LogWarning("Seed ignorado: há migrations pendentes. Rode 'dotnet ef database update'.");
            return;
        }

        var emailNormalizado = email.Trim().ToLowerInvariant();

        if (await db.Usuarios.AnyAsync(u => u.Email == emailNormalizado))
            return;

        var admin = new Usuario(
            config["Seed:AdminNome"] ?? "Administrador",
            emailNormalizado,
            hasher.Hash(senha),
            (int)TipoUsuarioEnum.Administrador);

        db.Usuarios.Add(admin);
        await db.SaveChangesAsync();

        logger.LogInformation("Administrador inicial criado: {Email}", emailNormalizado);
    }
}