using SurveillanceAPI.Domains;
using SurveillanceAPI.Dto.LojaDto;
using SurveillanceAPI.Dto.UsuarioDto;
using SurveillanceAPI.Exceptions;
using SurveillanceAPI.Interface;

namespace SurveillanceAPI.Applications.Service;

public class LojaService
{
    private const string PerfilAdmin = nameof(TipoUsuarioEnum.Administrador);
    private const string PerfilAuditor = nameof(TipoUsuarioEnum.Auditor);
    private const string PerfilGerente = nameof(TipoUsuarioEnum.Gerente);
    private const int LimiteListaGerentes = 50;

    private readonly ILojaRepository _lojas;
    private readonly IUsuarioRepository _usuarios;

    public LojaService(ILojaRepository lojas, IUsuarioRepository usuarios)
    {
        _lojas = lojas;
        _usuarios = usuarios;
    }


    public async Task<LerLojaDto> CriarAsync(CriarLojaDto dto, Guid adminId, CancellationToken ct)
    {
        var cnpj = Loja.NormalizarCnpj(dto.Cnpj);
        if (!Loja.CnpjValido(cnpj))
            throw new DomainException("CNPJ invalido");
        if (await _lojas.CnpjExisteAsync(cnpj, null, ct))
            throw new DomainException.ConflitoException("Já existe uma loja com esse CNPJ.");

        var auditorId = dto.AuditorUsuarioId ?? Guid.Empty;
        var gerenteId = dto.GerenteUsuarioId ?? Guid.Empty;

        // Toda loja tem auditor E gerente
        await ValidarAuditorAsync(auditorId, ct);
        await ValidarGerenteAsync(gerenteId, null, ct);

        var loja = new Loja(dto.RazaoSocial, dto.Cnpj, auditorId, gerenteId, adminId);
        

        _lojas.Adicionar(loja);
        await _lojas.SalvarAsync(ct);

        return Mapear(await ObterOuFalharAsync(loja.LojaId, ct));   // recarrega para trazer os nomes
    }

    public async Task<LerLojaDto> AtualizarAsync(Guid id, AtualizarLojaDto dto, CancellationToken ct)
    {
        var loja = await ObterOuFalharAsync(id, ct);

        var auditorId = dto.AuditorUsuarioId ?? Guid.Empty;
        var gerenteId = dto.GerenteUsuarioId ?? Guid.Empty;

        // Só valida o que mudou: não impede editar a razão social de uma loja cujo auditor já foi desativado
        if (auditorId != loja.AuditorUsuarioId)
            await ValidarAuditorAsync(auditorId, ct);

        if (gerenteId != loja.GerenteUsuarioId)
            await ValidarGerenteAsync(gerenteId, id, ct);

        loja.AlterarRazaoSocial(dto.RazaoSocial);
        loja.TrocarAuditor(auditorId);
        loja.TrocarGerente(gerenteId);

        await _lojas.SalvarAsync(ct);
        return Mapear(loja);
    }

    /// <summary>"Excluir" = desativar: a loja tem histórico de comparações.</summary>
    public async Task DesativarAsync(Guid id, CancellationToken ct)
    {
        var loja = await ObterOuFalharAsync(id, ct);
        loja.Desativar();
        await _lojas.SalvarAsync(ct);
    }

    public async Task ReativarAsync(Guid id, CancellationToken ct)
    {
        var loja = await ObterOuFalharAsync(id, ct);

        await ValidarAuditorAsync(loja.AuditorUsuarioId, ct);
        await ValidarGerenteAsync(loja.GerenteUsuarioId, id, ct);

        loja.Ativar();
        await _lojas.SalvarAsync(ct);
    }

    // ---------- Consulta (cada perfil enxerga só o que é seu) ----------

