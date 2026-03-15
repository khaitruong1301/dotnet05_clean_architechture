using repodemo.Infrastructure.Models;

public class MessageRepository : RepositoryBase<Message>
{
    public MessageRepository(CybersoftMarketplaceContext context) : base(context)
    {
    }
}
