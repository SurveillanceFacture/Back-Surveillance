using System.ComponentModel.DataAnnotations;

namespace SurveillanceAPI.Dto.LojaDto;

public class FiltroLojaDto
{
    [Range(1, int.MaxValue)]
    public int Pagina { get; init; } = 1;

    [Range(1, 100)]
    public int Tamanho { get; init; } = 20;

    [StringLength(100)]
    public string? Busca { get; init; }      // feito para aceitar o nome(razao social, cnpj) 

    public bool? Ativo { get; init; }
}