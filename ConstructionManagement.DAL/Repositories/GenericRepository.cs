using ConstructionManagement.DAL.Data;
using ConstructionManagement.DAL.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace ConstructionManagement.DAL.Repositories;

public class GenericRepository<T> : IGenericRepository<T> where T : class
{
    protected readonly AppDbContext Context;
    protected readonly DbSet<T> DbSet;

    public GenericRepository(AppDbContext context)
    {
        Context = context;
        DbSet = context.Set<T>();
    }

    public async Task<IReadOnlyList<T>> GetAllAsync() => await DbSet.ToListAsync();

    public async Task<IReadOnlyList<T>> GetAllAsync(Expression<Func<T, bool>> predicate) =>
        await DbSet.Where(predicate).ToListAsync();

    public async Task<T?> GetByIdAsync(object id) => await DbSet.FindAsync(id);

    public async Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate) =>
        await DbSet.FirstOrDefaultAsync(predicate);

    public async Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate) =>
        await DbSet.AnyAsync(predicate);

    public async Task AddAsync(T entity) => await DbSet.AddAsync(entity);

    public async Task AddRangeAsync(IEnumerable<T> entities) => await DbSet.AddRangeAsync(entities);

    public void Update(T entity) => DbSet.Update(entity);

    public void Remove(T entity) => DbSet.Remove(entity);

    public Task<int> SaveChangesAsync() => Context.SaveChangesAsync();
}
