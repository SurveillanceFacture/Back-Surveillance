using SurveillanceAPI.Applications.Service.Auditoria;
using SurveillanceAPI.Dto.ApiUpload.ERP;
using SurveillanceAPI.Dto.ApiUpload.XML;
using SurveillanceAPI.Services.Regras;

namespace SurveillanceAPI.Test
{
    public class NcmRegraTestes 
    {
        private readonly NcmRegra _regra = new();

        [Fact]
        public void Validar_QuandoNCMForDiferente_DeveRetornarDivergencia()
        {
            var ItemXml = new ItemNotaFiscalDto { NCM  = "84713012" };
            var ItemErp = new ItemMovimentacaoErpDto {NCM = "85171231"};

            var resultatdo = _regra.Validar(Guid.NewGuid(), ItemXml, ItemErp);

            Assert.NotNull(resultatdo);
            Assert.Equal("84713012", resultatdo.ValorXML);
            Assert.Equal("85171231", resultatdo.ValorERP);
        }


        [Fact]
        public void Validar_QuandoNCMForDiferente_DeveRetornarGravidadeCritica()
        {
            var ItemXml = new ItemNotaFiscalDto { NCM = "84713012" };
            var ItemErp = new ItemMovimentacaoErpDto { NCM = "85171231" };

            var resultado = _regra.Validar(Guid.NewGuid(), ItemXml, ItemErp);


            Assert.NotNull(resultado);
            Assert.Equal("Critica", resultado.Gravidade);


        }



        [Fact]

        public void Validar_QuandoNCMForIgual_DeveRetornarNull()
        {
            var ItemXml = new ItemNotaFiscalDto { NCM = "84713012" };
            var ItemErp = new ItemMovimentacaoErpDto { NCM = "84713012" };

            var resultado = _regra.Validar(Guid.NewGuid(), ItemXml, ItemErp);

            Assert.Null(resultado);


        }





    }
}
