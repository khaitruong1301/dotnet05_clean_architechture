using repodemo.Infrastructure.Models;




public class UnitOfWork
{
    public readonly CybersoftMarketplaceContext _context;

    public UnitOfWork(CybersoftMarketplaceContext context)
    {
        _context = context;
    }


    public async Task BeginTransactionAsync()
    {
        await _context.Database.BeginTransactionAsync();
    }

    public async Task CommitTransaction()
    {
        await _context.Database.CommitTransactionAsync();
    }

    public async Task RollbackTransaction()
    {
        await _context.Database.RollbackTransactionAsync();
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }


}