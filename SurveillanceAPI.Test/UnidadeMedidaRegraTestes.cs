using SurveillanceAPI.Dto.ApiUpload.ERP;
using SurveillanceAPI.Dto.ApiUpload.XML;
using SurveillanceAPI.Services.Regras;

namespace SurveillanceAPI.Test
{
    public class UnidadeMedidaRegraTestes
    {
        private readonly UnidadeMedidaRegra _regra = new();

        [Fact]
        public void Validar_QuandoUnidadesForemDiferentes_DeveRetornarDivergencia()
        {
            var itemXml = new ItemNotaFiscalDto { Unidade = "CX" };
            var itemErp = new ItemMovimentacaoErpDto { Unidade = "UN" };

            var resultado = _regra.Validar(1, itemXml, itemErp);

            Assert.NotNull(resultado);
            Assert.Equal("CX", resultado.ValorXML);
            Assert.Equal("UN", resultado.ValorERP);
        }

        [Fact]
        public void Validar_QuandoUnidadesForemDiferentes_DeveRetornarGravidadeMedia()
        {
            var itemXml = new ItemNotaFiscalDto { Unidade = "CX" };
            var itemErp = new ItemMovimentacaoErpDto { Unidade = "UN" };

            var resultado = _regra.Validar(1, itemXml, itemErp);

            Assert.NotNull(resultado);
            Assert.Equal("Media", resultado.Gravidade);
        }

        [Fact]
        public void Validar_QuandoUnidadesForemIguais_DeveRetornarNull()
        {
            var itemXml = new ItemNotaFiscalDto { Unidade = "UN" };
            var itemErp = new ItemMovimentacaoErpDto { Unidade = "UN" };

            var resultado = _regra.Validar(1, itemXml, itemErp);

            Assert.Null(resultado);
        }
    }
}
