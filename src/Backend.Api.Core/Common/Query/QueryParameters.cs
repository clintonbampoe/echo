namespace Backend.Api.Core.Common.Query;

public class QueryParameters
{
    public QueryParameters() { }

    public string? SearchTerm { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
}
