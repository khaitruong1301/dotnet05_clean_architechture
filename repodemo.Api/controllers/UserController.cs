using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
//using repodemo.Api.Models;

namespace repodemo.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpPost("login")]
        public async Task<ActionResult> Login([FromBody] UserLoginDTO userLogin)
        {
            var response = await _userService.Login(userLogin);
            return StatusCode(response.statusCode, response);
        }
        [Authorize]
        [HttpGet("get-user-profile")]
        public async Task<ActionResult> GetUserProfile()
        {
            //Tách chữ Bearer ra khỏi token nếu có (nếu client gửi lên header là
            //Authorization: Bearer <token>)
            string token = HttpContext.Request.Headers["Authorization"].FirstOrDefault() ?? string.Empty;
            
            if (token.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            {
                token = token.Substring("Bearer ".Length).Trim();
            }

            var response = await _userService.GetProfileByToken(token);
            return StatusCode(response.statusCode, response);
        }
    }
}