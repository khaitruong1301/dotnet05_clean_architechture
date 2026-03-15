using repodemo.Infrastructure.Models;

public class CartItemRepository : RepositoryBase<CartItem>
{
    public CartItemRepository(CybersoftMarketplaceContext context) : base(context)
    {
    }
}
