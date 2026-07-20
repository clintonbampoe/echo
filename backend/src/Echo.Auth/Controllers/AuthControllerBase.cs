using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Echo.Auth.Controllers;

[ApiController]
[ApiVersion(1.0)]
[AllowAnonymous]
[Route("/api/auth/v{version:apiVersion}/[controller]")]
public abstract class AuthBaseController : ControllerBase { }
