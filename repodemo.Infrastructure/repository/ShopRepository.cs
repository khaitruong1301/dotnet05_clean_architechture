using repodemo.Infrastructure.Models;

public class ShopRepository : RepositoryBase<Shop>
{
    public ShopRepository(CybersoftMarketplaceContext context) : base(context)
    {
    }
}
