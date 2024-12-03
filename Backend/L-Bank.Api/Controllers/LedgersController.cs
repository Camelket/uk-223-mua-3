using System.Security.Claims;
using L_Bank.Api.Dtos;
using L_Bank.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace L_Bank.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LedgersController(IBankService bankService) : ControllerBase
    {
        private readonly IBankService bankService = bankService;

        [HttpGet]
        [Authorize(Roles = "Admin, User")]
        public async Task<ActionResult<List<LedgerResponse>>> GetLedgers()
        {
            var requestorId = int.Parse(
                HttpContext.User.Claims.First(c => c.Type == ClaimTypes.UserData).Value
            );

            var result = await bankService.GetUserWithLedgers(requestorId);

            if (result.IsSuccess)
            {
                return Ok(result.Data.Ledgers);
            }

            return Problem(
                detail: result.Message,
                statusCode: ServiceStatusUtil.Map(result.Status),
                title: "Error"
            );
        }

        [HttpGet("names")]
        [Authorize(Roles = "Admin, User")]
        public async Task<ActionResult<List<SimpleLedgerResponse>>> GetAllLedgersInSimpleForm()
        {
            var result = await bankService.GetAllLedgersInSimpleForm();
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

        [HttpGet("all")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<List<LedgerResponse>>> GetAllLedgers()
        {
            var result = await bankService.GetAllLedgers();
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

        [HttpGet("{id}")]
        [Authorize(Roles = "Admin, User")]
        public async Task<ActionResult<LedgerResponse>> GetLedger(int id)
        {
            var requestorId = int.Parse(
                HttpContext.User.Claims.First(c => c.Type == ClaimTypes.UserData).Value
            );

            if (!HttpContext.User.IsInRole("Admin"))
            {
                var isAllowed = await bankService.LedgerBelongsToUser(id, requestorId);
                if (!isAllowed)
                {
                    return Forbid();
                }
            }

            var result = await bankService.GetLedger(id);
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

        [HttpPost]
        [Authorize(Roles = "Admin, User")]
        public async Task<ActionResult<LedgerResponse>> NewLedger(LedgerRequest request)
        {
            var requestorId = int.Parse(
                HttpContext.User.Claims.First(c => c.Type == ClaimTypes.UserData).Value
            );
            var result = await bankService.NewLedger(request, requestorId);

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

        [HttpPost("users/{userId}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<LedgerResponse>> NewLedger(LedgerRequest request, int userId)
        {
            var result = await bankService.NewLedger(request, userId);

            if (result.Status != ServiceStatus.Success)
            {
                return Problem(
                    detail: result.Message,
                    statusCode: ServiceStatusUtil.Map(result.Status),
                    title: "Error"
                );
            }

            return Ok(result.Data);
        }

        [HttpGet("{ledgerId}/bookings")]
        [Authorize(Roles = "Admin, User")]
        public async Task<ActionResult<List<BookingResponse>>> GetBookingsForLedger(int ledgerId)
        {
            if (ledgerId <= 0)
            {
                return BadRequest("Invalid ledger id");
            }

            var requestorId = int.Parse(
                HttpContext.User.Claims.First(c => c.Type == ClaimTypes.UserData).Value
            );

            if (!HttpContext.User.IsInRole("Admin"))
            {
                var isAllowed = await bankService.LedgerBelongsToUser(ledgerId, requestorId);
                if (!isAllowed)
                {
                    return Forbid();
                }
            }

            var result = await bankService.GetBookingsForLedger(ledgerId);

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
