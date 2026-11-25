using SurveillanceAPI.Dto.ApiUpload.ERP;
using SurveillanceAPI.Dto.ApiUpload.XML;
using SurveillanceAPI.Interfaces;
using SurveillanceAPI.Services.Engine;
using SurveillanceAPI.Services.Regras;

namespace SurveillanceAPI.Test
{
    public class ConciliacaoEngineTestes
    {
        private static ConciliacaoEngine CriarEngine()
        {
            var regras = new List<IConciliacaoRegra>
            {
                new NcmRegra(),
                new ValorUnitarioRegra(),
                new QuantidadeRegra(),
                new CfopRegra(),
                new UnidadeMedidaRegra(),
                new CodigoProdutoRegra()
            };

            return new ConciliacaoEngine(regras);
        }

        [Fact]
        public void Conciliar_QuandoItensForemConformes_DeveRetornarListaVazia()
        {
            var engine = CriarEngine();
            var itensXml = new List<ItemNotaFiscalDto>
            {
                new()
                {
                    NumeroItem = 1,
                    Ean = "7891000100103",
                    CodigoProduto = "FORN-1045",
                    NCM = "22021000",
                    CFOP = "5102",
                    Unidade = "UN",
                    Quantidade = 10,
                    ValorUnitario = 5.50m
                }
            };
            var itensErp = new List<ItemMovimentacaoErpDto>
            {
                new()
                {
                    Ean = "7891000100103",
                    CodigoProduto = "FORN-1045",
                    NCM = "22021000",
                    CFOP = "5102",
                    Unidade = "UN",
                    Quantidade = 10,
                    ValorUnitario = 5.50m
                }
            };

            var resultado = engine.Conciliar(1, itensXml, itensErp);

            Assert.Empty(resultado);
        }

        [Fact]
        public void Conciliar_QuandoItemNaoExistirNoErp_DeveRetornarItemNaoLancadoNoErp()
        {
            var engine = CriarEngine();
            long comparacaoId = 1;
            var itensXml = new List<ItemNotaFiscalDto>
            {
                new() { NumeroItem = 1, CodigoProduto = "FORN-1045", Descricao = "Arroz 5kg", Ean = "7891000100103" }
            };
            var itensErp = new List<ItemMovimentacaoErpDto>();

            var resultado = engine.Conciliar(comparacaoId, itensXml, itensErp);

            var divergencia = Assert.Single(resultado);
            Assert.Equal(comparacaoId, divergencia.ComparacaoId);
            Assert.Equal("ItemNaoLancadoNoErp", divergencia.TipoDivergencia);
            Assert.Equal("Alta", divergencia.Gravidade);
        }

        [Fact]
        public void Conciliar_QuandoItemDoErpNaoTiverNota_DeveRetornarItemSemNotaFiscal()
        {
            var engine = CriarEngine();
            long comparacaoId = 1;
            var itensXml = new List<ItemNotaFiscalDto>();
            var itensErp = new List<ItemMovimentacaoErpDto>
            {
                new() { Ean = "7891000100103" }
            };

            var resultado = engine.Conciliar(comparacaoId, itensXml, itensErp);

            var divergencia = Assert.Single(resultado);
            Assert.Equal(comparacaoId, divergencia.ComparacaoId);
            Assert.Equal("ItemSemNotaFiscal", divergencia.TipoDivergencia);
            Assert.Equal("Critica", divergencia.Gravidade);
        }

        [Fact]
        public void Conciliar_QuandoItemViolarMultiplasRegras_DeveRetornarTodasAsDivergencias()
        {
            var engine = CriarEngine();
            var itensXml = new List<ItemNotaFiscalDto>
            {
                new()
                {
                    NumeroItem = 1,
                    Ean = "7891000100103",
                    CodigoProduto = "FORN-1045",
                    NCM = "22021000",
                    CFOP = "5102",
                    Unidade = "CX",
                    Quantidade = 1,
                    ValorUnitario = 60.00m
                }
            };
            var itensErp = new List<ItemMovimentacaoErpDto>
            {
                new()
                {
                    Ean = "7891000100103",
                    CodigoProduto = "INT-9920",
                    NCM = "22021000",
                    CFOP = "5405",
                    Unidade = "UN",
                    Quantidade = 1,
                    ValorUnitario = 60.00m
                }
            };

            var resultado = engine.Conciliar(1, itensXml, itensErp);

            Assert.Equal(3, resultado.Count);
            Assert.Contains(resultado, d => d.CampoComparado == "CFOP");
            Assert.Contains(resultado, d => d.CampoComparado == "Unidade");
            Assert.Contains(resultado, d => d.CampoComparado == "CodigoProduto");
        }
    }
}