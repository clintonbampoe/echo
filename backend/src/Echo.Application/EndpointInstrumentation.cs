using System.Diagnostics;
using Echo.Shared.Extensions;
using Microsoft.AspNetCore.Http;

namespace Echo.Application;

public static class EndpointInstrumentation
{
    public static Activity? StartSpan(
        HttpContext context,
        ApplicationInstrumentation instrumentation,
        string spanName
    )
    {
        var activity = instrumentation.ActivitySource.StartActivity(spanName);
        var congregationId = context.User.GetCongregationId();

        activity?.SetTag("congregation.id", congregationId);
        activity?.SetTag("user.id", context.User.GetUserId());
        activity?.SetTag("user.role", context.User.GetUserRole());

        instrumentation.CongregationRequestVolume.Add(
            1,
            new TagList { { "congregation.id", congregationId } }
        );

        return activity;
    }
}
