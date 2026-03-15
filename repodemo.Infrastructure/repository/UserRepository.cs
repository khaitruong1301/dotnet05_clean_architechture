

using repodemo.Infrastructure.Models;
public class UserRepository : RepositoryBase<User>
{
    public UserRepository(CybersoftMarketplaceContext context) : base(context)
    {
    }

}
