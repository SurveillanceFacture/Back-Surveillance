using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SurveillanceAPI.Dto.ApiUpload.Conciliacao;
using SurveillanceAPI.Dto.ApiUpload.XML;
using SurveillanceAPI.Services.Engine;

namespace SurveillanceAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles="Auditor")]
    public class ConciliacaoController : ControllerBase
    {
        private readonly ConciliacaoEngine _engine;
        public ConciliacaoController(ConciliacaoEngine engine)
        {
            _engine = engine;
        }

        [HttpPost]
        public IActionResult Conciliar(
            [FromQuery] long comparacaoId,
            [FromBody] ConciliacaoRequestDto request,
            [FromQuery,Range(1,100)] int limite = 10)
        {

            var divergencias = _engine.Conciliar(comparacaoId, request.ItensXml, request.ItensErp);

            var resultadoLimitado = divergencias.Take(limite).ToList();

            return Ok(resultadoLimitado);

        }
    }
}
