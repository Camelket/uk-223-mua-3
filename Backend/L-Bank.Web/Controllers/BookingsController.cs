using L_Bank_W_Backend.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using L_Bank_W_Backend.Core.Models;

namespace L_Bank_W_Backend.Controllers
{

    [ApiController]
    [Route("api/v1/[controller]")]
    public class BookingsController : ControllerBase
    {
        private readonly IBookingRepository bookingRepository;
        
        BookingsController(IBookingRepository bookingRepository)
        {
            this.bookingRepository = bookingRepository;
        }
        
        [HttpPost]
        [Authorize(Roles = "Administrators")]
        public async Task<IActionResult> Post([FromBody(EmptyBodyBehavior = EmptyBodyBehavior.Allow)] Booking booking)
        {
            return await Task.Run(() =>
            {
                IActionResult response = Ok();

                // Rufen Sie "book" im "BookingService auf.
                // Geben Sie je nach Erfolg OK() oder Conflict() zurück
                return response;
            });
        }
    }

}
