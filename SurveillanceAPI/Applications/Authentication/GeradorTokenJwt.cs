using System.Security.Claims;
using System.Text;
using System.Xml.Xsl;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using SurveillanceAPI.Domains;
using SurveillanceAPI.Interface;

namespace SurveillanceAPI.Applications.Authentication;

public record TokenGerado(string Token, DateTime ExpiraEm);

public class TokenJwtTokenGenerator : IGeradorTokenJwt {
    private readonly ConfiguracoesJwt _settings;
    private readonly SigningCredentials _credentials;

    public TokenJwtTokenGenerator(ConfiguracoesJwt settings)
    {
        _settings = settings;

        // Mesma chave e mesmo algoritmo que o AddJwtBearer usa para validar no Program.cs
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(settings.Key));
        _credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
    }

    public TokenGerado Gerar(Usuario usuario)
    {
        var agora = DateTime.UtcNow;
        var expiraEm = agora.AddMinutes(_settings.ExpiracaoMinutos);

        var descriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, usuario.UsuarioId.ToString()),
                new Claim(ClaimTypes.Name, usuario.NomeUsuario),
                new Claim(ClaimTypes.Email, usuario.Email),
                new Claim(ClaimTypes.Role, usuario.TipoUsuario.NomeTipoUsuario)
            }),
            Issuer = _settings.Issuer,
            Audience = _settings.Audience,
            IssuedAt = agora,
            NotBefore = agora,
            Expires = expiraEm,
            SigningCredentials = _credentials
        };

        var token = new JsonWebTokenHandler().CreateToken(descriptor);
        return new TokenGerado(token, expiraEm);
    }
}