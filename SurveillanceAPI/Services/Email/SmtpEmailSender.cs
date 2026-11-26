using MailKit;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;
using SurveillanceAPI.Interface;

namespace SurveillanceAPI.Services.Email;

/// <summary>
/// SMTP genérico (Gmail, Outlook/Microsoft 365, Zoho, Brevo, SendGrid, Mailgun, SES, Postmark...).
/// SecureSocketOptions.Auto escolhe sozinho: porta 465 = TLS direto; 587/25 = STARTTLS.
/// </summary>
public class SmtpEmailSender : IEmailSender
{
    private readonly EmailOptions _options;

    public SmtpEmailSender(IOptions<EmailOptions> options)
    {
        _options = options.Value;
    }

    public async Task EnviarAsync(string para, string assunto, string corpoTexto, CancellationToken ct)
    {
        var mensagem = new MimeMessage();
        mensagem.From.Add(new MailboxAddress(_options.NomeRemetente, _options.Remetente));
        mensagem.To.Add(MailboxAddress.Parse(para));
        mensagem.Subject = assunto;
        mensagem.Body = new TextPart("plain") { Text = corpoTexto };

        using var client = new SmtpClient { Timeout = 15_000 };
        await client.ConnectAsync(_options.Host, _options.Porta, SecureSocketOptions.Auto, ct);

        if (!string.IsNullOrWhiteSpace(_options.Usuario))
            await client.AuthenticateAsync(_options.Usuario, _options.Senha, ct);

        await client.SendAsync(mensagem, ct);
        await client.DisconnectAsync(true, ct);
    }
}

/// <summary>
/// Usado só em Development: escreve o e-mail (com o link) no log em vez de enviar.
/// </summary>
public class LogEmailSender : IEmailSender
{
    private readonly ILogger<LogEmailSender> _logger;

    public LogEmailSender(ILogger<LogEmailSender> logger)
    {
        _logger = logger;
    }

    public Task EnviarAsync(string para, string assunto, string corpoTexto, CancellationToken ct)
    {
        _logger.LogWarning("Provedor de e-mail 'log'. E-mail para {Para} | {Assunto}\n{Corpo}", para, assunto, corpoTexto);
        return Task.CompletedTask;
    }
}