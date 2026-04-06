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
    public class OrderController : ControllerBase
    {
        private readonly IOrderService _orderService;
        public OrderController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        [Authorize]
        [HttpPost("add-order")]
        public async Task<ActionResult<ResponseData<OrderDTO>>> AddOrder([FromBody] CartDTO cartDTO)
        {
              //Tách chữ Bearer ra khỏi token nếu có (nếu client gửi lên header là
            //Authorization: Bearer <token>)
             string token = HttpContext.Request.Headers["Authorization"].FirstOrDefault() ?? string.Empty;
            
            if (token.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            {
                token = token.Substring("Bearer ".Length).Trim();
            }
            ResponseData<OrderDTO>? result = await _orderService.AddOrder(token, cartDTO);
            return StatusCode(result.statusCode,  result);
        }


  
    }
}