    public async Task<PaginaDto<LerLojaDto>> ListarAsync(
        FiltroLojaDto filtro, Guid usuarioId, string perfil, CancellationToken ct)
    {
        Guid? auditorId = perfil == PerfilAuditor ? usuarioId : null;
        Guid? gerenteId = perfil == PerfilGerente ? usuarioId : null;

        // Perfil desconhecido não pode cair no "sem filtro" e enxergar tudo
        if (perfil != PerfilAdmin && auditorId is null && gerenteId is null)
            throw new DomainException("Perfil sem acesso às lojas.");

        var (itens, total) = await _lojas.ListarAsync(
            filtro.Busca, filtro.Ativo, auditorId, gerenteId, filtro.Pagina, filtro.Tamanho, ct);

        return new PaginaDto<LerLojaDto>
        {
            Itens = itens.Select(Mapear).ToList(),
            Pagina = filtro.Pagina,
            Tamanho = filtro.Tamanho,
            Total = total
        };
    }

    public async Task<LerLojaDto> ObterAsync(Guid id, Guid usuarioId, string perfil, CancellationToken ct)
    {
        var loja = await _lojas.ObterPorIdAsync(id, ct);

        // Fora do escopo = "não encontrada", para não revelar que a loja existe
        if (loja is null || !PodeVer(loja, usuarioId, perfil))
            throw new DomainException.NaoEncontradoException("Loja não encontrada.");

        return Mapear(loja);
    }
    
    public async Task<IReadOnlyList<UsuarioResumoDto>> ListarGerentesDisponiveisAsync(
        string? busca, Guid? lojaId, CancellationToken ct)
    {
        var gerentes = await _lojas.ListarGerentesDisponiveisAsync(busca, lojaId, LimiteListaGerentes, ct);

        return gerentes
            .Select(g => new UsuarioResumoDto { UsuarioId = g.UsuarioId, Nome = g.NomeUsuario })
            .ToList();
    }

    // ----------------- auxiliares -----------------

    private async Task<Loja> ObterOuFalharAsync(Guid id, CancellationToken ct) =>
        await _lojas.ObterPorIdAsync(id, ct)
        ?? throw new DomainException.NaoEncontradoException("Loja não encontrada.");

    private async Task ValidarAuditorAsync(Guid usuarioId, CancellationToken ct)
    {
        var u = await _usuarios.ObterPorIdAsync(usuarioId, ct)
            ?? throw new DomainException("Auditor não encontrado.");

        if (!u.StatusUsuario)
            throw new DomainException("O auditor informado está desativado.");
        if (u.TipoUsuarioId != (int)TipoUsuarioEnum.Auditor)
            throw new DomainException("O usuário informado não é um Auditor.");
    }

    private async Task ValidarGerenteAsync(Guid usuarioId, Guid? lojaAtualId, CancellationToken ct)
    {
        var u = await _usuarios.ObterPorIdAsync(usuarioId, ct)
            ?? throw new DomainException("Gerente não encontrado.");

        if (!u.StatusUsuario)
            throw new DomainException("O gerente informado está desativado.");
        if (u.TipoUsuarioId != (int)TipoUsuarioEnum.Gerente)
            throw new DomainException("O usuário informado não é um Gerente.");

        if (await _lojas.GerenteVinculadoAsync(usuarioId, lojaAtualId, ct))
            throw new DomainException.ConflitoException("Esse gerente já está vinculado a outra loja.");
    }

    private static bool PodeVer(Loja loja, Guid usuarioId, string perfil) =>
        perfil == PerfilAdmin
        || (perfil == PerfilAuditor && loja.AuditorUsuarioId == usuarioId)
        || (perfil == PerfilGerente && loja.GerenteUsuarioId == usuarioId);

    private static LerLojaDto Mapear(Loja l) => new()
    {
        LojaId = l.LojaId,
        RazaoSocial = l.RazaoSocial,
        Cnpj = l.Cnpj,
        Status = l.StatusLoja,
        DataCriacao = l.DataCriacao,
        Auditor = new UsuarioResumoDto
        {
            UsuarioId = l.AuditorUsuarioId,
            Nome = l.Auditor?.NomeUsuario ?? ""
        },
        Gerente = new UsuarioResumoDto
        {
            UsuarioId = l.GerenteUsuarioId,
            Nome = l.Gerente?.NomeUsuario ?? ""
        }
    };
}
