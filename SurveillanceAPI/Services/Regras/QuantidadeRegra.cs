using SurveillanceAPI.Dto.ApiUpload.ERP;
using SurveillanceAPI.Dto.ApiUpload.XML;
using SurveillanceAPI.Interfaces;

namespace SurveillanceAPI.Services.Regras
{
    public class QuantidadeRegra: IConciliacaoRegra
    {
        public string NomeCampo => "Quantidade";

        public DivergenciasDto? Validar(Guid comparacaoID, ItemNotaFiscalDto ItemXML, ItemMovimentacaoErpDto ItemERP)
        {
            decimal diferenca = Math.Abs(ItemXML.Quantidade - ItemERP.Quantidade);
            if(diferenca > 0.01m)
            {
                return new DivergenciasDto
                {
                    NumeroItem = ItemXML.NumeroItem,
                    CampoComparado = NomeCampo,
                    Ean = ItemXML.Ean,
                    ValorXML = ItemXML.Quantidade.ToString(),
                    ValorERP = ItemERP.Quantidade.ToString(),
                    Gravidade = "Critica",
                    DataDeteccao = DateTimeOffset.UtcNow
                };
            }
                return null;
        }
    }
}
