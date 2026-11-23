using SurveillanceAPI.Dto.ApiUpload.ERP;
using SurveillanceAPI.Dto.ApiUpload.XML;
using SurveillanceAPI.Services.Regras;
using System;
using System.Collections.Generic;
using System.Text;

namespace SurveillanceAPI.Test
{
    public class QuantitadeRegraTestes
    {
        private readonly QuantidadeRegra _regra = new();

        [Fact]

        public void Validar_QuandoQuantidadeForDiferente_DeveRetornarDivergencia()
        {
          var itemXml = new ItemNotaFiscalDto { Quantidade = 1.49m };
          var itemErp = new ItemMovimentacaoErpDto { Quantidade = 1.50m };

          var resultado = _regra.Validar(Guid.NewGuid(), itemXml, itemErp);

            Assert.NotNull(resultado);
            Assert.Equal(itemXml.Quantidade.ToString(), resultado.ValorXML);
            Assert.Equal(itemErp.Quantidade.ToString(), resultado.ValorERP);

        }


        [Fact]

        public void Validar_QuandoQuantidadeForDiferente_DeveRetornarGravidadeCritica()
        {

            var itemXml = new ItemNotaFiscalDto { Quantidade = 1.49m };
            var itemErp = new ItemMovimentacaoErpDto { Quantidade = 1.50m };

            var resultado = _regra.Validar(Guid.NewGuid(), itemXml, itemErp);

            Assert.NotNull(resultado);
            Assert.Equal("Critica", resultado.Gravidade);


        }



        [Fact]

         public void Validar_QuandoQuantidadeForIgual_DeveRetornarNull()
        {
            var itemXml = new ItemNotaFiscalDto { Quantidade = 1.49m };
            var itemErp = new ItemMovimentacaoErpDto { Quantidade = 1.49m };

            var resultado = _regra.Validar(Guid.NewGuid(), itemXml, itemErp);

            Assert.Null(resultado);

        }





    }
}
