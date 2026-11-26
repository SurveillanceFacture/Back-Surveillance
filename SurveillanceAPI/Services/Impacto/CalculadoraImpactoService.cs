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
            if (divergencias is null || divergencias.Count == 0)
                return divergencias ?? [];

            var listaXml = itensXML ?? [];
            var listaErp = itensERP ?? [];

            foreach (var divergencia in divergencias)
            {
                var eanBusca = divergencia.Ean?.Trim();

                var itemXml = !string.IsNullOrWhiteSpace(eanBusca)
     ? listaXml.FirstOrDefault(x => string.Equals(x.Ean?.Trim(), eanBusca, StringComparison.OrdinalIgnoreCase))
     : null;

                var itemErp = !string.IsNullOrWhiteSpace(eanBusca)
                    ? listaErp.FirstOrDefault(e => string.Equals(e.Ean?.Trim(), eanBusca, StringComparison.OrdinalIgnoreCase))
                    : null;

                var valorBruto = CalcularValor(divergencia, itemXml, itemErp);
                divergencia.ImpactoFinanceiro = Math.Round(valorBruto, 2, MidpointRounding.AwayFromZero);
            }

            return divergencias;
        }

        private static decimal CalcularValor(
            DivergenciasDto divergencia,
            ItemNotaFiscalDto? itemXml,
            ItemMovimentacaoErpDto? itemErp)
        {
            if (divergencia.TipoDivergencia == "ItemNaoLancadoNoErp" && itemXml != null)
                return Math.Abs(itemXml.Quantidade * itemXml.ValorUnitario);

            if (divergencia.TipoDivergencia == "ItemSemNotaFiscal" && itemErp != null)
                return Math.Abs(itemErp.Quantidade * itemErp.ValorUnitario);

            if (itemXml == null || itemErp == null)
                return 0m;

            return divergencia.CampoComparado switch
            {
                "ValorUnitario" => Math.Abs(itemXml.ValorUnitario - itemErp.ValorUnitario) * itemXml.Quantidade,

                "Quantidade" => Math.Abs(itemXml.Quantidade - itemErp.Quantidade) * itemXml.ValorUnitario,

                "NCM" or "CFOP" => Math.Abs(itemXml.Quantidade * itemXml.ValorUnitario),

                "Unidade" or "CodigoProduto" => 0m,

                _ => 0m
            };
        }
    }
}