using Surveillance_API.Dto.ApiUpload.ERP;
using Surveillance_API.Dto.ApiUpload.XML;
using Surveillance_API.Interfaces;

namespace Surveillance_API.Services.Regras
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
