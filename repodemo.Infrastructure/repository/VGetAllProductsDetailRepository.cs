using repodemo.Infrastructure.Models;

public class VGetAllProductsDetailRepository : RepositoryBase<VGetAllProductsDetail>
{
    public VGetAllProductsDetailRepository(CybersoftMarketplaceContext context) : base(context)
    {
    }
}
