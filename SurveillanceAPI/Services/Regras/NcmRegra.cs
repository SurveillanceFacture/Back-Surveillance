using SurveillanceAPI.Dto.ApiUpload.ERP;
using SurveillanceAPI.Dto.ApiUpload.XML;
using SurveillanceAPI.Interfaces;

namespace SurveillanceAPI.Services.Regras
{
    public class NcmRegra: IConciliacaoRegra
    {
        public string NomeCampo => "NCM";

        public DivergenciasDto? Validar(Guid comparacaoID, ItemNotaFiscalDto ItemXML, ItemMovimentacaoErpDto ItemERP)
        {
            if(ItemXML.NCM != ItemERP.NCM)
            {
                return new DivergenciasDto
                {
                    NumeroItem = ItemXML.NumeroItem,
                    CampoComparado = NomeCampo,
                    Ean = ItemXML.Ean,
                    ValorXML = ItemXML.NCM,
                    ValorERP = ItemERP.NCM,
                    Gravidade = "critica",
                    DataDeteccao = DateTimeOffset.UtcNow
                };
            }

            return null;
        }

    }
}
