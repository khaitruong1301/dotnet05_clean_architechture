using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using repodemo.Infrastructure.Models;
using Microsoft.AspNetCore.Authorization;
//using repodemo.Api.Models;

namespace repodemo.Api.Controllers
{
    [Authorize(Roles = "ADMIN")]
    [Route("api/[controller]")]
    [ApiController]
    public class AdminController : ControllerBase
    {
        private readonly IUserService _userService;
        public AdminController(IUserService userService)
        {
            _userService = userService;
            
        }
        // [Authorize(Roles = "ADMIN")]

        [HttpPost("create-user")]
        public async Task<ActionResult> createUser([FromBody]AddUserDTO userDTO)
        {
            var response = await _userService.AddUser(userDTO);

            return StatusCode(response.statusCode, response);
        }

    
    }
}