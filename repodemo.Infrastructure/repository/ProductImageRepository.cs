using repodemo.Infrastructure.Models;

public class ProductImageRepository : RepositoryBase<ProductImage>
{
    public ProductImageRepository(CybersoftMarketplaceContext context) : base(context)
    {
    }
}
