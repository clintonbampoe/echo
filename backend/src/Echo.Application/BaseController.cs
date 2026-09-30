using System.Diagnostics;
using Asp.Versioning;
using Echo.Domain.Users;
using Echo.Shared.Extensions;
using Microsoft.AspNetCore.Mvc;

namespace Echo.Application;

[ApiController]
[ApiVersion(1.0)]
[Route("/api/v{version:apiVersion}/[controller]")]
public abstract class BaseController : ControllerBase
{
    protected Guid GetCongregationId() => User.GetCongregationId();

    protected Guid GetUserId() => User.GetUserId();

    protected UserRole GetRole() => User.GetUserRole();

    protected Activity? StartEndpointSpan(
        ApplicationInstrumentation instrumentation,
        string spanName
    )
    {
        var activity = instrumentation.ActivitySource.StartActivity(spanName);

        activity?.SetTag("congregation.id", GetCongregationId());
        activity?.SetTag("user.id", GetUserId());
        activity?.SetTag("user.role", GetRole());

        return activity;
    }
}
