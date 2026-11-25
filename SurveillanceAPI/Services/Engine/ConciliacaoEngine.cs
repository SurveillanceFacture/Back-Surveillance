using SurveillanceAPI.Dto.ApiUpload.ERP;
using SurveillanceAPI.Dto.ApiUpload.XML;
using SurveillanceAPI.Interfaces;

namespace SurveillanceAPI.Services.Engine;

public class ConciliacaoEngine
{
    private readonly IEnumerable<IConciliacaoRegra> _regras;

    public ConciliacaoEngine(IEnumerable<IConciliacaoRegra> regras)
    {
        _regras = regras;
    }

    public List<DivergenciasDto> Conciliar(long id, List<ItemNotaFiscalDto> itensXML, List<ItemMovimentacaoErpDto> itensERP)
    {
        var divergencias = new List<DivergenciasDto>();
        var eansXml = itensXML.Select(x => x.Ean).ToHashSet();

        foreach (var itemXml in itensXML)
        {
            var itemErp = itensERP.FirstOrDefault(erp => erp.Ean == itemXml.Ean);

            if (itemErp == null)
            {
                divergencias.Add(new DivergenciasDto
                {
                    ComparacaoId = id,
                    NumeroItem = itemXml.NumeroItem,
                    CodigoProduto = itemXml.CodigoProduto,
                    Ean = itemXml.Ean,
                    TipoDivergencia = "ItemNaoLancadoNoErp",
                    CampoComparado = "NumeroItem",
                    ValorXML = $"Presente em (Ean:{itemXml.Ean})",
                    ValorERP = "Não Encontrado",
                    Gravidade = "Alta",
                    Descricao = $"O item '{itemXml.Descricao}' (Ean:{itemXml.Ean}) consta na nota fiscal, mas não foi lançado no ERP.",
                    AcaoRecomendada = "Dar entrada manual no item no sistema ERP."
                });
                continue;
            }

            foreach (var regra in _regras)
            {
                var divergencia = regra.Validar(id, itemXml, itemErp);
                if (divergencia != null)
                {
                    divergencias.Add(divergencia);
                }
            }
        }

        foreach (var itemErp in itensERP)
        {
            if (!eansXml.Contains(itemErp.Ean))
            {
                divergencias.Add(new DivergenciasDto
                {
                    ComparacaoId = id,
                    Ean = itemErp.Ean,
                    TipoDivergencia = "ItemSemNotaFiscal",
                    CampoComparado = "Ean",
                    ValorXML = "Não Encontrado",
                    ValorERP = $"Presente em (Ean:{itemErp.Ean})",
                    Gravidade = "Critica",
                    Descricao = $"O item (Ean:{itemErp.Ean}) consta no ERP, mas não possui nota fiscal válida correspondente (possível venda cancelada).",
                    AcaoRecomendada = "Conferir status da nota e estornar a movimentação no ERP."
                });
            }
        }

        return divergencias;
    }
}