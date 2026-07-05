using Echo.Auth.Controllers.Base;
using Echo.Auth.Dtos;
using Echo.Auth.Services;
using Microsoft.AspNetCore.Mvc;

namespace Echo.Auth.Controllers;

public class RegisterController(RegistrationService service) : AuthBaseController
{
    private readonly RegistrationService _service = service;

    [HttpPost]
    public async Task<ActionResult> RegisterCongregation(
        [FromBody] RegisterCongregationRequest request,
        CancellationToken ct
    )
    {
        var response = await _service.RegisterCongregation(
            request.CongregationDto,
            request.UserDto,
            ct
        );
        return response.ToActionResult();
    }
}
