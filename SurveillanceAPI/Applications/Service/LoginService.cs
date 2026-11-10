using SurveillanceAPI.Dto;
using SurveillanceAPI.Exceptions;
using SurveillanceAPI.Inteface;
using SurveillanceAPI.Interface;

namespace SurveillanceAPI.Applications.Service;

public class LoginService
{
    private readonly IUsuarioRepository _repository;
    private readonly IHashSenha _hasher;
    private readonly IGeradorTokenJwt _tokenGenerator;

    public LoginService(
        IUsuarioRepository repository,
        IHashSenha hasher,
        IGeradorTokenJwt tokenGenerator)
    {
        _repository = repository;
        _hasher = hasher;
        _tokenGenerator = tokenGenerator;
    }

    public async Task<LerLoginDto> LoginAsync(CriarLoginDto dto, CancellationToken ct)
    {
        var email = dto.Email.Trim().ToLowerInvariant();

        var usuario = await _repository.ObterPorEmailAsync(email, ct);

        if (usuario is null)
        {
            _hasher.SimularValidacao(dto.Senha);
            throw new DomainException("E-mail ou senha inválidos.");
        }
        
        var senhaOk = _hasher.Validar(usuario.HashSenha, dto.Senha);
        if (!senhaOk || !usuario.StatusUsuario)
            throw new DomainException("E-mail ou senha inválidos.");

        usuario.RegistrarAcesso();
        await _repository.SalvarAsync(ct);

        var token = _tokenGenerator.Gerar(usuario);

        return new LerLoginDto
        {
            Token = token.Token,
            ExpiraEm = token.ExpiraEm,
            UsuarioId = usuario.UsuarioId,
            Nome = usuario.NomeUsuario,
            TipoUsuario = usuario.TipoUsuario.NomeTipoUsuario
        };
    }
}