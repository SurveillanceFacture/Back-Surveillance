using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using Microsoft.Extensions.Options;
using SurveillanceAPI.Interface;

namespace SurveillanceAPI.Services.Email;

/// <summary>
/// Provedores que enviam por API HTTPS (porta 443). Funcionam mesmo em hospedagens que bloqueiam SMTP de saída.
/// </summary>
internal static class EmailHttp
{
    public static async Task EnviarAsync(HttpClient http, HttpRequestMessage req, string provedor, CancellationToken ct)
    {
        using var resp = await http.SendAsync(req, ct);
        if (resp.IsSuccessStatusCode) return;

        var corpo = await resp.Content.ReadAsStringAsync(ct);
        if (corpo.Length > 500) corpo = corpo[..500];
        throw new InvalidOperationException($"{provedor} respondeu {(int)resp.StatusCode}: {corpo}");
    }
}

public class ResendEmailSender : IEmailSender
{
    private readonly HttpClient _http;
    private readonly EmailOptions _o;

    public ResendEmailSender(HttpClient http, IOptions<EmailOptions> o) { _http = http; _o = o.Value; }

    public async Task EnviarAsync(string para, string assunto, string corpoTexto, CancellationToken ct)
    {
        using var req = new HttpRequestMessage(HttpMethod.Post, "https://api.resend.com/emails");
        req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _o.ApiKey);
        req.Content = JsonContent.Create(new
        {
            from = $"{_o.NomeRemetente} <{_o.Remetente}>",
            to = new[] { para },
            subject = assunto,
            text = corpoTexto
        });
        await EmailHttp.EnviarAsync(_http, req, "Resend", ct);
    }
}

public class SendGridEmailSender : IEmailSender
{
    private readonly HttpClient _http;
    private readonly EmailOptions _o;

    public SendGridEmailSender(HttpClient http, IOptions<EmailOptions> o) { _http = http; _o = o.Value; }

    public async Task EnviarAsync(string para, string assunto, string corpoTexto, CancellationToken ct)
    {
        using var req = new HttpRequestMessage(HttpMethod.Post, "https://api.sendgrid.com/v3/mail/send");
        req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _o.ApiKey);
        req.Content = JsonContent.Create(new
        {
            personalizations = new[] { new { to = new[] { new { email = para } } } },
            from = new { email = _o.Remetente, name = _o.NomeRemetente },
            subject = assunto,
            content = new[] { new { type = "text/plain", value = corpoTexto } }
        });
        await EmailHttp.EnviarAsync(_http, req, "SendGrid", ct);
    }
}

public class BrevoEmailSender : IEmailSender
{
    private readonly HttpClient _http;
    private readonly EmailOptions _o;

    public BrevoEmailSender(HttpClient http, IOptions<EmailOptions> o) { _http = http; _o = o.Value; }

    public async Task EnviarAsync(string para, string assunto, string corpoTexto, CancellationToken ct)
    {
        using var req = new HttpRequestMessage(HttpMethod.Post, "https://api.brevo.com/v3/smtp/email");
        req.Headers.Add("api-key", _o.ApiKey);
        req.Content = JsonContent.Create(new
        {
            sender = new { name = _o.NomeRemetente, email = _o.Remetente },
            to = new[] { new { email = para } },
            subject = assunto,
            textContent = corpoTexto
        });
        await EmailHttp.EnviarAsync(_http, req, "Brevo", ct);
    }
}

public class MailgunEmailSender : IEmailSender
{
    private readonly HttpClient _http;
    private readonly EmailOptions _o;

    public MailgunEmailSender(HttpClient http, IOptions<EmailOptions> o) { _http = http; _o = o.Value; }

    public async Task EnviarAsync(string para, string assunto, string corpoTexto, CancellationToken ct)
    {
        var host = string.Equals(_o.Regiao, "eu", StringComparison.OrdinalIgnoreCase)
            ? "api.eu.mailgun.net"
            : "api.mailgun.net";

        using var req = new HttpRequestMessage(HttpMethod.Post, $"https://{host}/v3/{_o.Dominio}/messages");
        req.Headers.Authorization = new AuthenticationHeaderValue(
            "Basic", Convert.ToBase64String(Encoding.UTF8.GetBytes($"api:{_o.ApiKey}")));
        req.Content = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["from"] = $"{_o.NomeRemetente} <{_o.Remetente}>",
            ["to"] = para,
            ["subject"] = assunto,
            ["text"] = corpoTexto
        });
        await EmailHttp.EnviarAsync(_http, req, "Mailgun", ct);
    }
}

public class PostmarkEmailSender : IEmailSender
{
    private readonly HttpClient _http;
    private readonly EmailOptions _o;

    public PostmarkEmailSender(HttpClient http, IOptions<EmailOptions> o) { _http = http; _o = o.Value; }

    public async Task EnviarAsync(string para, string assunto, string corpoTexto, CancellationToken ct)
    {
        using var req = new HttpRequestMessage(HttpMethod.Post, "https://api.postmarkapp.com/email");
        req.Headers.Add("X-Postmark-Server-Token", _o.ApiKey);
        // Dictionary: o Postmark exige nomes em PascalCase e o JSON padrão do .NET converteria para camelCase.
        req.Content = JsonContent.Create(new Dictionary<string, object>
        {
            ["From"] = $"{_o.NomeRemetente} <{_o.Remetente}>",
            ["To"] = para,
            ["Subject"] = assunto,
            ["TextBody"] = corpoTexto,
            ["MessageStream"] = "outbound"
        });
        await EmailHttp.EnviarAsync(_http, req, "Postmark", ct);
    }
}
