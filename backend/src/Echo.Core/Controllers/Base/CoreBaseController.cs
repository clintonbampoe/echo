using Asp.Versioning;
using Echo.Application.Extensions;
using Echo.Domain.Enums;
using Microsoft.AspNetCore.Mvc;

namespace Echo.Core.Controllers.Base;

[ApiController]
[ApiVersion(1.0)]
[Route("/api/v{version:apiVersion}/[controller]")]
public abstract class CoreBaseController : ControllerBase
{
    protected Guid GetCongregationId() => User.GetCongregationId();
    protected Guid GetUserId() => User.GetUserId();
    protected UserRole GetRole => User.GetUserRole();
}
