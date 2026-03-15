using repodemo.Infrastructure.Models;

public class CategoryRepository : RepositoryBase<Category>
{
    public CategoryRepository(CybersoftMarketplaceContext context) : base(context)
    {
    }
}
