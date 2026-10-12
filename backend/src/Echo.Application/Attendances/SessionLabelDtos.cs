using System.ComponentModel.DataAnnotations;
using Echo.Domain.Attendances;

namespace Echo.Application.Attendances;

public record SessionLabelCreateDto
{
    [Required, StringLength(100, MinimumLength = 1)]
    public required string Name { get; init; }
    public SessionCategory Category { get; init; }
}

public record SessionLabelUpdateDto
{
    [StringLength(100, MinimumLength = 1)]
    public string? Name { get; init; }

    // Kind is intentionally absent — it is not editable after creation.
}

public record SessionLabelResponseDto
{
    public int Id { get; init; }
    public required string Name { get; init; }
    public SessionCategory Category { get; init; }
}

public record SessionLabelSearchResultDto
{
    public int Id { get; init; }
    public required string Name { get; init; }
    public SessionCategory Category { get; init; }
}
