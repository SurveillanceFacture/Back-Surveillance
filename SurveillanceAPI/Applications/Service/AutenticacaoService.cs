using System.Buffers.Text;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;
using SurveillanceAPI.Dto;
using SurveillanceAPI.Exceptions;
using SurveillanceAPI.Inteface;
using SurveillanceAPI.Interface;
using SurveillanceAPI.Services.Email;

namespace SurveillanceAPI.Applications.Service;

public class AutenticacaoService
{
    private readonly IUsuarioRepository _repository;
    private readonly IHashSenha _hasher;
    private readonly IGeradorTokenJwt _tokenGenerator;
    private readonly IEmailSender _email;
    private readonly RedefinicaoSenhaOptions _redefinicao;
    private readonly ILogger<AutenticacaoService> _logger;

    public AutenticacaoService(
        IUsuarioRepository repository,
        IHashSenha hasher,
        IGeradorTokenJwt tokenGenerator,
        IEmailSender email,
        IOptions<RedefinicaoSenhaOptions> redefinicao,
        ILogger<AutenticacaoService> logger)
    {
        _repository = repository;
        _hasher = hasher;
        _tokenGenerator = tokenGenerator;
        _email = email;
        _redefinicao = redefinicao.Value;
        _logger = logger;
    }

    /// <summary>
    /// Sempre retorna sem erro, exista o e-mail ou não, para não revelar quais e-mails estão cadastrados.
    /// </summary>
    public async Task SolicitarRedefinicaoAsync(EsqueciSenhaDto dto, CancellationToken ct)
    {
        var email = dto.Email.Trim().ToLowerInvariant();
        var usuario = await _repository.ObterPorEmailAsync(email, ct);

        if (usuario is null || !usuario.StatusUsuario)
            return;

        // Limite por conta (o rate limit por IP não protege a caixa de entrada de quem usa vários IPs):
        // o token atual foi gerado em (expiração - validade); se foi há pouco, ignora o pedido em silêncio.
        if (usuario.TokenRedefinicaoExpiraEm is { } expira
            && expira - TimeSpan.FromMinutes(_redefinicao.ValidadeMinutos)
               > DateTime.UtcNow - TimeSpan.FromSeconds(_redefinicao.CooldownSegundos))
            return;

        var token = Base64Url.EncodeToString(RandomNumberGenerator.GetBytes(32));
        usuario.GerarTokenRedefinicao(HashToken(token), TimeSpan.FromMinutes(_redefinicao.ValidadeMinutos));
        await _repository.SalvarAsync(ct);

        var link = $"{_redefinicao.UrlBase}?token={Uri.EscapeDataString(token)}";
        var corpo =
            $"Olá, {usuario.NomeUsuario}.\n\n" +
            "Recebemos um pedido para redefinir a sua senha. Use o link abaixo para criar uma nova:\n\n" +
            $"{link}\n\n" +
            $"O link vale por {_redefinicao.ValidadeMinutos} minutos e só pode ser usado uma vez.\n" +
            "Se você não pediu isso, ignore este e-mail: a sua senha continua a mesma.";

        try
        {
            await _email.EnviarAsync(usuario.Email, "Redefinição de senha", corpo, ct);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            // Não propaga: falhar aqui revelaria que o e-mail existe.
            _logger.LogError(ex, "Falha ao enviar e-mail de redefinição para o usuário {Id}.", usuario.UsuarioId);
        }
    }

    public async Task RedefinirComTokenAsync(RedefinirSenhaComTokenDto dto, CancellationToken ct)
    {
        var usuario = await _repository.ObterPorHashTokenRedefinicaoAsync(HashToken(dto.Token), ct);

        if (usuario is null || !usuario.TokenRedefinicaoValido() || !usuario.StatusUsuario)
            throw new DomainException("Link inválido ou expirado. Solicite uma nova redefinição.");

        usuario.AlterarSenha(_hasher.Hash(dto.NovaSenha)); // também invalida o token (uso único)
        await _repository.SalvarAsync(ct);
    }

    private static string HashToken(string token) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(token)));

    public async Task<LerLoginDto> LoginAsync(CriarLoginDto dto, CancellationToken ct)
    {
        var email = dto.Email.Trim().ToLowerInvariant();

        var usuario = await _repository.ObterPorEmailAsync(email, ct);

        if (usuario is null)
        {
            _hasher.SimularValidacao(dto.Senha);
            throw new DomainException.NaoAutorizadoException("E-mail ou senha inválidos.");
        }
        
        var senhaOk = _hasher.Validar(usuario.HashSenha, dto.Senha);
        if (!senhaOk || !usuario.StatusUsuario)
            throw new DomainException.NaoAutorizadoException("E-mail ou senha inválidos.");
        
        if (usuario.SenhaTemporariaExpirada())
            throw new DomainException.NaoAutorizadoException("A senha temporária expirou. Solicite uma nova ao administrador.");
        
        usuario.RegistrarAcesso();
        await _repository.SalvarAsync(ct);

        var token = _tokenGenerator.Gerar(usuario);

        return new LerLoginDto
        {
            Token = token.Token,
            ExpiraEm = token.ExpiraEm,
            UsuarioId = usuario.UsuarioId,
            Nome = usuario.NomeUsuario,
            TipoUsuario = usuario.TipoUsuario.NomeTipoUsuario,
            DeveTrocarSenha = usuario.DeveTrocarSenha
        };
    }
    
    public async Task<LerLoginDto> RenovarAsync(Guid usuarioId, CancellationToken ct)
    {
        var u = await _repository.ObterPorIdAsync(usuarioId, ct);
        if (u is null || !u.StatusUsuario || u.SenhaTemporariaExpirada())
            throw new DomainException.NaoAutorizadoException("Sessão inválida.");

        var token = _tokenGenerator.Gerar(u);
        return new LerLoginDto
        {
            Token = token.Token,
            ExpiraEm = token.ExpiraEm,
            UsuarioId = u.UsuarioId,
            Nome = u.NomeUsuario,
            TipoUsuario = u.TipoUsuario.NomeTipoUsuario,
            DeveTrocarSenha = u.DeveTrocarSenha
        };
    }
}