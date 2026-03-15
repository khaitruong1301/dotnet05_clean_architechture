//Tạo class repository base class T

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using repodemo.Infrastructure.Models;   

public class RepositoryBase<T> where T : class
{
    private readonly CybersoftMarketplaceContext _context;
    private readonly DbSet<T> _dbSet;
    public RepositoryBase(CybersoftMarketplaceContext context)
    {
        _context = context;
        _dbSet = _context.Set<T>();
    }

    // public async Task<List<T>> GetAll()
    // {
    //     //Cách 1:
    //     var lstProduct = await _context.Products.ToListAsync();
    //     //cách 2
    //     var lstProduct2 = await _context.Set<Product>().ToListAsync();
    //     return await _dbSet.ToListAsync();
    // }
    public async Task<List<T>> GetAllAsync()
    {
        return await _dbSet.ToListAsync();
    }

    public async Task<T?> SingleOrDefaultAsync(Func<T, bool> predicate)
    {
        return await _dbSet.SingleOrDefaultAsync(e => predicate(e));
    }

    public async Task<List<T>> WhereAsync(Func<T, bool> predicate)
    {
        return await Task.Run(() => _dbSet.Where(predicate).ToList());
    }

    public async Task AddAsync(T entity)
    {
        await _dbSet.AddAsync(entity);
    }

    public async Task RemoveAsync(T entity)
    {
    
        _dbSet.Remove(entity);
    }

    public async Task UpdateAsync(T entity)
    {
        _dbSet.Update(entity);
    }
    

}