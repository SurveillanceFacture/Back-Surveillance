using SurveillanceAPI.Dto.ApiUpload.ERP;
using SurveillanceAPI.Dto.ApiUpload.XML;

namespace SurveillanceAPI.Services.Impacto
{
    public class CalculadoraImpactoService
    {
        public List<DivergenciasDto> CalcularImpactos(
            List<DivergenciasDto> divergencias,
            List<ItemNotaFiscalDto> itensXML,
            List<ItemMovimentacaoErpDto> itensERP)
        {
            foreach (var divergencia in divergencias)
            {
                var itemXml = itensXML.FirstOrDefault(x => x.Ean == divergencia.Ean);
                var itemErp = itensERP.FirstOrDefault(e => e.Ean == divergencia.Ean);

                divergencia.ImpactoFinanceiro = Math.Round(CalcularValor(divergencia, itemXml, itemErp), 2);
            }

            return divergencias;
        }

        private static decimal CalcularValor(
            DivergenciasDto divergencia,
            ItemNotaFiscalDto? itemXml,
            ItemMovimentacaoErpDto? itemErp)
        {
            if (divergencia.TipoDivergencia == "ItemNaoLancadoNoErp" && itemXml != null)
                return itemXml.Quantidade * itemXml.ValorUnitario;

            if (divergencia.TipoDivergencia == "ItemSemNotaFiscal" && itemErp != null)
                return itemErp.Quantidade * itemErp.ValorUnitario;

            if (itemXml == null || itemErp == null)
                return 0m;

            return divergencia.CampoComparado switch
            {
                "ValorUnitario" => Math.Abs(itemXml.ValorUnitario - itemErp.ValorUnitario) * itemXml.Quantidade,

                "Quantidade" => Math.Abs(itemXml.Quantidade - itemErp.Quantidade) * itemXml.ValorUnitario,

                "NCM" or "CFOP" => itemXml.Quantidade * itemXml.ValorUnitario,

                "Unidade" or "CodigoProduto" => 0m,

                _ => 0m
            };
        }
    }
}