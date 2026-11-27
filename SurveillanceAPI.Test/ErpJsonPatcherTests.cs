using System.Text.Json.Nodes;
using SurveillanceAPI.Applications.Correcao;
using SurveillanceAPI.Domains;
using SurveillanceAPI.Exceptions;
using Xunit;

namespace SurveillanceAPI.Test;

public class ErpJsonPatcherTests
{
    private const string Ean = "7891000100103";

    // JSON do ERP com um lançamento (L1) e um item. "campo_desconhecido" e "extra" precisam sobreviver ao patch.
    private static JsonNode ErpBase() => JsonNode.Parse("""
    {
      "loja": { "cnpj": "11222333000181" },
      "movimentacoes": [
        {
          "id_lancamento": "L1",
          "extra": "fica",
          "itens": [
            {
              "ean": "7891000100103",
              "ncm": "22029900",
              "quantidade": 8,
              "valor_unitario": 10.00,
              "valor_total": 80.00,
              "campo_desconhecido": "fica"
            }
          ]
        }
      ]
    }
    """)!;

    private static CorrecaoErp Correcao(string idLancamento, int indice, string? ean, params AlteracaoErp[] alteracoes) =>
        new(
            new Comparacao(Guid.NewGuid(), Guid.NewGuid(), new string('1', 44), "hash-xml"),
            Guid.NewGuid(), Guid.NewGuid(),
            idLancamento, indice, numeroItem: 1, ean,
            alteracoes, "hash-erp", "explicacao", false,
            DateTime.UtcNow, TimeSpan.FromMinutes(30));

    private static JsonObject Item(JsonNode erp) =>
        ErpJsonPatcher.LocalizarItem(erp, "L1", 0);

    // ---------- caminho feliz ----------

    [Fact]
    public void Aplica_NCM_e_preserva_campos_desconhecidos()
    {
        var erp = ErpBase();
        var c = Correcao("L1", 0, Ean, new AlteracaoErp("ncm", "NCM", "22029900", "22021000", false));

        ErpJsonPatcher.Aplicar(erp, [c]);

        var item = Item(erp);
        Assert.Equal("22021000", item["ncm"]!.GetValue<string>());
        Assert.Equal("fica", item["campo_desconhecido"]!.GetValue<string>());
        Assert.Equal("fica", erp["movimentacoes"]![0]!["extra"]!.GetValue<string>());
        Assert.Equal(8m, item["quantidade"]!.GetValue<decimal>()); // o que não foi pedido não muda
    }

    [Fact]
    public void Aplica_quantidade_junto_com_valor_total_derivado()
    {
        var erp = ErpBase();
        var c = Correcao("L1", 0, Ean,
            new AlteracaoErp("quantidade", "Quantidade", "8", "10", false),
            new AlteracaoErp("valor_total", "ValorTotal", "80.00", "100.00", true));

        ErpJsonPatcher.Aplicar(erp, [c]);

        var item = Item(erp);
        Assert.Equal(10m, item["quantidade"]!.GetValue<decimal>());
        Assert.Equal(100m, item["valor_total"]!.GetValue<decimal>());
    }

    [Fact]
    public void Aceita_antes_com_formatacao_diferente_mas_mesmo_valor()
    {
        // JSON tem 10.00; a proposta guardou "10". Mesmo número, não pode virar conflito.
        var erp = ErpBase();
        var c = Correcao("L1", 0, Ean, new AlteracaoErp("valor_unitario", "ValorUnitario", "10", "12.5", false));

        ErpJsonPatcher.Aplicar(erp, [c]);

        Assert.Equal(12.5m, Item(erp)["valor_unitario"]!.GetValue<decimal>());
    }

    // ---------- conflitos: o ERP mudou desde a análise ----------

    [Fact]
    public void Conflito_quando_o_valor_atual_nao_e_o_antes_analisado()
    {
        var erp = ErpBase();
        var c = Correcao("L1", 0, Ean, new AlteracaoErp("ncm", "NCM", "99999999", "22021000", false));

        Assert.Throws<DomainException.ConflitoException>(() => ErpJsonPatcher.Aplicar(erp, [c]));
        Assert.Equal("22029900", Item(erp)["ncm"]!.GetValue<string>()); // nada gravado
    }

    [Fact]
    public void Conflito_quando_o_numero_atual_nao_e_o_antes_analisado()
    {
        var erp = ErpBase();
        var c = Correcao("L1", 0, Ean, new AlteracaoErp("quantidade", "Quantidade", "7", "10", false));

        Assert.Throws<DomainException.ConflitoException>(() => ErpJsonPatcher.Aplicar(erp, [c]));
    }

    [Fact]
    public void Conflito_quando_o_EAN_do_item_mudou()
    {
        var erp = ErpBase();
        var c = Correcao("L1", 0, "7890000000000", new AlteracaoErp("ncm", "NCM", "22029900", "22021000", false));

        Assert.Throws<DomainException.ConflitoException>(() => ErpJsonPatcher.Aplicar(erp, [c]));
    }

    [Fact]
    public void Conflito_quando_o_indice_do_item_nao_existe_mais()
    {
        var erp = ErpBase();
        var c = Correcao("L1", 5, Ean, new AlteracaoErp("ncm", "NCM", "22029900", "22021000", false));

        Assert.Throws<DomainException.ConflitoException>(() => ErpJsonPatcher.Aplicar(erp, [c]));
    }

    [Fact]
    public void Conflito_quando_o_lancamento_nao_existe()
    {
        var erp = ErpBase();
        var c = Correcao("L-INEXISTENTE", 0, Ean, new AlteracaoErp("ncm", "NCM", "22029900", "22021000", false));

        Assert.Throws<DomainException.ConflitoException>(() => ErpJsonPatcher.Aplicar(erp, [c]));
    }

    [Fact]
    public void Conflito_quando_o_id_do_lancamento_aparece_duas_vezes()
    {
        var erp = ErpBase();
        var movs = (JsonArray)erp["movimentacoes"]!;
        movs.Add(movs[0]!.DeepClone()); // L1 duplicado
        var c = Correcao("L1", 0, Ean, new AlteracaoErp("ncm", "NCM", "22029900", "22021000", false));

        Assert.Throws<DomainException.ConflitoException>(() => ErpJsonPatcher.Aplicar(erp, [c]));
    }

    // ---------- entradas inválidas ----------

    [Fact]
    public void Rejeita_campo_fora_da_lista_permitida()
    {
        var erp = ErpBase();
        var c = Correcao("L1", 0, Ean, new AlteracaoErp("cfop", "CFOP", "5102", "6102", false));

        Assert.Throws<DomainException>(() => ErpJsonPatcher.Aplicar(erp, [c]));
    }

    [Fact]
    public void Rejeita_valor_numerico_invalido()
    {
        var erp = ErpBase();
        var c = Correcao("L1", 0, Ean, new AlteracaoErp("quantidade", "Quantidade", "8", "abc", false));

        Assert.Throws<DomainException>(() => ErpJsonPatcher.Aplicar(erp, [c]));
    }

    [Fact]
    public void Rejeita_json_sem_a_lista_de_movimentacoes()
    {
        var erp = JsonNode.Parse("""{ "loja": {} }""")!;
        var c = Correcao("L1", 0, Ean, new AlteracaoErp("ncm", "NCM", "22029900", "22021000", false));

        Assert.Throws<DomainException>(() => ErpJsonPatcher.Aplicar(erp, [c]));
    }
}
