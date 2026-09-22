namespace Echo.Domain;

public interface ISoftDeletable
{
    public DateTime? DeletedAt { get; set; }
}
