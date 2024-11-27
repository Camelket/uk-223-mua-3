using L_Bank_W_Backend;
using L_Bank.Api.Dtos;
using L_Bank.Api.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace L_Bank.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService authService;

        [HttpPost("login")]
        public async Task<ActionResult<LoginResponse>> Login(LoginRequest request)
        {
            var result = await authService.Login(request);

            if (!result.IsSuccess)
            {
                return Problem(
                    detail: result.Message,
                    statusCode: StatusCodes.Status401Unauthorized,
                    title: "Error"
                );
            }

            return Ok(result.Data);
        }
    }
}
