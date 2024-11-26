using Microsoft.AspNetCore.Mvc;
using System.Data.Common;
using Microsoft.AspNetCore.Authorization;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace L_Bank_W_Backend.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class LBankInfoController : ControllerBase
    {
        [HttpGet]
        public Dictionary<string, string> Get()
        {
            var currentUser = HttpContext.User;
            var ret = new Dictionary<string, string> { ["name"] = "L-Bank.Web", ["version"] = "1" };
            if (currentUser != null)
            {
                foreach (var claim in currentUser.Claims)
                {
                    ret.Add(claim.Type, claim.Value);
                }
            }
            return ret;
        }
    }
}
