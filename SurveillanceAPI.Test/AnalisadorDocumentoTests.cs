using SurveillanceAPI.Applications.Correcao;
using SurveillanceAPI.Dto.ApiUpload.ERP;
using SurveillanceAPI.Dto.ApiUpload.Xml;
using SurveillanceAPI.Dto.ApiUpload.XML;
using SurveillanceAPI.Exceptions;
using SurveillanceAPI.Interfaces;
using SurveillanceAPI.Services.Engine;
using SurveillanceAPI.Services.Regras;
using Xunit;

namespace SurveillanceAPI.Test;

public class AnalisadorDocumentoTests
{
    private static readonly string Chave = new('1', 44);
    private static readonly string OutraChave = new('2', 44);
    private const string Ean = "7891000100103";

    // ---------- helpers ----------

    private static AnalisadorDocumento Novo() =>
        new(new ConciliacaoEngine(new IConciliacaoRegra[]
        {
            new ValorUnitarioRegra(), new QuantidadeRegra(), new NcmRegra()
        }));

    private static ItemNotaFiscalDto ItemXml(
        string ean = Ean, string ncm = "22021000", decimal qtd = 10m, decimal vu = 10m, decimal total = 100m, int n = 1) =>
        new() { NumeroItem = n, Ean = ean, NCM = ncm, Quantidade = qtd, ValorUnitario = vu, ValorTotal = total };

    private static ItemMovimentacaoErpDto ItemErp(
        string ean = Ean, string ncm = "22021000", decimal qtd = 10m, decimal vu = 10m, decimal total = 100m) =>
        new() { Ean = ean, NCM = ncm, Quantidade = qtd, ValorUnitario = vu, ValorTotal = total };

    private static NotaFiscalDto Nota(params ItemNotaFiscalDto[] itens) =>
        new() { ChaveAcesso = Chave, Itens = itens.ToList() };

    private static LancamentoMovimentacaoDto Lanc(string id, string chave, params ItemMovimentacaoErpDto[] itens) =>
        new() { IdLancamento = id, Documento = new DocumentoMovimentacaoDto { Chave = chave }, Itens = itens.ToList() };

    private static MovimentacaoErpDto Erp(params LancamentoMovimentacaoDto[] lancamentos) =>
        new() { Movimentacoes = lancamentos.ToList() };

    // ---------- sem divergência ----------

    [Fact]
    public void Nota_igual_ao_ERP_nao_gera_divergencia_nem_plano()
    {
        var r = Novo().Analisar(Nota(ItemXml()), Erp(Lanc("L1", Chave, ItemErp())));

        Assert.Empty(r.Divergencias);
        Assert.Empty(r.Planos);
    }

    [Fact]
    public void Diferenca_de_exatamente_um_centavo_na_quantidade_e_tolerada()
    {
        var r = Novo().Analisar(
            Nota(ItemXml(qtd: 10.01m, total: 100.10m)),
            Erp(Lanc("L1", Chave, ItemErp(qtd: 10m))));

        Assert.Empty(r.Divergencias);
    }

    // ---------- correção proposta ----------

    [Fact]
    public void NCM_diferente_propoe_trocar_pelo_NCM_da_nota()
    {
        var r = Novo().Analisar(
            Nota(ItemXml(ncm: "22021000")),
            Erp(Lanc("L1", Chave, ItemErp(ncm: "22029900"))));

        var d = Assert.Single(r.Divergencias);
        Assert.Equal("NCM", d.Divergencia.CampoComparado);
        Assert.NotNull(d.Alteracao);
        Assert.Equal("ncm", d.Alteracao!.Campo);
        Assert.Equal("22029900", d.Alteracao.Antes);
        Assert.Equal("22021000", d.Alteracao.Depois);
        Assert.Equal(new ErpLocalizador("L1", 0), d.Alvo);

        var plano = Assert.Single(r.Planos);
        Assert.Single(plano.Alteracoes);
    }

    [Fact]
    public void Quantidade_diferente_com_nota_consistente_propoe_quantidade_e_total_derivado()
    {
        var r = Novo().Analisar(
            Nota(ItemXml(qtd: 10m, vu: 10m, total: 100m)),
            Erp(Lanc("L1", Chave, ItemErp(qtd: 8m, vu: 10m, total: 80m))));

        var plano = Assert.Single(r.Planos);
        Assert.Equal(2, plano.Alteracoes.Count);

        Assert.Equal("quantidade", plano.Alteracoes[0].Campo);
        Assert.Equal("8", plano.Alteracoes[0].Antes);
        Assert.Equal("10", plano.Alteracoes[0].Depois);
        Assert.False(plano.Alteracoes[0].Derivada);

        Assert.Equal("valor_total", plano.Alteracoes[1].Campo);
        Assert.Equal("80", plano.Alteracoes[1].Antes);
        Assert.Equal("100", plano.Alteracoes[1].Depois);
        Assert.True(plano.Alteracoes[1].Derivada);
    }

