using Surveillance_API.Dto.ApiUpload.ERP;
using Surveillance_API.Dto.ApiUpload.XML;
using Surveillance_API.Interfaces;
using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography.X509Certificates;

namespace Surveillance_API.Services.Engine
{
    public class ConciliacaoEngine
    {
        private readonly IEnumerable<IConciliacaoRegra> _regras;

        public ConciliacaoEngine(IEnumerable<IConciliacaoRegra> regras)
        {
            _regras = regras;
        }

        public List<DivergenciasDto> Conciliar(Guid id, List<ItemNotaFiscalDto> itensXML, List<ItemMovimentacaoErpDto> itensERP)
        {
            var Divergencias = new List<DivergenciasDto>();

            foreach (var itemXml in itensXML)
            {
                var itemErp = itensERP.FirstOrDefault(erp => erp.Ean == itemXml.Ean);

                if (itemErp == null)
                {
                    Divergencias.Add(new DivergenciasDto
                    {
                        ComparacaoId = id,
                        NumeroItem = itemXml.NumeroItem,
                        CodigoProduto = itemXml.CodigoProduto,
                        Ean = itemXml.Ean,
                        TipoDivergencia = "ItemNaoLancadoNoErp",
                        CampoComparado = "NumeroItem",
                        ValorXML = $"Presente em (Ean{itemXml.Ean})",
                        ValorERP = "Não Encontrado",
                        Gravidade = "Alta",
                        Descricao = $"O Item '{itemXml.Descricao}' (Ean:{itemXml.Ean}) consta na nota fiscal, mas não foi lancada no Erp.",
                        AcaoRecomendada = "Dar entrada manual no item no sistema Erp."

                    });
                    continue;
                }


                foreach (var regra in _regras)
                {
                    var divergencia = regra.Validar(id, itemXml, itemErp);

                    if (divergencia != null)
                    {
                        Divergencias.Add(divergencia);

                    }




                }
            }
                    return Divergencias;
        }
    }
}

