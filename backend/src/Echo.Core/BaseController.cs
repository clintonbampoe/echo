using Asp.Versioning;
using Echo.Application.Extensions;
using Echo.Domain.Users;
using Microsoft.AspNetCore.Mvc;

namespace Echo.Core;

[ApiController]
[ApiVersion(1.0)]
[Route("/api/v{version:apiVersion}/[controller]")]
public abstract class BaseController : ControllerBase
{
    protected Guid GetCongregationId() => User.GetCongregationId();
    protected Guid GetUserId() => User.GetUserId();
    protected UserRole GetRole => User.GetUserRole();
}
