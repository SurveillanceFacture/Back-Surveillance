using SurveillanceAPI.Dto.ApiUpload.ERP;
using SurveillanceAPI.Dto.ApiUpload.XML;
using SurveillanceAPI.Interfaces;

namespace SurveillanceAPI.Services.Regras
{
    public class CfopRegra: IConciliacaoRegra
    {
        public string NomeCampo => "CFOP";

        public DivergenciasDto? Validar(long comparacaoId, ItemNotaFiscalDto ItemXML, ItemMovimentacaoErpDto ItemERP) 
        { 
          if(ItemXML.CFOP != ItemERP.CFOP)
            {
                return new DivergenciasDto
                {
                    NumeroItem = ItemXML.NumeroItem,
                    CampoComparado = NomeCampo,
                    Ean = ItemXML.Ean,
                    ValorXML = ItemXML.CFOP,
                    ValorERP = ItemERP.CFOP,
                    Gravidade = "Alta",
                    DataDeteccao = DateTimeOffset.UtcNow
                };

            }
                return null;
        
        
        
        }




    }
}
