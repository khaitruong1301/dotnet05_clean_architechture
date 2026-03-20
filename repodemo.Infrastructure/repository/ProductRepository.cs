
using Microsoft.EntityFrameworkCore;
using repodemo.Infrastructure.Models;
public class ProductRepository:RepositoryBase<Product>
{
    private readonly CybersoftMarketplaceContext _context;
    public ProductRepository(CybersoftMarketplaceContext context):base(context)
    {
        _context = context; 
    }

}