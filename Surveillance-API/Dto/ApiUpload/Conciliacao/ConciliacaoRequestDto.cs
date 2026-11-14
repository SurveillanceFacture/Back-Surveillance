using Surveillance_API.Dto.ApiUpload.ERP;
using Surveillance_API.Dto.ApiUpload.XML;

namespace Surveillance_API.Dto.ApiUpload.Conciliacao
{
    public class ConciliacaoRequestDto
    {

        public List<ItemNotaFiscalDto> ItensXml { get; set; } = new();

        public List<ItemMovimentacaoErpDto> ItensErp { get; set; } = new();
    }
}
