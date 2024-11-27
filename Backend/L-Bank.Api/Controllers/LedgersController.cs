using System.Security.Claims;
using L_Bank.Api.Dtos;
using L_Bank.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
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

        [HttpGet("/all")]
        public async Task<ActionResult<List<LedgerResponse>>> GetLedgers()
        {
            var user = HttpContext.User;

            var result = await bankService.GetUser(
                int.Parse(user.Claims.First(c => c.Type == ClaimTypes.UserData).Value)
            );

            if (result.Status != ServiceStatus.Success)
            {
                // ?
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

        [HttpGet]
        
    }
}
