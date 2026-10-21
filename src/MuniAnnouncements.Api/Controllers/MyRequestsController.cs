using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MuniAnnouncements.Api.Auth;
using MuniAnnouncements.Api.Models.Api;
using MuniAnnouncements.Api.Services;

namespace MuniAnnouncements.Api.Controllers;

[ApiController]
[Authorize]
public sealed class MyRequestsController : ControllerBase
{
    private readonly MyRequestsService _service;

    public MyRequestsController(MyRequestsService service)
    {
        _service = service;
    }

    [HttpGet("api/my-requests")]
    [ProducesResponseType(typeof(MyRequestsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<MyRequestsResponse>> GetMyRequests(CancellationToken ct)
    {
        var idNumber = User.FindFirstValue(JwtOptions.IdNumberClaim);
        if (string.IsNullOrWhiteSpace(idNumber))
        {
            return Forbid();
        }

        var response = await _service.GetMyRequestsAsync(idNumber, ct);
        return Ok(response);
    }
}
