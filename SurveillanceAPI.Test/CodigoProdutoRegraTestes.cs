using SurveillanceAPI.Dto.ApiUpload.ERP;
using SurveillanceAPI.Dto.ApiUpload.XML;
using SurveillanceAPI.Services.Regras;

namespace SurveillanceAPI.Test
{
    public class CodigoProdutoRegraTestes
    {
        private readonly CodigoProdutoRegra _regra = new();

        [Fact]
        public void Validar_QuandoCodigosProdutosForemDiferentes_DeveRetornarDivergencia()
        {
            var itemXml = new ItemNotaFiscalDto { CodigoProduto = "FORN-1045" };
            var itemErp = new ItemMovimentacaoErpDto { CodigoProduto = "INT-9920" };

            var resultado = _regra.Validar(1, itemXml, itemErp);

            Assert.NotNull(resultado);
            Assert.Equal(itemXml.CodigoProduto, resultado.ValorXML);
            Assert.Equal(itemErp.CodigoProduto, resultado.ValorERP);
        }

        [Fact]
        public void Validar_QuandoCodigosProdutosForemDiferentes_DeveRetornarGravidadeBaixa()
        {
            var itemXml = new ItemNotaFiscalDto { CodigoProduto = "FORN-1045" };
            var itemErp = new ItemMovimentacaoErpDto { CodigoProduto = "INT-9920" };

            var resultado = _regra.Validar(1, itemXml, itemErp);

            Assert.NotNull(resultado);
            Assert.Equal("Baixa", resultado.Gravidade);
        }

        [Fact]
        public void Validar_QuandoCodigosProdutosForemIguais_DeveRetornarNull()
        {
            var itemXml = new ItemNotaFiscalDto { CodigoProduto = "FORN-1045" };
            var itemErp = new ItemMovimentacaoErpDto { CodigoProduto = "FORN-1045" };

            var resultado = _regra.Validar(1, itemXml, itemErp);

            Assert.Null(resultado);
        }
    }
}