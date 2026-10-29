using BillLocket.Application.DTOs;
using BillLocket.Application.Errors;
using BillLocket.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;

namespace BillLocket.Api.Controllers;
[ApiController]
[Authorize]
[Route("api/v1/users")]
public sealed class UsersController(IUserService userService) : ControllerBase
{
    [HttpPatch("me/username")]
    public async Task<IActionResult> UpdateUsername(
        [FromBody] UpdateUsernameRequest request,
        CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(JwtRegisteredClaimNames.Sub);

        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        var result = await userService.UpdateUsernameAsync(
            userId,
            request,
            cancellationToken);

        if (result.Succeeded)
        {
            return Ok(result.Value);
        }

        // TODO: Ánh xạ AppError thành HTTP status và ProblemDetails.
        var error = result.Error!;

        var statusCode = error.Code switch
        {
            var code when code == UserErrors.InvalidUsername.Code
                => StatusCodes.Status400BadRequest,

            var code when code == UserErrors.NotFound.Code
                => StatusCodes.Status404NotFound,

            var code when code == UserErrors.UsernameTaken.Code
                || code == UserErrors.UsernameUnchanged.Code
                || code == UserErrors.UsernameCooldown.Code
                || code == UserErrors.ConcurrencyConflict.Code
                => StatusCodes.Status409Conflict,

            _ => StatusCodes.Status500InternalServerError
        };

        var problem = new ProblemDetails
        {
            Status = statusCode,
            Title = error.Message
        };

        problem.Extensions["code"] = error.Code;

        if (error.Details is not null)
        {
            foreach (var detail in error.Details)
            {
                problem.Extensions[detail.Key] = detail.Value;
            }
        }

        return StatusCode(statusCode, problem);
    }
}
