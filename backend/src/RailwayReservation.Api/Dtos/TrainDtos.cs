using System.ComponentModel.DataAnnotations;

namespace RailwayReservation.Api.Dtos;

public sealed class CreateTrainRequest
{
    [Required, MaxLength(20)]
    public string Code { get; set; } = default!;

    [Required, MaxLength(120)]
    public string Name { get; set; } = default!;

    public string? Description { get; set; }
    public bool IsExpress { get; set; }
    public bool IsActive { get; set; } = true;
}

public sealed class UpdateTrainRequest
{
    [MaxLength(20)]
    public string? Code { get; set; }

    [MaxLength(120)]
    public string? Name { get; set; }

    public string? Description { get; set; }
    public bool? IsExpress { get; set; }
    public bool? IsActive { get; set; }
}
