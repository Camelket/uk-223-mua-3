using Azure.Core;
using L_Bank.Api.Dtos;
using L_Bank.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace L_Bank.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DepositsController(BankService bankService) : ControllerBase
    {
        private readonly BankService bankService = bankService;

        [HttpGet()]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<List<DepositResponse>>> GetAllDeposits()
        {
            var deposits = await bankService.GetAllDeposits();
            if (deposits.IsSuccess)
            {
                return Ok(deposits);
            }
            return Problem(
                detail: deposits.Message,
                statusCode: ServiceStatusUtil.Map(deposits.Status),
                title: "Error"
            );
        }
    }
}
