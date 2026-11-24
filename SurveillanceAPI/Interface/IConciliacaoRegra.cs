using SurveillanceAPI.Dto.ApiUpload.ERP;
using SurveillanceAPI.Dto.ApiUpload.XML;

namespace SurveillanceAPI.Interfaces
{
    public interface IConciliacaoRegra
    {
        public string NomeCampo { get; }
        public DivergenciasDto? Validar(long comparacaoId, ItemNotaFiscalDto ItemXML, ItemMovimentacaoErpDto ItemERP);
        
                    
    }
}
