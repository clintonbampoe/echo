namespace Echo.Domain.Projects;

/// <summary>
/// How much was given to one project and in how many gifts, counted once and stored by the database
/// (the <c>project_totals</c> materialized view). The project list reads one row per project instead of adding
/// up every gift. Read-only, and up to the refresh interval behind the contributions table.
/// </summary>
public class ProjectTotals
{
    public Guid ProjectId { get; set; }
    public decimal Amount { get; set; }
    public int Contributions { get; set; }
}
