using System.Security.Claims;
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
    public class DepositsController(IBankService bankService) : ControllerBase
    {
        private readonly IBankService bankService = bankService;

        [HttpGet("/all")]
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

        [HttpPost]
        [Authorize(Roles = "Admin, User")]
        public async Task<ActionResult<DepositResponse>> MakeDeposit(
            [FromBody] DepositRequest depositRequest
        )
        {
            var requestorId = int.Parse(
                HttpContext.User.Claims.First(c => c.Type == ClaimTypes.UserData).Value
            );

            var result = await bankService.MakeDeposit(depositRequest, requestorId);
            if (result.Status == ServiceStatus.Success)
            {
                return result.Data;
            }
            else
            {
                return Problem(
                    detail: result.Message,
                    statusCode: ServiceStatusUtil.Map(result.Status),
                    title: "Error"
                );
            }
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "Admin, User")]
        public async Task<ActionResult<List<DepositResponse>>> GetDepositsByLedger(int id) { }

        [HttpGet]
        [Authorize(Roles = "Admin, User")]
        public async Task<ActionResult<List<DepositResponse>>> GetDepositsOfUser()
        {
            var requestorId = int.Parse(
                HttpContext.User.Claims.First(c => c.Type == ClaimTypes.UserData).Value
            );
        }
    }
}
