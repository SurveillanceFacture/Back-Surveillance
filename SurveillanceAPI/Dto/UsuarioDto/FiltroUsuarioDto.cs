using System.ComponentModel.DataAnnotations;

namespace SurveillanceAPI.Dto.UsuarioDto;

public class FiltroUsuarioDto
{
    [Range(1, int.MaxValue)]
    public int Pagina { get; init; } = 1;

    [Range(1, 100)]
    public int Tamanho { get; init; } = 20;

    [StringLength(100)]
    public string? Busca { get; init; }

    public int? TipoUsuarioId { get; init; }
    public bool? Ativo { get; init; }
}