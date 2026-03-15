using repodemo.Infrastructure.Models;

public class ConversationRepository : RepositoryBase<Conversation>
{
    public ConversationRepository(CybersoftMarketplaceContext context) : base(context)
    {
    }
}
