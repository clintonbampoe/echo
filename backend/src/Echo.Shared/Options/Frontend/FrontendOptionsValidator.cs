using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace Echo.Shared.Options.Frontend;

public class FrontendOptionsValidator(IHostEnvironment environment)
    : IValidateOptions<FrontendOptions>
{
    public ValidateOptionsResult Validate(string? name, FrontendOptions options)
    {
        if (!environment.IsDevelopment() && !options.BaseUrl.StartsWith("https://"))
        {
            return ValidateOptionsResult.Fail(
                "FrontendClient:BaseUrl must use HTTPS in non-development environments."
            );
        }

        return ValidateOptionsResult.Success;
    }
}
