using System.Globalization;
using SurveillanceAPI.Dto.ApiUpload.ERP;
using SurveillanceAPI.Dto.ApiUpload.XML;
using SurveillanceAPI.Interfaces;

namespace SurveillanceAPI.Services.Regras
{
    public class ValorUnitarioRegra : IConciliacaoRegra
    {
        public string NomeCampo => "ValorUnitario";

        public DivergenciasDto? Validar(Guid comparacaoID, ItemNotaFiscalDto ItemXML, ItemMovimentacaoErpDto ItemERP)
        {
            decimal diferanca = Math.Abs(ItemXML.ValorUnitario - ItemERP.ValorUnitario);
            if(diferanca > 0.01m)
            {
                return new DivergenciasDto
                {
                    NumeroItem = ItemXML.NumeroItem,
                    CampoComparado = NomeCampo,
                    Ean = ItemXML.Ean,
                    ValorXML = ItemXML.ValorUnitario.ToString(CultureInfo.InvariantCulture),
                    ValorERP = ItemERP.ValorUnitario.ToString(CultureInfo.InvariantCulture),
                    Gravidade = "Critica",
                    DataDeteccao = DateTimeOffset.UtcNow
                };
            }
            return null;
        }
    }
}
