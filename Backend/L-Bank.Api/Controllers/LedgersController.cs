using System.Security.Claims;
using L_Bank.Api.Dtos;
using L_Bank.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace L_Bank.Api.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    [Authorize(Roles = "User")]
    public class LedgersController : ControllerBase
    {
        private readonly IBankService bankService;

        public LedgersController(IBankService bankService)
        {
            this.bankService = bankService;
        }

        [HttpGet]
        [Authorize(Roles = "Admin, User")]
        public async Task<ActionResult<List<LedgerResponse>>> GetLedgers()
        {
            var user = HttpContext.User;

            var result = await bankService.GetUserWithLedgers(
                int.Parse(user.Claims.First(c => c.Type == ClaimTypes.UserData).Value)
            );

            if (!result.IsSuccess)
            {
                return Problem(
                    detail: result.Message,
                    statusCode: ServiceStatusUtil.Map(result.Status),
                    title: "Error"
                );
            }
            var ledgers = result
                .Data?.Ledgers.Select(l => new LedgerResponse
                {
                    Id = l.Id,
                    Name = l.Name,
                    Balance = l.Balance,
                })
                .ToList();

            return Ok(ledgers);
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<LedgerResponse>> GetLedger(int id)
        {
            var result = await bankService.GetLedger(id);

            if (!result.IsSuccess)
            {
                return Problem(
                    detail: result.Message,
                    statusCode: ServiceStatusUtil.Map(result.Status),
                    title: "Error"
                );
            }

            var ledger = result.Data;

            return Ok(
                new LedgerResponse
                {
                    Id = ledger.Id,
                    Name = ledger.Name,
                    Balance = ledger.Balance,
                }
            );
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<List<LedgerResponse>>> GetAllLedgers()
        {
            var result = await bankService.GetAllLedgers();

            if (!result.IsSuccess)
            {
                return Problem(
                    detail: result.Message,
                    statusCode: ServiceStatusUtil.Map(result.Status),
                    title: "Error"
                );
            }

            var ledgers = result
                .Data.Select(l => new LedgerResponse
                {
                    Id = l.Id,
                    Name = l.Name,
                    Balance = l.Balance,
                })
                .ToList();

            return Ok(ledgers);
        }

        [HttpPut]
        [Authorize(Roles = "Admin, User")]
        public async Task<ActionResult<LedgerResponse>> NewLedger(LedgerRequest request)
        {
            var userId = int.Parse(
                HttpContext.User.Claims.First(c => c.Type == ClaimTypes.UserData).Value
            );
            var result = await bankService.NewLedger(request, userId);

            if (result.Status != ServiceStatus.Success)
            {
                return Problem(
                    detail: result.Message,
                    statusCode: ServiceStatusUtil.Map(result.Status),
                    title: "Error"
                );
            }

            var ledger = result.Data;

            return Ok(
                new LedgerResponse
                {
                    Id = ledger.Id,
                    Name = ledger.Name,
                    Balance = ledger.Balance,
                }
            );
        }

        [HttpPut("{userId}")]
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

            var ledger = result.Data;

            return Ok(
                new LedgerResponse
                {
                    Id = ledger.Id,
                    Name = ledger.Name,
                    Balance = ledger.Balance,
                }
            );
        }

        [HttpGet("{ledgerId}/bookings")]
        [Authorize(Roles = "Admin, User")]
        public async Task<ActionResult<List<BookingResponse>>> GetBookingsForLedger(int ledgerId)
        {
            if (ledgerId <= 0)
            {
                return BadRequest("Invalid ledger id");
            }

            var userId = int.Parse(
                HttpContext.User.Claims.First(c => c.Type == ClaimTypes.UserData).Value
            );
            var isOwner = await bankService.LedgerBelongsToUser(ledgerId, userId);
            if (isOwner == false && !HttpContext.User.IsInRole("Admin"))
            {
                return Unauthorized();
            }
            var result = await bankService.GetBookingsForLedger(ledgerId);

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
    }
}
