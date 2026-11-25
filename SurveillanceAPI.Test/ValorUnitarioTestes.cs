
using SurveillanceAPI.Dto.ApiUpload.ERP;
using SurveillanceAPI.Dto.ApiUpload.XML;
using SurveillanceAPI.Services.Regras;

namespace SurveillanceAPI.Test
{
    public class ValorUnitarioTestes
    {
        private readonly ValorUnitarioRegra _regra = new();

        [Fact]
        public void Validar_QuandosValoresUnitariosForemDiferentes_DeveRetornarDivergencia()
        {
            var ItemXml = new ItemNotaFiscalDto { ValorUnitario = 275.905m };
            var ItemErp = new ItemMovimentacaoErpDto { ValorUnitario = 275.904m };

            var resultado = _regra.Validar(1, ItemXml, ItemErp);

            Assert.NotNull(resultado);
            Assert.Equal(ItemXml.ValorUnitario.ToString(),resultado.ValorXML);
            Assert.Equal(ItemErp.ValorUnitario.ToString(),resultado.ValorERP);

        }

        [Fact]

        public void Validar_QuandosValoresUnitariosForemDiferentes_DeveRetornarGravidadesCriticas()
        {
            var ItemXml = new ItemNotaFiscalDto { ValorUnitario = 275.90m };
            var ItemErp = new ItemMovimentacaoErpDto { ValorUnitario = 275.80m };

            var resultado = _regra.Validar(1, ItemXml, ItemErp);

            Assert.NotNull(resultado);
            Assert.Equal("Critica", resultado.Gravidade);

        }




        [Fact]

        public void Validar_QuandoValoresUnitariosForamIguais_DeveRetornarDivergencia()
        {
            var ItemXml = new ItemNotaFiscalDto { ValorUnitario = 275.80m };
            var ItemErp = new ItemMovimentacaoErpDto { ValorUnitario = 275.80m };

            var resultado = _regra.Validar(1, ItemXml, ItemErp);

            Assert.Null(resultado);

        }



    }
}
