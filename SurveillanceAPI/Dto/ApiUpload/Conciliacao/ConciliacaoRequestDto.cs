using SurveillanceAPI.Dto.ApiUpload.ERP;
using SurveillanceAPI.Dto.ApiUpload.XML;

namespace SurveillanceAPI.Dto.ApiUpload.Conciliacao
{
    public class ConciliacaoRequestDto
    {

        public List<ItemNotaFiscalDto> ItensXml { get; set; } = new();

        public List<ItemMovimentacaoErpDto> ItensErp { get; set; } = new();
    }
}
