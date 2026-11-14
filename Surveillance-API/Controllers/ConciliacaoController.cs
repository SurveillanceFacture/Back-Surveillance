using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Surveillance_API.Dto.ApiUpload.Conciliacao;
using Surveillance_API.Dto.ApiUpload.XML;
using Surveillance_API.Services.Engine;

namespace Surveillance_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ConciliacaoController : ControllerBase
    {
        private readonly ConciliacaoEngine _engine;
        public ConciliacaoController(ConciliacaoEngine engine)
        {
            _engine = engine;
        }

        [HttpPost]
        public IActionResult Conciliar(
            [FromQuery] Guid comparacaoId,
            [FromBody] ConciliacaoRequestDto request,
            [FromQuery] int limite = 10)
        {

            var divergencias = _engine.Conciliar(comparacaoId, request.ItensXml, request.ItensErp);

            var resultadoLimitado = divergencias.Take(limite).ToList();

            return Ok(resultadoLimitado);

        }
    }
}
