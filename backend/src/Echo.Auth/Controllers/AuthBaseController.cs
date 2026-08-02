using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;

namespace Echo.Auth.Controllers;

[ApiController]
[ApiVersion(1.0)]
[Route("/auth/v{version:ApiVersion}/[controller]")]
public abstract class AuthBaseController : ControllerBase { }
