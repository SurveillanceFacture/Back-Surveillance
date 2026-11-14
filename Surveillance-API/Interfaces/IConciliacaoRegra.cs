using Surveillance_API.Dto.ApiUpload.ERP;
using Surveillance_API.Dto.ApiUpload.XML;

namespace Surveillance_API.Interfaces
{
    public interface IConciliacaoRegra
    {
        public string NomeCampo { get; }
        public DivergenciasDto? Validar(Guid comparacaoID, ItemNotaFiscalDto ItemXML, ItemMovimentacaoErpDto ItemERP);
        
                    
    }
}
