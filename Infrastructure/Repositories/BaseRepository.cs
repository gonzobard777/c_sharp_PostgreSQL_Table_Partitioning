using System.Linq.Expressions;
using Domain.Contract;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Infrastructure.Repositories;

public class BaseRepository<T> : IBaseRepository<T> where T : class
{
    protected DbSet<T> DbSet { get; }

    protected BaseRepository(MyDbContext dbContext)
    {
        DbSet = dbContext.Set<T>();
    }

    public async Task<EntityEntry<T>> CreateAsync(T entity) => await DbSet.AddAsync(entity);

    // public virtual async Task<T?> GetByIdAsync(int id, EntityTracking tracking) =>
    //     await NewQuery(tracking).FirstOrDefaultAsync(entity => entity.Id == id);

    // public virtual EntityEntry<T> Update(T entity)
    // {
    //     /*
    //      * Здесь будет создана новая сущность, если:
    //      *   - явно передали Id = 0
    //      *   - отсутствует Id (будет преобразовано в Id = 0)
    //      */
    //     if (entity.Id == 0)
    //         throw new Exception(BaseRepositoryResource.IdMustBeGreaterThan0);
    //     return DbSet.Update(entity);
    // }

    public virtual EntityEntry<T> Delete(T entity) => DbSet.Remove(entity);


    public IQueryable<T> NewQuery(EntityTracking tracking)
    {
        switch (tracking)
        {
            case EntityTracking.Disabled:
                return DbSet.AsNoTracking();
            case EntityTracking.DisabledWithIdentityResolution:
                return DbSet.AsNoTrackingWithIdentityResolution();
            default:
                return DbSet.AsQueryable();
        }
    }

    public IQueryable<T> GetSomeAsync(EntityTracking tracking, Func<IQueryable<T>, IOrderedQueryable<T>> orderBy = null, Expression<Func<T, bool>> predicate = null,
        params Expression<Func<T, object>>[] includes)
    {
        IQueryable<T> query = NewQuery(tracking);
        if (orderBy != null)
        {
            query = orderBy(query);
        }

        foreach (var navigationExpr in includes)
        {
            query = query.Include(navigationExpr);
        }

        if (predicate != null)
        {
            query = query.Where(predicate);
        }

        return query;
    }
}