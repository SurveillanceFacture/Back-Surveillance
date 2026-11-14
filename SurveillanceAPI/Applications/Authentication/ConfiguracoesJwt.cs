using System.Text;

namespace SurveillanceAPI.Applications.Authentication;

public class ConfiguracoesJwt
{
    public const string SectionName = "Jwt";

    public string Key { get; set; } = string.Empty;
    public string Issuer { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;
    public int ExpiracaoMinutos { get; set; } = 20;

    public void Validar()
    {
        if (Encoding.UTF8.GetByteCount(Key) < 32)
        {
            throw new InvalidOperationException("Jwt:Key não configurada ou com menos de 32 bytes. " +
                                                "Defina com: dotnet user-secrets set \"Jwt:Key\" \"<chave>\"");
        }

        if (string.IsNullOrWhiteSpace(Issuer))
        {
            throw new InvalidOperationException("Jwt:Issuer não configurado.");
        }

        if (string.IsNullOrWhiteSpace(Audience))
        {
            throw new InvalidOperationException("Jwt:Audience não configurado.");
        }

        if (ExpiracaoMinutos <= 0)
        {
            throw new InvalidOperationException("Jwt:ExpiracaoMinutos deve ser maior que zero.");
        }
    }

}
