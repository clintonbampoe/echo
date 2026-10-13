namespace Echo.Application.Events;

public static class EventCapacity
{
    /// <summary>An event with no capacity is never full. Lowering a capacity below the registrations only keeps it full.</summary>
    public static bool IsFull(int? capacity, int registered) =>
        capacity is not null && registered >= capacity;
}
