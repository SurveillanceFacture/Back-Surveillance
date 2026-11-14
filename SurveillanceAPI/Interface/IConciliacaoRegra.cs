using SurveillanceAPI.Dto.ApiUpload.ERP;
using SurveillanceAPI.Dto.ApiUpload.XML;

namespace SurveillanceAPI.Interfaces
{
    public interface IConciliacaoRegra
    {
        public string NomeCampo { get; }
        public DivergenciasDto? Validar(Guid comparacaoID, ItemNotaFiscalDto ItemXML, ItemMovimentacaoErpDto ItemERP);
        
                    
    }
}
