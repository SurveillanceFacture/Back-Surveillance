namespace SurveillanceAPI.Services.Email;

public class EmailOptions
{
    public const string SectionName = "Email";

    /// <summary>smtp | resend | sendgrid | brevo | mailgun | postmark | log (log só em Development).</summary>
    public string Provedor { get; set; } = string.Empty;

    public string Remetente { get; set; } = string.Empty;
    public string NomeRemetente { get; set; } = "SurveillanceAPI";

    // SMTP
    public string Host { get; set; } = string.Empty;
    public int Porta { get; set; } = 587;
    public string Usuario { get; set; } = string.Empty;
    public string Senha { get; set; } = string.Empty;

    // Provedores por API HTTP
    public string ApiKey { get; set; } = string.Empty;

    // Mailgun
    public string Dominio { get; set; } = string.Empty;
    public string Regiao { get; set; } = "us"; // us | eu
}

public class RedefinicaoSenhaOptions
{
    public const string SectionName = "RedefinicaoSenha";

    /// <summary>URL da tela do front que recebe ?token=... (ex.: https://app.exemplo.com/redefinir-senha)</summary>
    public string UrlBase { get; set; } = string.Empty;

    public int ValidadeMinutos { get; set; } = 30;

    /// <summary>Intervalo mínimo entre dois e-mails de redefinição para a mesma conta.</summary>
    public int CooldownSegundos { get; set; } = 60;
}