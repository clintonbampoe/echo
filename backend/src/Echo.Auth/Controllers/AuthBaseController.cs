using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;

namespace Echo.Auth.Controllers;

[ApiController]
[ApiVersion(1.0)]
[Route("/api/auth/v{version:apiVersion}/[controller]")]
public abstract class AuthBaseController : ControllerBase { }
