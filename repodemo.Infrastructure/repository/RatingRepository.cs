using repodemo.Infrastructure.Models;

public class RatingRepository : RepositoryBase<Rating>
{
    public RatingRepository(CybersoftMarketplaceContext context) : base(context)
    {
    }
}
