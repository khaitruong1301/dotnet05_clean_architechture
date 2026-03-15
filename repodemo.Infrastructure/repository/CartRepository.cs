using repodemo.Infrastructure.Models;

public class CartRepository : RepositoryBase<Cart>
{
    public CartRepository(CybersoftMarketplaceContext context) : base(context)
    {
    }
}
