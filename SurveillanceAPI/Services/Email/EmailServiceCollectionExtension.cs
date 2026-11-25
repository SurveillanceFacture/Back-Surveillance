using SurveillanceAPI.Interface;

namespace SurveillanceAPI.Services.Email;

public static class EmailServiceCollectionExtensions
{
    public static IServiceCollection AddEmailSender(
        this IServiceCollection services, IConfiguration config, IHostEnvironment env)
    {
        services.Configure<EmailOptions>(config.GetSection(EmailOptions.SectionName));
        services.Configure<RedefinicaoSenhaOptions>(config.GetSection(RedefinicaoSenhaOptions.SectionName));

        var o = config.GetSection(EmailOptions.SectionName).Get<EmailOptions>() ?? new EmailOptions();
        var provedor = o.Provedor.Trim().ToLowerInvariant();

        // Compatibilidade: sem Provedor, mas com Host => SMTP. Sem nada, só Development usa "log".
        if (provedor.Length == 0)
            provedor = !string.IsNullOrWhiteSpace(o.Host) ? "smtp" : env.IsDevelopment() ? "log" : "";

        if (provedor.Length == 0)
            throw new InvalidOperationException(
                "Email:Provedor não configurado. Use: smtp, resend, sendgrid, brevo, mailgun ou postmark.");

        if (provedor == "log")
        {
            if (!env.IsDevelopment())
                throw new InvalidOperationException("Email:Provedor 'log' só é permitido em Development.");
            services.AddScoped<IEmailSender, LogEmailSender>();
            return services;
        }

        Exigir(o.Remetente, "Email:Remetente");

        switch (provedor)
        {
            case "smtp":
                Exigir(o.Host, "Email:Host");
                services.AddScoped<IEmailSender, SmtpEmailSender>();
                break;
            case "resend":
                Exigir(o.ApiKey, "Email:ApiKey");
                services.AddHttpClient<IEmailSender, ResendEmailSender>(Configurar);
                break;
            case "sendgrid":
                Exigir(o.ApiKey, "Email:ApiKey");
                services.AddHttpClient<IEmailSender, SendGridEmailSender>(Configurar);
                break;
            case "brevo":
                Exigir(o.ApiKey, "Email:ApiKey");
                services.AddHttpClient<IEmailSender, BrevoEmailSender>(Configurar);
                break;
            case "mailgun":
                Exigir(o.ApiKey, "Email:ApiKey");
                Exigir(o.Dominio, "Email:Dominio");
                services.AddHttpClient<IEmailSender, MailgunEmailSender>(Configurar);
                break;
            case "postmark":
                Exigir(o.ApiKey, "Email:ApiKey");
                services.AddHttpClient<IEmailSender, PostmarkEmailSender>(Configurar);
                break;
            default:
                throw new InvalidOperationException(
                    $"Email:Provedor '{provedor}' inválido. Use: smtp, resend, sendgrid, brevo, mailgun ou postmark.");
        }

        return services;
    }

    private static void Configurar(HttpClient client) => client.Timeout = TimeSpan.FromSeconds(15);

    private static void Exigir(string valor, string chave)
    {
        if (string.IsNullOrWhiteSpace(valor))
            throw new InvalidOperationException($"{chave} não configurado.");
    }
}
