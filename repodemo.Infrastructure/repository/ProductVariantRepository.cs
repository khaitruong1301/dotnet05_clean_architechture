using repodemo.Infrastructure.Models;

public class ProductVariantRepository : RepositoryBase<ProductVariant>
{
    public ProductVariantRepository(CybersoftMarketplaceContext context) : base(context)
    {
    }
}
