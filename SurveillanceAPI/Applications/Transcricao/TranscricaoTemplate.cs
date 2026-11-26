using System.Text;

namespace SurveillanceAPI.Applications.Transcricao;

// Texto determinístico: é o fallback quando a IA está fora, devolve algo inválido ou está desligada.
public static class TranscricaoTemplate
{
    public static string Item(FatoDivergencia f)
    {
        var origem = (f.NumeroItem, string.IsNullOrWhiteSpace(f.Ean)) switch
        {
            ({ } n, false) => $"Item {n} (EAN {f.Ean})",
            ({ } n, true) => $"Item {n}",
            (null, false) => $"Item (EAN {f.Ean})",
            _ => "Item"
        };

        string descricao;
        if (f.Tipo == TiposFato.ItemAusenteNoErp)
        {
            descricao = $"{origem}: consta na nota fiscal, mas não foi lançado no ERP.";
        }
        else
        {
            var diferenca = string.IsNullOrWhiteSpace(f.Diferenca) ? "" : $" (diferença de {f.Diferenca})";
            descricao = $"{origem}: divergência de {f.Campo} — XML: {f.ValorXml ?? "(vazio)"}, ERP: {f.ValorErp ?? "(vazio)"}{diferenca}.";
        }

        if (f.PropostaAlterarErp)
        {
            var total = (f.TotalErpAtual, f.TotalErpProposto) is ({ } de, { } para)
                ? $" e o valor total do item de {de} para {para}"
                : "";
            return $"{descricao} Proposta: ajustar {f.Campo} no ERP para {f.ValorXml ?? "(vazio)"}{total}.";
        }

        if (!string.IsNullOrWhiteSpace(f.SemCorrecao))
            return $"{descricao} Sem correção automática: {f.SemCorrecao}.";

        return descricao;
    }

    public static string Resumo(ResumoDados r)
    {
        if (r.Total == 0) return "Nenhuma divergência encontrada.";

        var abertura = r.Total == 1
            ? "Foi encontrada 1 divergência"
            : $"Foram encontradas {r.Total} divergências";

        var gravidade = string.Join("; ", r.PorGravidade.Select(c => $"{c.Nome}: {c.Quantidade}"));
        var campos = string.Join("; ", r.PorCampo.Select(c => $"{c.Nome}: {c.Quantidade}"));

        var sb = new StringBuilder($"{abertura}. Por gravidade — {gravidade}. Por campo — {campos}.");

        if (r.ItensMaisAfetados.Count > 0)
        {
            var itens = string.Join("; ", r.ItensMaisAfetados.Select(i => $"item {i.NumeroItem}: {i.Quantidade}"));
            sb.Append(" Itens com mais divergências — ").Append(itens).Append('.');
        }

        sb.Append($" {r.ComPropostaDeCorrecao} com proposta de correção automática e {r.SomenteManuais} só com correção manual.");

        return sb.ToString();
    }
}