    // ---------- motivos para NÃO corrigir ----------

    [Fact]
    public void Total_da_nota_que_nao_confere_com_qtd_x_valor_nao_gera_correcao()
    {
        // 10 × 10 = 100, mas a nota diz 90: mexer na quantidade exigiria inventar o total.
        var r = Novo().Analisar(
            Nota(ItemXml(qtd: 10m, vu: 10m, total: 90m)),
            Erp(Lanc("L1", Chave, ItemErp(qtd: 8m, vu: 10m, total: 80m))));

        var d = Assert.Single(r.Divergencias);
        Assert.Null(d.Alteracao);
        Assert.Equal(MotivosSemCorrecao.TotalNaoConfere, d.MotivoSemCorrecao);
        Assert.Empty(r.Planos);
    }

    [Fact]
    public void NCM_da_nota_sem_8_digitos_nao_gera_correcao()
    {
        var r = Novo().Analisar(
            Nota(ItemXml(ncm: "2202")),
            Erp(Lanc("L1", Chave, ItemErp(ncm: "22029900"))));

        var d = Assert.Single(r.Divergencias);
        Assert.Null(d.Alteracao);
        Assert.Equal(MotivosSemCorrecao.NcmInvalido, d.MotivoSemCorrecao);
    }

    [Fact]
    public void EAN_repetido_na_nota_nunca_recebe_correcao_automatica()
    {
        var r = Novo().Analisar(
            Nota(ItemXml(n: 1, qtd: 10m), ItemXml(n: 2, qtd: 10m)),
            Erp(Lanc("L1", Chave, ItemErp(qtd: 8m, total: 80m))));

        Assert.NotEmpty(r.Divergencias);
        Assert.All(r.Divergencias, d =>
        {
            Assert.Null(d.Alteracao);
            Assert.Equal(MotivosSemCorrecao.EanRepetido, d.MotivoSemCorrecao);
        });
        Assert.Empty(r.Planos);
    }

    [Fact]
    public void Id_de_lancamento_repetido_no_arquivo_impede_a_correcao()
    {
        // Dois lançamentos com id "L1" (um deles de outra nota): não dá para endereçar o item com segurança.
        var r = Novo().Analisar(
            Nota(ItemXml(ncm: "22021000")),
            Erp(
                Lanc("L1", Chave, ItemErp(ncm: "22029900")),
                Lanc("L1", OutraChave, ItemErp())));

        var d = Assert.Single(r.Divergencias);
        Assert.Null(d.Alteracao);
        Assert.Equal(MotivosSemCorrecao.SemLocalizador, d.MotivoSemCorrecao);
    }

    [Fact]
    public void Item_da_nota_ausente_no_ERP_e_divergencia_manual()
    {
        var r = Novo().Analisar(
            Nota(ItemXml(), ItemXml(ean: "7891000200205", n: 2)),
            Erp(Lanc("L1", Chave, ItemErp())));

        var d = Assert.Single(r.Divergencias);
        Assert.Equal("ItemNaoLancadoNoErp", d.Divergencia.TipoDivergencia);
        Assert.Null(d.Alteracao);
        Assert.Equal(MotivosSemCorrecao.ItemAusente, d.MotivoSemCorrecao);
    }

    // ---------- escolha do lançamento ----------

    [Fact]
    public void Lancamento_de_outra_chave_e_ignorado()
    {
        // L1 (outra nota) tem um item divergente; L2 (a nota analisada) está correto.
        var r = Novo().Analisar(
            Nota(ItemXml()),
            Erp(
                Lanc("L1", OutraChave, ItemErp(ncm: "99999999")),
                Lanc("L2", Chave, ItemErp())));

        Assert.Empty(r.Divergencias);
    }

    [Fact]
    public void Chave_sem_nenhum_lancamento_no_ERP_lanca_erro_de_dominio()
    {
        Assert.Throws<DomainException>(() =>
            Novo().Analisar(Nota(ItemXml()), Erp(Lanc("L1", OutraChave, ItemErp()))));
    }

    // ---------- problema conhecido (item 3 da lista) ----------

    [Fact]
    public void Item_sem_EAN_valido_nunca_recebe_correcao_automatica()
    {
        var r = Novo().Analisar(
            Nota(ItemXml(ean: "", ncm: "22021000")),
            Erp(Lanc("L1", Chave, ItemErp(ean: "", ncm: "22029900"))));

        // Hoje isto PROPÕE trocar o NCM de um item que só "casou" porque os dois não têm EAN.
        Assert.All(r.Divergencias, d => Assert.Null(d.Alteracao));
        Assert.Empty(r.Planos);
    }
}
