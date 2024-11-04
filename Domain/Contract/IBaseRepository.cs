using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Domain.Contract;

public interface IBaseRepository<T> where T : class
{
    Task<EntityEntry<T>> CreateAsync(T entity);
    // Task<T?> GetByIdAsync(int id, EntityTracking tracking);
    // EntityEntry<T> Update(T entity);
    EntityEntry<T> Delete(T entity);

    IQueryable<T> GetSomeAsync(EntityTracking tracking, Func<IQueryable<T>, IOrderedQueryable<T>> orderBy = null,
        Expression<Func<T, bool>> predicate = null,
        params Expression<Func<T, object>>[] includes);

    /*
     * Возвращает пустой IQueryable по конкретной таблице,
     * в который потом можно добавить все, что угодно.
     */
    IQueryable<T> NewQuery(EntityTracking tracking);
}