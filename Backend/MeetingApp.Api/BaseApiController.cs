using MeetingApp.Models.ReturnTypes.Abstract;
using MeetingApp.Models.ReturnTypes.Concrete;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace MeetingApp.Api;

[ApiController]
[Route("[controller]")]
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
}