using System.Globalization;

namespace SurveillanceAPI.Applications.Transcricao;

public sealed record Contagem(string Nome, int Quantidade);

public sealed record ItemContagem(int NumeroItem, int Quantidade);

// Só agregados: é isto (e nada de valores individuais) que a IA recebe para escrever o resumo.
public sealed record ResumoDados(
    int Total,
    int ComPropostaDeCorrecao,
    int SomenteManuais,
    IReadOnlyList<Contagem> PorGravidade,
    IReadOnlyList<Contagem> PorCampo,
    IReadOnlyList<ItemContagem> ItensMaisAfetados)
{
    public static ResumoDados De(IReadOnlyList<FatoDivergencia> fatos)
    {
        var porGravidade = fatos
            .GroupBy(f => f.Gravidade)
            .OrderByDescending(g => FatoDivergenciaFactory.RankGravidade(g.Key))
            .Select(g => new Contagem(g.Key, g.Count()))
            .ToList();

        var porCampo = fatos
            .GroupBy(f => f.Tipo == TiposFato.ItemAusenteNoErp ? "item não lançado no ERP" : f.Campo)
            .OrderByDescending(g => g.Count())
            .ThenBy(g => g.Key, StringComparer.Ordinal)
            .Select(g => new Contagem(g.Key, g.Count()))
            .ToList();

        var itens = fatos
            .Where(f => f.NumeroItem.HasValue)
            .GroupBy(f => f.NumeroItem!.Value)
            .Where(g => g.Count() >= 2)
            .OrderByDescending(g => g.Count())
            .ThenBy(g => g.Key)
            .Take(3)
            .Select(g => new ItemContagem(g.Key, g.Count()))
            .ToList();

        var comProposta = fatos.Count(f => f.PropostaAlterarErp);

        return new ResumoDados(fatos.Count, comProposta, fatos.Count - comProposta, porGravidade, porCampo, itens);
    }

    public IEnumerable<string?> ValoresPermitidos()
    {
        yield return Total.ToString(CultureInfo.InvariantCulture);
        yield return ComPropostaDeCorrecao.ToString(CultureInfo.InvariantCulture);
        yield return SomenteManuais.ToString(CultureInfo.InvariantCulture);
        foreach (var c in PorGravidade) yield return c.Quantidade.ToString(CultureInfo.InvariantCulture);
        foreach (var c in PorCampo) yield return c.Quantidade.ToString(CultureInfo.InvariantCulture);
        foreach (var i in ItensMaisAfetados)
        {
            yield return i.NumeroItem.ToString(CultureInfo.InvariantCulture);
            yield return i.Quantidade.ToString(CultureInfo.InvariantCulture);
        }
    }
}
