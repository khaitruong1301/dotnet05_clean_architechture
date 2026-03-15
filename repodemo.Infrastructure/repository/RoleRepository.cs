using repodemo.Infrastructure.Models;

public class RoleRepository : RepositoryBase<Role>
{
    public RoleRepository(CybersoftMarketplaceContext context) : base(context)
    {
    }
}
