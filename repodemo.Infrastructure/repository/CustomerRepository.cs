using repodemo.Infrastructure.Models;

public class CustomerRepository : RepositoryBase<Customer>
{
    public CustomerRepository(CybersoftMarketplaceContext context) : base(context)
    {
    }
}
