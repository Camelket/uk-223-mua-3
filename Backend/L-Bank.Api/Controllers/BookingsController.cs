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
    public class BookingsController(IBankService bankService) : ControllerBase
    {
        private readonly IBankService bankService = bankService;

        [HttpGet]
        [Authorize(Roles = "Admin, User")]
        public async Task<ActionResult<List<BookingResponse>>> GetBookings()
        {
            var requestorId = int.Parse(
                HttpContext.User.Claims.First(c => c.Type == ClaimTypes.UserData).Value
            );

            var result = await bankService.GetBookingsForUser(requestorId);
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
        public async Task<ActionResult<List<BookingResponse>>> GetAllBookings()
        {
            var result = await bankService.GetAllBookings();
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
        public async Task<ActionResult<BookingResponse>> GetSingleBooking(int id)
        {
            var requestorId = int.Parse(
                HttpContext.User.Claims.First(c => c.Type == ClaimTypes.UserData).Value
            );

            if (!HttpContext.User.IsInRole("Admin"))
            {
                var isAllowed = await bankService.BookingBelongsToUser(id, requestorId);
                if (!isAllowed)
                {
                    return Forbid();
                }
            }

            var result = await bankService.GetBooking(id);
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

        private async Task<ActionResult<BookingResponse>> _NewBooking(
            BookingRequest request,
            Func<BookingRequest, Task<DtoWrapper<BookingResponse>>> func
        )
        {
            var requestorId = int.Parse(
                HttpContext.User.Claims.First(c => c.Type == ClaimTypes.UserData).Value
            );

            if (!HttpContext.User.IsInRole("Admin"))
            {
                var isAllowed = await bankService.LedgerBelongsToUser(
                    request.SourceId,
                    requestorId
                );
                if (!isAllowed)
                {
                    return Forbid();
                }
            }
            var result = await func(request);
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

        [HttpPost("procedure")]
        [Authorize(Roles = "Admin, User")]
        public async Task<ActionResult<BookingResponse>> NewBookingWithProcedure(
            [FromBody] BookingRequest request
        )
        {
            return await _NewBooking(request, bankService.NewBookingWithProcedure);
        }

        [HttpPost]
        [Authorize(Roles = "Admin, User")]
        public async Task<ActionResult<BookingResponse>> NewBookingWithQueue(
            [FromBody] BookingRequest request
        )
        {
            return await _NewBooking(request, bankService.NewBookingWithQueue);
        }

        [HttpPost("procedure/queue")]
        [Authorize(Roles = "Admin, User")]
        public async Task<ActionResult<BookingResponse>> NewBookingWithProcedureAndQueue(
            [FromBody] BookingRequest request
        )
        {
            return await _NewBooking(request, bankService.NewBookingWithProcedureAndQueue);
        }
    }
}
