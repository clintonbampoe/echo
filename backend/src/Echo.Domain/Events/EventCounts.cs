namespace Echo.Domain.Events;

/// <summary>
/// How many registered for one event and how many came, counted once and stored by the database
/// (the <c>event_counts</c> materialized view). The event list reads one row per event instead of
/// counting every registration and check-in. Read-only, and up to the refresh interval behind the
/// registration and attendance tables.
/// </summary>
public class EventCounts
{
    public Guid EventId { get; set; }
    public int Registrations { get; set; }
    public int Attendees { get; set; }
}
