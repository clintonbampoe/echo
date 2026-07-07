using Echo.Application.Configuration;
using Microsoft.Extensions.Options;

namespace Echo.Application.Services;

public abstract class LinkBuilder(IOptions<FrontendClientOptions>  options)
{
    protected string BaseUrl { get; } = options.Value.BaseUrl.TrimEnd('/');
}
