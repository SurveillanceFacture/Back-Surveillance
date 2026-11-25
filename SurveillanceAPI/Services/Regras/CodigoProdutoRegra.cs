using SurveillanceAPI.Dto.ApiUpload.ERP;
using SurveillanceAPI.Dto.ApiUpload.XML;
using SurveillanceAPI.Interfaces;

namespace SurveillanceAPI.Services.Regras
{
    public class CodigoProdutoRegra : IConciliacaoRegra
    {
        public string NomeCampo => "CodigoProduto";

        public DivergenciasDto? Validar(long comparacaoID, ItemNotaFiscalDto ItemXML, ItemMovimentacaoErpDto ItemERP)
        {
            if (ItemXML.CodigoProduto != ItemERP.CodigoProduto)
            {
                return new DivergenciasDto
                {
                    NumeroItem = ItemXML.NumeroItem,
                    CampoComparado = NomeCampo,
                    Ean = ItemXML.Ean,
                    ValorXML = ItemXML.CodigoProduto,
                    ValorERP = ItemERP.CodigoProduto,
                    Gravidade = "Baixa",
                    DataDeteccao = DateTimeOffset.UtcNow
                };
            }

            return null;
        }

    }
}
