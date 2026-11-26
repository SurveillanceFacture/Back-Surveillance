using System.Globalization;
using System.Text.Json.Nodes;
using SurveillanceAPI.Domains;
using SurveillanceAPI.Exceptions;

namespace SurveillanceAPI.Applications.Correcao;

// Único ponto do sistema que escreve no JSON do ERP.
//  - trabalha sobre JsonNode: tudo que não é alterado (campos desconhecidos, ordem, formatos) é preservado;
//  - só aceita a lista fixa de campos abaixo;
//  - só altera se o valor atual ainda for o "antes" analisado (senão conflito);
//  - o XML nunca passa por aqui.
public static class ErpJsonPatcher
{
    private static readonly HashSet<string> CamposTexto = new(StringComparer.Ordinal) { "ncm" };

    private static readonly HashSet<string> CamposNumericos =
        new(StringComparer.Ordinal) { "quantidade", "valor_unitario", "valor_total" };

    public static void Aplicar(JsonNode erp, IEnumerable<CorrecaoErp> correcoes)
    {
        foreach (var correcao in correcoes)
        {
            var item = LocalizarItem(erp, correcao.IdLancamento, correcao.IndiceItem);
            ConferirEan(item, correcao.Ean);

            foreach (var alteracao in correcao.Alteracoes())
            {
                if (CamposTexto.Contains(alteracao.Campo))
                {
                    ConferirTexto(item, alteracao);
                    item[alteracao.Campo] = alteracao.Depois;
                }
                else if (CamposNumericos.Contains(alteracao.Campo))
                {
                    var depois = LerDecimal(alteracao.Depois, alteracao.Campo);
                    ConferirNumero(item, alteracao);
                    item[alteracao.Campo] = depois;
                }
                else
                {
                    throw new DomainException($"Campo não permitido para correção: {alteracao.Campo}.");
                }
            }
        }
    }

    public static JsonObject LocalizarItem(JsonNode erp, string idLancamento, int indice)
    {
        if (erp["movimentacoes"] is not JsonArray movimentacoes)
            throw new DomainException("O JSON do ERP não tem a lista 'movimentacoes'.");

        var encontrados = movimentacoes
            .OfType<JsonObject>()
            .Where(m => m["id_lancamento"] is JsonValue v
                        && v.TryGetValue<string>(out var id)
                        && string.Equals(id, idLancamento, StringComparison.Ordinal))
            .ToList();

        if (encontrados.Count != 1)
            throw new DomainException.ConflitoException(
                "O lançamento do ERP não foi encontrado ou aparece mais de uma vez no arquivo.");

        if (encontrados[0]["itens"] is not JsonArray itens || indice < 0 || indice >= itens.Count)
            throw new DomainException.ConflitoException("O item do ERP não existe mais nessa posição.");

        return itens[indice] as JsonObject
               ?? throw new DomainException.ConflitoException("O item do ERP não existe mais nessa posição.");
    }

    private static void ConferirEan(JsonObject item, string? ean)
    {
        var atual = item["ean"] is JsonValue v && v.TryGetValue<string>(out var s) ? s : null;

        if (!string.Equals(atual ?? string.Empty, ean ?? string.Empty, StringComparison.Ordinal))
            throw new DomainException.ConflitoException("O EAN do item do ERP mudou desde a análise.");
    }

    private static void ConferirTexto(JsonObject item, AlteracaoErp alteracao)
    {
        var atual = item[alteracao.Campo] is JsonValue v && v.TryGetValue<string>(out var s) ? s : null;

        if (!string.Equals(atual, alteracao.Antes, StringComparison.Ordinal))
            throw new DomainException.ConflitoException(
                $"O valor atual de '{alteracao.Campo}' no ERP mudou desde a análise.");
    }

    private static void ConferirNumero(JsonObject item, AlteracaoErp alteracao)
    {
        var antes = LerDecimal(alteracao.Antes, alteracao.Campo);

        if (item[alteracao.Campo] is not JsonValue v || !v.TryGetValue<decimal>(out var atual) || atual != antes)
            throw new DomainException.ConflitoException(
                $"O valor atual de '{alteracao.Campo}' no ERP mudou desde a análise.");
    }

    private static decimal LerDecimal(string texto, string campo)
    {
        if (decimal.TryParse(
                texto,
                NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint,
                CultureInfo.InvariantCulture,
                out var valor))
            return valor;

        throw new DomainException($"Valor numérico inválido na correção de '{campo}'.");
    }
}
