using SurveillanceAPI.Dto.ApiUpload.ERP;
using SurveillanceAPI.Dto.ApiUpload.XML;
using SurveillanceAPI.Services.Regras;

namespace SurveillanceAPI.Test
{
    public class CfopRegraTestes
    {
        private readonly CfopRegra _regra = new();

        [Fact]
        public void Validar_QuandoCfopsForemDiferentes_DeveRetornarDivergencia()
        {
            var itemXml = new ItemNotaFiscalDto { CFOP = "5102" };
            var itemErp = new ItemMovimentacaoErpDto { CFOP = "5405" };

            var resultado = _regra.Validar(Guid.NewGuid(), itemXml, itemErp);

            Assert.NotNull(resultado);
            Assert.Equal(itemXml.CFOP, resultado.ValorXML);
            Assert.Equal(itemErp.CFOP, resultado.ValorERP);
        }

        [Fact]
        public void Validar_QuandoCfopsForemDiferentes_DeveRetornarGravidadeAlta()
        {
            var itemXml = new ItemNotaFiscalDto { CFOP = "5102" };
            var itemErp = new ItemMovimentacaoErpDto { CFOP = "5405" };

            var resultado = _regra.Validar(Guid.NewGuid(), itemXml, itemErp);

            Assert.NotNull(resultado);
            Assert.Equal("Alta", resultado.Gravidade);
        }

        [Fact]
        public void Validar_QuandoCfopsForemIguais_DeveRetornarNull()
        {
            var itemXml = new ItemNotaFiscalDto { CFOP = "5102" };
            var itemErp = new ItemMovimentacaoErpDto { CFOP = "5102" };

            var resultado = _regra.Validar(Guid.NewGuid(), itemXml, itemErp);

            Assert.Null(resultado);
        }
    }
}