using Echo.Domain.Assets;
using Echo.Domain.Attendances;
using Echo.Domain.Congregations;
using Echo.Domain.Projects;
using Echo.Domain.Transactions;
using Echo.Domain.Users;

namespace Echo.Seeder.Core;

/// <summary>
/// Everything the seeder knows about one congregation.
///
/// Each phase reads what it needs from this record and writes its own output
/// back into it. The fields are populated in phase order — a field is only
/// valid after the phase that populates it has run.
/// </summary>
public sealed class CongregationSeed
{
    public required Congregation Congregation { get; init; }
    public required bool IsBig { get; init; }
    public required User Admin { get; init; }

    // Lookup entities (int PKs are populated by EF after SaveChanges)
    public List<AttendanceType> AttendanceTypes { get; } = [];
    public List<AssetCategory> AssetCategories { get; } = [];
    public List<ProjectCategory> ProjectCategories { get; } = [];
    public List<TransactionCategory> TransactionCategories { get; } = [];

    // MemberIds and VisitorIds are hashsets to prevent duplicates since they're both linked to Persons
    public HashSet<Guid> MemberIds { get; } = [];
    public HashSet<Guid> VisitorIds { get; } = [];
    public List<Guid> OrganizationIds { get; } = [];
    public List<Guid> EventIds { get; } = [];
    public List<Guid> ProjectIds { get; } = [];
    public List<Guid> AssetIds { get; } = [];

    public Guid Id => Congregation.Id;

    /// <summary>
    /// All person IDs (members + visitors). Used by attendance which records
    /// both kinds against the persons table.
    /// </summary>
    public IEnumerable<Guid> AllPersonIds => MemberIds.Concat(VisitorIds);
}
