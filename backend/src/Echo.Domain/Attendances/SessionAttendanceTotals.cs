namespace Echo.Domain.Attendances;

/// <summary>
/// Who was present at one session, counted once and stored by the database (the
/// <c>attendance_per_session</c> materialized view). Summaries add up a few rows per session
/// instead of counting every check-in. Read-only, and up to the refresh interval behind the
/// attendance table.
/// </summary>
public class SessionAttendanceTotals
{
    public Guid CongregationId { get; set; }
    public Guid SessionId { get; set; }
    public DateOnly SessionDate { get; set; }
    public int SessionLabelId { get; set; }
    public int Total { get; set; }
    public int Members { get; set; }
    public int Visitors { get; set; }
    public int FirstTime { get; set; }
}
