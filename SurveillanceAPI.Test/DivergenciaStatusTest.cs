using SurveillanceAPI.Domains;
using SurveillanceAPI.Exceptions;
using Xunit;

namespace SurveillanceAPI.Test;

public class DivergenciaStatusTests
{
    private static readonly Guid Auditor = Guid.NewGuid();

    // O construtor de Divergencia é internal: o caminho público é Comparacao.AdicionarDivergencia.
    private static Divergencia Nova()
    {
        var comparacao = new Comparacao(Guid.NewGuid(), Auditor, new string('1', 44), "hash-xml");
        comparacao.AdicionarDivergencia(1, "P1", "7891000100103", "NCM", "22021000", "22029900", GravidadeDivergencia.Critica);
        return comparacao.Divergencias.Single();
    }

    [Fact]
    public void Nasce_aberta()
    {
        Assert.Equal(StatusDivergencia.Aberta, Nova().Status);
    }

    [Fact]
    public void Correcao_gerada_nao_encerra_a_divergencia()
    {
        var d = Nova();

        d.MarcarCorrecaoGerada(Auditor, "correção gerada");

        Assert.Equal(StatusDivergencia.CorrecaoGerada, d.Status);
        Assert.Null(d.DataResolucao);           // ainda não foi resolvida
        Assert.Null(d.ResolvidoPorUsuarioId);
        Assert.Single(d.Historico);
    }

    [Fact]
    public void Correcao_gerada_pode_ser_confirmada_como_resolvida()
    {
        var d = Nova();
        d.MarcarCorrecaoGerada(Auditor);

        d.Resolver(Auditor, "confirmado no ERP");

        Assert.Equal(StatusDivergencia.Resolvida, d.Status);
        Assert.NotNull(d.DataResolucao);
        Assert.Equal(Auditor, d.ResolvidoPorUsuarioId);
        Assert.Equal(2, d.Historico.Count);
    }

    [Fact]
    public void Correcao_gerada_pode_ser_reaberta_se_o_ERP_ainda_diverge()
    {
        var d = Nova();
        d.MarcarCorrecaoGerada(Auditor);

        d.Reabrir(Auditor, "ainda diverge");

        Assert.Equal(StatusDivergencia.Aberta, d.Status);
        Assert.Null(d.DataResolucao);
        Assert.Equal(2, d.Historico.Count);
    }

    [Fact]
    public void Reaberta_pode_ter_nova_correcao_gerada()
    {
        var d = Nova();
        d.MarcarCorrecaoGerada(Auditor);
        d.Reabrir(Auditor);

        d.MarcarCorrecaoGerada(Auditor);

        Assert.Equal(StatusDivergencia.CorrecaoGerada, d.Status);
    }

    [Fact]
    public void Nao_reabre_divergencia_que_nao_esta_com_correcao_gerada()
    {
        Assert.Throws<DomainException>(() => Nova().Reabrir(Auditor));
    }

    [Fact]
    public void Nao_gera_correcao_para_divergencia_ja_resolvida()
    {
        var d = Nova();
        d.Resolver(Auditor);

        Assert.Throws<DomainException>(() => d.MarcarCorrecaoGerada(Auditor));
    }

    [Fact]
    public void Nao_gera_correcao_duas_vezes_seguidas()
    {
        var d = Nova();
        d.MarcarCorrecaoGerada(Auditor);

        Assert.Throws<DomainException>(() => d.MarcarCorrecaoGerada(Auditor));
    }

    [Fact]
    public void Exige_usuario_valido()
    {
        var d = Nova();

        Assert.Throws<DomainException>(() => d.MarcarCorrecaoGerada(Guid.Empty));
    }
    
    private static Comparacao ComDuas()
    {
        var c = new Comparacao(Guid.NewGuid(), Auditor, new string('1', 44), "hash-xml");
        c.AdicionarDivergencia(1, "P1", "7891000100103", "NCM", "22021000", "22029900", GravidadeDivergencia.Critica);
        c.AdicionarDivergencia(2, "P2", "7891000200205", "Quantidade", "10", "8", GravidadeDivergencia.Critica);
        return c;
    }

    [Fact]
    public void Comparacao_so_fica_regularizada_quando_todas_as_divergencias_estao_encerradas()
    {
        var c = ComDuas();
        var (a, b) = (c.Divergencias.First(), c.Divergencias.Last());

        a.Resolver(Auditor);
        c.RecalcularStatus();
        Assert.Equal(StatusComparacao.ComDivergencias, c.Status);   // ainda falta uma

        b.Justificar(Auditor, "justificada");
        c.RecalcularStatus();
        Assert.Equal(StatusComparacao.Regularizada, c.Status);
    }

    [Fact]
    public void Divergencia_reaberta_tira_a_comparacao_de_regularizada()
    {
        var c = ComDuas();
        foreach (var d in c.Divergencias) d.Resolver(Auditor);
        c.RecalcularStatus();
        Assert.Equal(StatusComparacao.Regularizada, c.Status);

        // Cenário de correção gerada que depois é reaberta, em outra comparação:
        var c2 = ComDuas();
        foreach (var d in c2.Divergencias) d.MarcarCorrecaoGerada(Auditor);
        c2.RecalcularStatus();
        Assert.Equal(StatusComparacao.ComDivergencias, c2.Status);   // CorrecaoGerada não encerra

        c2.Divergencias.First().Reabrir(Auditor);
        c2.RecalcularStatus();
        Assert.Equal(StatusComparacao.ComDivergencias, c2.Status);
    }

    [Fact]
    public void Comparacao_sem_divergencia_e_conforme()
    {
        var c = new Comparacao(Guid.NewGuid(), Auditor, new string('1', 44), "hash-xml");
        c.RecalcularStatus();
        Assert.Equal(StatusComparacao.Conforme, c.Status);
    }
}
