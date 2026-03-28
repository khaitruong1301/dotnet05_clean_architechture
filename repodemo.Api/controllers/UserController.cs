using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using repodemo.Infrastructure.Models;
using Azure;
//using repodemo.Api.Models;

namespace repodemo.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly ICartService _cartService;

        public UserController(IUserService userService, ICartService cartService)
        {
            _userService = userService;
            _cartService = cartService;
        }

        [HttpPost("login")]
        public async Task<ActionResult> Login([FromBody] UserLoginDTO userLogin)
        {
            var response = await _userService.Login(userLogin);
            return StatusCode(response.statusCode, response);
        }
        [HttpPost("register")]
        public async Task <ActionResult> Register ([FromBody] RegisterDTO userRegister)
        {
            ResponseData<RegisterDTO> res = await _userService.Register(userRegister);
            return StatusCode(res.statusCode, res);
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

        [Authorize]
        [HttpGet("get-cart-by-user-id")]
        public async Task<ActionResult> GetCartByUserId()
        {
            string token = HttpContext.Request.Headers["Authorization"].FirstOrDefault() ?? string.Empty;

            if (token.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            {
                token = token.Substring("Bearer ".Length).Trim();
            }

            ResponseData<CartDTO> response  = await _cartService.GetCartByUserId(token);
            return StatusCode(response.statusCode, response);
        }


        [Authorize]
        [HttpPost("add-to-cart")]
        public async Task<ActionResult> AddToCart([FromBody] ItemAddToCartDTO model)
        {
            string token = HttpContext.Request.Headers["Authorization"].FirstOrDefault() ?? string.Empty;

            if (token.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            {
                token = token.Substring("Bearer ".Length).Trim();
            }

            ResponseData<CartDTO> response = await _cartService.AddToCart(token, model);
            return StatusCode(response.statusCode, response);
        }

        [Authorize]
        [HttpDelete("remove-item-from-cart/{variantId}")]
        public async Task<ActionResult> RemoveItemFromCart([FromRoute] int  variantId)
        {
            string token = HttpContext.Request.Headers["Authorization"].FirstOrDefault() ?? string.Empty;

            if (token.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            {
                token = token.Substring("Bearer ".Length).Trim();
            }

            ResponseData<CartDTO> response = await _cartService.RemoveFromCart(token, variantId);
            return StatusCode(response.statusCode, response);
        }
        [Authorize]
        [HttpPut("update-cart-item")]
        public async Task<ActionResult> UpdateCartItem([FromBody] ItemUpdateToCartDTO model)
        {
            string token = HttpContext.Request.Headers["Authorization"].FirstOrDefault() ?? string.Empty;

            if (token.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            {
                token = token.Substring("Bearer ".Length).Trim();
            }

            ResponseData<CartDTO> response = await _cartService.UpdateCartItem(token, model);
            return StatusCode(response.statusCode, response);
        }
    }
}