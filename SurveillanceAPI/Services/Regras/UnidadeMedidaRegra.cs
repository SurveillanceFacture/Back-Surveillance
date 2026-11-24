using SurveillanceAPI.Dto.ApiUpload.ERP;
using SurveillanceAPI.Dto.ApiUpload.XML;
using SurveillanceAPI.Interfaces;

namespace SurveillanceAPI.Services.Regras
{
    public class UnidadeMedidaRegra : IConciliacaoRegra
    {
        public string NomeCampo => "Unidade";

        public DivergenciasDto? Validar(long comparacaoID, ItemNotaFiscalDto ItemXML, ItemMovimentacaoErpDto ItemERP)
        {
            var unidadeXml = ItemXML.Unidade?.Trim().ToUpperInvariant();
            var unidadeErp = ItemERP.Unidade?.Trim().ToUpperInvariant();

            if (unidadeXml != unidadeErp)
            {
                return new DivergenciasDto
                {
                    NumeroItem = ItemXML.NumeroItem,
                    CampoComparado = NomeCampo,
                    Ean = ItemXML.Ean,
                    ValorXML = ItemXML.Unidade,
                    ValorERP = ItemERP.Unidade,
                    Gravidade = "Media",
                    DataDeteccao = DateTimeOffset.UtcNow
                };
            }

            return null;
        }
    }
}
