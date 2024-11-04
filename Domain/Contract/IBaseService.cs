using Domain.ViewModels;

namespace Domain.Contract;

public interface IBaseService<T> where T : class
{
    Task<T> CreateAsync(T entity);
    // Task<T> GetByIdAsync(int id);
    // Task UpdateAsync(T entity);
    Task DeleteAsync(int id);

    Task<PagedResult<T>> List(ListQueryParams? queryParams);

    // Task Copy(int fromId, int toId);
}