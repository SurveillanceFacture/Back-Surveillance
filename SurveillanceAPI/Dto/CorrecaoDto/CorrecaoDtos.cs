using System.ComponentModel.DataAnnotations;
using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using SurveillanceAPI.Dto.ApiUpload.Xml;

namespace SurveillanceAPI.Dto.CorrecaoDto;

// O JSON do ERP entra como JsonNode (cru) para que a correção preserve tudo que não for alterado.
public sealed class PropostaCorrecaoRequestDto
{
    [Required] public NotaFiscalDto? NotaFiscal { get; set; }

    [ValidateNever] public JsonNode? Erp { get; set; }
}

public sealed class AplicarCorrecoesRequestDto
{
    // Só o que o auditor marcou na tela. Não existe "aplicar tudo": cada id é uma decisão explícita.
    [Required, MinLength(1), MaxLength(500)]
    public List<Guid> PropostaIds { get; set; } = new();

    [Required] public NotaFiscalDto? NotaFiscal { get; set; }

    [ValidateNever] public JsonNode? Erp { get; set; }
}

public sealed class AlteracaoDto
{
    public string Campo { get; init; } = string.Empty;
    public string Antes { get; init; } = string.Empty;
    public string Depois { get; init; } = string.Empty;
    public bool Derivada { get; init; }
}

public sealed class PropostaItemDto
{
    public Guid PropostaId { get; init; }
    public int NumeroItem { get; init; }
    public string? Ean { get; init; }
    public string IdLancamento { get; init; } = string.Empty;
    public int IndiceItem { get; init; }
    public IReadOnlyList<AlteracaoDto> Alteracoes { get; init; } = [];
    public string Explicacao { get; init; } = string.Empty;
    public bool ExplicacaoGeradaPorIa { get; init; }
    public DateTime ExpiraEm { get; init; }
}

public sealed class ItemRevisaoDto
{
    public int? NumeroItem { get; init; }
    public string? Ean { get; init; }
    public string Campo { get; init; } = string.Empty;
    public string Gravidade { get; init; } = string.Empty;
    public string? ValorXml { get; init; }
    public string? ValorErp { get; init; }
    public string Transcricao { get; init; } = string.Empty;
    public bool GeradoPorIa { get; init; }
    public bool CorrecaoAutomatica { get; init; }
    public string? MotivoSemCorrecao { get; init; }
    public Guid? PropostaId { get; init; }
}

public sealed class PropostaCorrecaoResponseDto
{
    public long ComparacaoId { get; init; }
    public int TotalDivergencias { get; init; }
    public int TotalCorrigiveis { get; init; }
    public string Resumo { get; init; } = string.Empty;
    public bool ResumoGeradoPorIa { get; init; }
    public IReadOnlyList<ItemRevisaoDto> Itens { get; init; } = [];
    public IReadOnlyList<PropostaItemDto> Propostas { get; init; } = [];
}

public sealed class DivergenciaRestanteDto
{
    public int? NumeroItem { get; init; }
    public string? Ean { get; init; }
    public string Campo { get; init; } = string.Empty;
    public string Gravidade { get; init; } = string.Empty;
}

public sealed class AplicacaoCorrecaoResponseDto
{
    public int CorrecoesAplicadas { get; init; }
    public int DivergenciasResolvidas { get; init; }
    public string HashErpDepois { get; init; } = string.Empty;
    public IReadOnlyList<DivergenciaRestanteDto> Restantes { get; init; } = [];

    // Cópia corrigida do JSON do ERP, pronta para o auditor baixar e importar de volta no ERP.
    public JsonNode? ErpCorrigido { get; init; }
}
