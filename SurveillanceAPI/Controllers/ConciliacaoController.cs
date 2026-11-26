using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using SurveillanceAPI.Domains;
using SurveillanceAPI.Dto.ApiUpload.Conciliacao;
using SurveillanceAPI.Dto.ApiUpload.XML;
using SurveillanceAPI.Services.Engine;

namespace SurveillanceAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ConciliacaoController : ControllerBase
    {
        private const string Auditor = nameof(TipoUsuarioEnum.Auditor);
        
        private readonly ConciliacaoEngine _engine;
        public ConciliacaoController(ConciliacaoEngine engine)
        {
            _engine = engine;
        }
        
        [HttpPost]
        [Authorize(Roles = Auditor)]
        [EnableRateLimiting("conciliacao")]
        public IActionResult Conciliar(
            [FromQuery] Guid comparacaoId,
            [FromBody] ConciliacaoRequestDto request,
            [FromQuery, System.ComponentModel.DataAnnotations.Range(1,500)] int limite = 10)
        {

            var divergencias = _engine.Conciliar(comparacaoId, request.ItensXml, request.ItensErp);

            var resultadoLimitado = divergencias.Take(limite).ToList();

            return Ok(resultadoLimitado);

        }
    }
}
