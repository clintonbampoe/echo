using Echo.Domain.Assets;
using Echo.Domain.Attendances;
using Echo.Domain.Congregations;
using Echo.Domain.Projects;
using Echo.Domain.Transactions;
using Echo.Domain.Users;

namespace Echo.Seeder.Core;

/// <summary>
/// Everything the seeder knows about one congregation.
/// <br/><br/>
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
    public List<SessionLabel> SessionLabels { get; } = [];
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

    /// <summary>
    /// Sessions created by SessionsPhase, read by AttendancePhase.
    /// </summary>
    public List<Session> Sessions { get; } = [];

    /// <summary>
    /// Visitors who later converted to members. Keyed by PersonId, value is
    /// the instant of conversion.
    ///
    /// <br/><br/>
    ///
    /// Populated by PeoplePhase. AttendancePhase reads this to decide
    /// <c>PersonKindAtTime</c> for each attendance row: Visitor before the
    /// conversion instant, Member after.
    ///
    /// <br/><br/>
    ///
    /// This is why the snapshot works — attendance written before conversion
    /// stays attributed to the visitor role even though the person is now a
    /// member.
    /// </summary>
    public Dictionary<Guid, DateTime> ConvertedVisitorDates { get; } = [];

    public Guid Id => Congregation.Id;

    /// <summary>
    /// All person IDs (members + visitors).
    ///
    /// <br/><br/>
    ///
    /// Used by attendance, which records both kinds against the persons table.
    /// </summary>
    public IEnumerable<Guid> AllPersonIds => MemberIds.Concat(VisitorIds);
}
