using repodemo.Infrastructure.Models;

public class OrderRepository : RepositoryBase<Order>
{
    public OrderRepository(CybersoftMarketplaceContext context) : base(context)
    {
    }
}
