using SurveillanceAPI.Dto.ApiUpload.ERP;
using SurveillanceAPI.Dto.ApiUpload.XML;
using SurveillanceAPI.Interfaces;

namespace SurveillanceAPI.Services.Regras
{
    public class ValorUnitarioRegra : IConciliacaoRegra
    {
        public string NomeCampo => "ValorUnitario";

        public DivergenciasDto? Validar(long comparacaoId, ItemNotaFiscalDto ItemXML, ItemMovimentacaoErpDto ItemERP)
        {
            if (ItemXML.ValorUnitario != ItemERP.ValorUnitario)
            {
                return new DivergenciasDto
                {
                    NumeroItem = ItemXML.NumeroItem,
                    CampoComparado = NomeCampo,
                    Ean = ItemXML.Ean,
                    ValorXML = ItemXML.ValorUnitario.ToString(),
                    ValorERP = ItemERP.ValorUnitario.ToString(),
                    Gravidade = "Critica",
                    DataDeteccao = DateTimeOffset.UtcNow



                };

            }
            return null;
        }
    }
}
