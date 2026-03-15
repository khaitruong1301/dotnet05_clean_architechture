
using Microsoft.EntityFrameworkCore;
using repodemo.Infrastructure.Models;
public class ProductRepository
{
    private readonly CybersoftMarketplaceContext _context;
    public ProductRepository(CybersoftMarketplaceContext context)
    {
        _context = context;
    }

    public async Task<List<Product>> GetAllProduct()
    {
        return await _context.Products.Skip(0).Take(10).ToListAsync();
    }
    //Thêm xoá sửa tìm kiếm  (CRUD)

}