using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using repodemo.Infrastructure.Models;
//using repodemo.Api.Models;

namespace repodemo.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        private readonly CybersoftMarketplaceContext _context;
        private readonly IProductService _productService;
        public ProductController(CybersoftMarketplaceContext context,IProductService productService)
        {
            _context = context;
            _productService = productService;
        }

        [HttpGet("get-all-product")]
        public async Task<ActionResult> GetAllProduct()
        {
            //Code rất nhiều logic
            //Tái sử dụng phức tạp (copy code)
            //Khó bảo trì (sửa các chổ duplicate code)
            //Tạo class service để xử lý nghiệp vụ, controller chỉ gọi service (tách biệt rõ ràng)

            //repository pattern: Tạo class trung gian giữa service và database, class này sẽ chứa các phương thức truy xuất dữ liệu, service sẽ gọi class này để lấy dữ liệu, controller sẽ gọi service để lấy dữ liệu (tách biệt rõ ràng hơn nữa)
            return Ok(await _productService.GetAllProduct());
        }
        [HttpGet("get-product-by-id/{id}")]
        public async Task<ActionResult> GetProductById(int id)
        {
            var product = await _productService.GetProductById(id);
            if (product == null)
            {
                return NotFound();
            }
            return Ok(product);
        }
    }
}