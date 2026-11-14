using SurveillanceAPI.Domains;

namespace SurveillanceAPI.Applications.Service.Auditoria;

// ---- XML ----
public record ItemXml(int Numero, string Ean, string? Ncm, string? Cfop, string Unidade,
    decimal Quantidade, decimal ValorTotal);
public record NotaXml(string Chave, string Hash, string Modelo, string? CnpjEmitente,
    string? CnpjDestinatario, string? CStat, decimal ValorTotal,
    IReadOnlyList<ItemXml> Itens);
public record EventoXml(string Chave, string TipoEvento);
public record XmlLido(NotaXml? Nota, EventoXml? Evento);

// ---- ERP (o JSON é snake_case; o leitor converte) ----
public record ItemErp(string CodigoProduto, decimal Quantidade, decimal ValorTotal);
public record DocumentoErp(string Chave);
public record LancamentoErp(string IdLancamento, string Tipo, DocumentoErp Documento,
    decimal ValorTotal, List<ItemErp> Itens);
public record ArquivoErp(string Loja, List<LancamentoErp> Movimentacoes);

// ---- Cadastro de produtos ----
public record EmbalagemCompra(string Unidade, int Fator);
public record ProdutoCadastro(string Codigo, string Ean, string? Ncm, EmbalagemCompra? EmbalagemCompra);

// ---- Resultado: o quê, onde, quanto (R$) e por que importa ----
public record DivergenciaEncontrada(string Chave, int NumeroItem, string? Codigo, string? Ean,
    string Campo, string? ValorXml, string? ValorErp, GravidadeDivergencia Gravidade,
    decimal ImpactoReais, string Motivo);