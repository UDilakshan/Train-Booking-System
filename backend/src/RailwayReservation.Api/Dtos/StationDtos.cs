using System.ComponentModel.DataAnnotations;

namespace RailwayReservation.Api.Dtos;

public sealed class CreateStationRequest
{
    [Required, MaxLength(10)]
    public string Code { get; set; } = default!;

    [Required, MaxLength(120)]
    public string Name { get; set; } = default!;

    [Range(0, int.MaxValue)]
    public int Order { get; set; }

    [Range(0, double.MaxValue)]
    public decimal DistanceKm { get; set; }
}

public sealed class UpdateStationRequest
{
    [MaxLength(10)]
    public string? Code { get; set; }

    [MaxLength(120)]
    public string? Name { get; set; }

    public int? Order { get; set; }

    public decimal? DistanceKm { get; set; }
}
