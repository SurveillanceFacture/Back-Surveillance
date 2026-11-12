using SurveillanceAPI.Domains;
using SurveillanceAPI.Dto;
using SurveillanceAPI.Dto.UsuarioDto;
using SurveillanceAPI.Exceptions;
using SurveillanceAPI.Inteface;
using SurveillanceAPI.Interface;

namespace SurveillanceAPI.Applications.Service;

public class UsuarioService
{
    private readonly IUsuarioRepository _repository;
    private readonly IHashSenha _hasher;

    public UsuarioService(IUsuarioRepository repository, IHashSenha hasher)
    {
        _repository = repository;
        _hasher = hasher;
    }

    public async Task<LerUsuarioDto> CriarAsync(CriarUsuarioDto dto, CancellationToken ct)
    {
        ValidarTipo(dto.TipoUsuarioId);

        var email = Normalizar(dto.Email);
        if (await _repository.EmailExisteAsync(email, null, ct))
            throw new DomainException("E-mail já cadastrado.");

        var usuario = new Usuario(dto.NomeUsuario, email, _hasher.Hash(dto.Senha), dto.TipoUsuarioId);

        _repository.Adicionar(usuario);
        await _repository.SalvarAsync(ct);

        return Mapear(usuario);
    }

    public async Task<PaginaDto<LerUsuarioDto>> ListarAsync(FiltroUsuarioDto filtro, CancellationToken ct)
    {
        var (itens, total) = await _repository.ListarAsync(
            filtro.Busca, filtro.TipoUsuarioId, filtro.Ativo, filtro.Pagina, filtro.Tamanho, ct);

        return new PaginaDto<LerUsuarioDto>
        {
            Itens = itens.Select(Mapear).ToList(),
            Pagina = filtro.Pagina,
            Tamanho = filtro.Tamanho,
            Total = total
        };
    }

    public async Task<LerUsuarioDto> ObterAsync(Guid id, CancellationToken ct)
    {
        var usuario = await ObterOuFalharAsync(id, ct);
        return Mapear(usuario);
    }

    public async Task<LerUsuarioDto> AtualizarAsync(
        Guid id, AtualizarUsuarioDto dto, Guid usuarioLogadoId, CancellationToken ct)
    {
        var usuario = await ObterOuFalharAsync(id, ct);

        ValidarTipo(dto.TipoUsuarioId);

        // Evita o administrador se rebaixar e deixar o sistema sem ninguém com acesso total
        if (id == usuarioLogadoId && dto.TipoUsuarioId != usuario.TipoUsuarioId)
            throw new DomainException("Você não pode alterar o seu próprio tipo de usuário.");

        var email = Normalizar(dto.Email);
        if (email != usuario.Email && await _repository.EmailExisteAsync(email, id, ct))
            throw new DomainException("E-mail já cadastrado.");

        usuario.AlterarNome(dto.NomeUsuario);
        usuario.AlterarEmail(email);
        usuario.AlterarTipoUsuario(dto.TipoUsuarioId);

        await _repository.SalvarAsync(ct);

        return Mapear(usuario);
    }

    /// <summary>"Excluir" = desativar. O usuário tem histórico (lojas, comparações, divergências).</summary>
    public async Task DesativarAsync(Guid id, Guid usuarioLogadoId, CancellationToken ct)
    {
        if (id == usuarioLogadoId)
            throw new DomainException("Você não pode desativar a si mesmo.");

        var usuario = await ObterOuFalharAsync(id, ct);
        usuario.Desativar();
        await _repository.SalvarAsync(ct);
    }

    public async Task ReativarAsync(Guid id, CancellationToken ct)
    {
        var usuario = await ObterOuFalharAsync(id, ct);
        usuario.Ativar();
        await _repository.SalvarAsync(ct);
    }

    public async Task RedefinirSenhaAsync(Guid id, RedefinirSenhaDto dto, CancellationToken ct)
    {
        var usuario = await ObterOuFalharAsync(id, ct);
        usuario.AlterarSenha(_hasher.Hash(dto.NovaSenha));
        await _repository.SalvarAsync(ct);
    }

    public async Task AlterarMinhaSenhaAsync(Guid usuarioLogadoId, AlterarSenhaDto dto, CancellationToken ct)
    {
        var usuario = await ObterOuFalharAsync(usuarioLogadoId, ct);

        if (!_hasher.Validar(usuario.HashSenha, dto.SenhaAtual))
            throw new DomainException("Senha atual incorreta.");

        if (dto.SenhaAtual == dto.NovaSenha)
            throw new DomainException("A nova senha deve ser diferente da atual.");

        usuario.AlterarSenha(_hasher.Hash(dto.NovaSenha));
        await _repository.SalvarAsync(ct);
    }

    // ----------------- auxiliares -----------------

    private async Task<Usuario> ObterOuFalharAsync(Guid id, CancellationToken ct) =>
        await _repository.ObterPorIdAsync(id, ct)
        ?? throw new DomainException("Usuário não encontrado.");

    private static string Normalizar(string email) => email.Trim().ToLowerInvariant();

    // Os tipos são semeados a partir do enum, então o enum é a fonte da verdade
    private static void ValidarTipo(int tipoUsuarioId)
    {
        if (!Enum.IsDefined((TipoUsuarioEnum)tipoUsuarioId))
            throw new DomainException("Tipo de usuário inválido.");
    }

    private static LerUsuarioDto Mapear(Usuario u) => new()
    {
        UsuarioId = u.UsuarioId,
        NomeUsuario = u.NomeUsuario,
        Email = u.Email,
        TipoUsuarioId = u.TipoUsuarioId,
        TipoUsuario = u.TipoUsuario?.NomeTipoUsuario ?? ((TipoUsuarioEnum)u.TipoUsuarioId).ToString(),
         Status= u.StatusUsuario,
        DataCriacao = u.DataCriacao,
        UltimoAcesso = u.UltimoAcesso
    };
}
