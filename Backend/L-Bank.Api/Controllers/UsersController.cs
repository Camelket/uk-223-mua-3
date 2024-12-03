using System.Security.Claims;
using L_Bank.Api.Dtos;
using L_Bank.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace L_Bank.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController(IBankService bankService) : ControllerBase
    {
        private readonly IBankService bankService = bankService;

        [HttpGet("me")]
        [Authorize(Roles = "Admin, User")]
        public async Task<ActionResult<UserResponse>> GetMyself()
        {
            var requestorId = int.Parse(
                HttpContext.User.Claims.First(c => c.Type == ClaimTypes.UserData).Value
            );

            var result = await bankService.GetUserWithLedgers(requestorId);
            if (result.IsSuccess)
            {
                return Ok(result.Data);
            }

            return Problem(
                detail: result.Message,
                statusCode: ServiceStatusUtil.Map(result.Status),
                title: "Error"
            );
        }
    }
}
