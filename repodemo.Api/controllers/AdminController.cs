using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using repodemo.Infrastructure.Models;
using Microsoft.AspNetCore.Authorization;
using Swashbuckle.AspNetCore.Annotations;
using Swashbuckle.AspNetCore.SwaggerGen;

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
 
   
        [HttpPost("create-user")]
           [SwaggerOperation(Summary = "Create a new user", Description = "This endpoint allows an admin to create a new user in the system.")]
        [SwaggerResponse(201, "User created successfully", typeof(ResponseData<User>))]
        [SwaggerResponse(400, "Bad request", typeof(ResponseData<string>))]
        public async Task<ActionResult> createUser([FromBody]AddUserDTO userDTO)
        {
            var response = await _userService.AddUser(userDTO);

            return StatusCode(response.statusCode, response);
        }
    }
}



