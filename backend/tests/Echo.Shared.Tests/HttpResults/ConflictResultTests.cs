using Echo.Shared.HttpResults;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Echo.Shared.Tests.HttpResults;

[Trait("Category", "Unit")]
public class ConflictResultTests
{
    [Fact]
    public void WithAnErrorCode_TheProblemCarriesItAsAnExtension()
    {
        var result = (ProblemHttpResult)new ConflictResult("This event is full.", "EVENT_FULL").ToResult();

        Assert.Equal(409, result.StatusCode);
        Assert.Equal("EVENT_FULL", result.ProblemDetails.Extensions["errorCode"]);
    }

    [Fact]
    public void WithoutAnErrorCode_NoExtensionIsAdded()
    {
        var result = (ProblemHttpResult)new ConflictResult("Already registered.").ToResult();

        Assert.False(result.ProblemDetails.Extensions.ContainsKey("errorCode"));
    }
}
