using MeetingApp.Models.ReturnTypes.Abstract;
using MeetingApp.Models.ReturnTypes.Concrete;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Security.Claims;

namespace MeetingApp.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BaseApiController : ControllerBase
{
    [NonAction]
    public IActionResult HandleResult(Models.ReturnTypes.Abstract.IResult result)
    {
        if (result.IsSuccess) return Ok(result);

        ErrorResult errorResult = (ErrorResult)result;

        return errorResult.StatusCode switch
        {
            HttpStatusCode.Conflict => Conflict(result),
            HttpStatusCode.Forbidden => StatusCode((int)HttpStatusCode.Forbidden, result),
            HttpStatusCode.InternalServerError => StatusCode((int)HttpStatusCode.InternalServerError, result),
            HttpStatusCode.Unauthorized => Unauthorized(result),
            HttpStatusCode.NotFound => NotFound(result),
            HttpStatusCode.Gone => StatusCode((int)HttpStatusCode.Gone, result),
            _ => BadRequest(result),
        };
    }

    [NonAction]
    public Guid GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            throw new UnauthorizedAccessException("Kullanıcı kimliği bulunamadı");
        return userId;
    }
}