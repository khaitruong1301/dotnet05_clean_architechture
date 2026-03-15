using repodemo.Infrastructure.Models;

public class OrderItemRepository : RepositoryBase<OrderItem>
{
    public OrderItemRepository(CybersoftMarketplaceContext context) : base(context)
    {
    }
}
