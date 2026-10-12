namespace Echo.Domain.Attendances;

/// <summary>
/// Broad category of a session type, used for cross-congregation analytics.
/// <br/><br/>
/// This is separate from the user-facing Name on SessionLabel, which each
/// congregation can rename freely. However the Category never changes and is not exposed in
/// the client.
/// <br/>
/// It exists so that "how many attended services across all
/// congregations" stays answerable when some congregations call it
/// "Service" and others call it "Sunday Morning Worship".
/// </summary>
public enum SessionCategory
{
    Service,
    Meeting,
    Study,
    Prayer,
    Practice,
    Outreach,
    Class,
    Special,
}
