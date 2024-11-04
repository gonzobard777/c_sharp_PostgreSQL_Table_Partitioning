using System.Linq.Expressions;
using Domain.Contract;
using Domain.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace Domain.Services;

public class BaseService<T> : IBaseService<T> where T : class
{
    protected IUnitOfWork UnitOfWork { get; }
    public IBaseRepository<T> Repository { get; }

    protected BaseService(IBaseRepository<T> repository, IUnitOfWork unitOfWork)
    {
        Repository = repository;
        UnitOfWork = unitOfWork;
    }

    public virtual async Task<T> CreateAsync(T model)
    {
        var entry = await Repository.CreateAsync(model);
        await UnitOfWork.CommitAsync();
        return entry.Entity;
    }

    // public virtual async Task<T> GetByIdAsync(int id) => await Repository.GetByIdAsync(id, EntityTracking.Disabled);
    //
    // public virtual async Task UpdateAsync(T model)
    // {
    //     Repository.Update(model);
    //     await UnitOfWork.CommitAsync();
    // }

    public virtual async Task DeleteAsync(int id)
    {
        throw new Exception("Not implemented");
        // var entity = await Repository.GetByIdAsync(id, EntityTracking.Disabled);
        // if (entity is null)
        //     throw new Exception(string.Format(CommonResource.NoEntityWithId, id));
        // Repository.Delete(entity);
        // await UnitOfWork.CommitAsync();
    }

    /*
     * Для списка единый алгоритм:
     *   - отфильтровать
     *   - отсортировать
     *   - пагинация
     *   - вернуть список и количество записей после фильтров
     */
    public virtual async Task<PagedResult<T>> List(ListQueryParams queryParams)
    {
        // Фильтрация. Все запросы должны применить одинаковые фильтры
        var filteredQuery = FilterList(
            Repository.NewQuery(EntityTracking.DisabledWithIdentityResolution),
            queryParams
        );

        var listQuery = AddIncludesForList(filteredQuery);

        // Сортировка
        listQuery = queryParams.SortBy is null
            ? SortDefault(listQuery)
            : Sort(listQuery, queryParams.SortBy, queryParams.Desc);

        // Пагинация
        listQuery = Paginate(listQuery, queryParams.Skip, queryParams.Take);

        var list = await listQuery.ToListAsync();
        var total = await filteredQuery.CountAsync();

        list = ListProcessing(list); // дополнительная обработка списка 

        return new PagedResult<T>(list, total);
    }


    // public virtual async Task Copy(int fromId, int toId)
    // {
    // }


    #region Сквозные методы

    public virtual IQueryable<T> AddIncludesForList(IQueryable<T> query) => query;

    public virtual IQueryable<T> FilterList(IQueryable<T> query, ListQueryParams queryParams) => query;

    // public virtual IQueryable<T> SortDefault(IQueryable<T> query) => query.OrderBy(x => x.Id);
    public virtual IQueryable<T> SortDefault(IQueryable<T> query) => query;

    public virtual List<T> ListProcessing(List<T> list) => list;

    #endregion Сквозные методы


    #region Common queries

    /// <summary>
    /// Универсальная сортировка.
    /// Сортировку по умолчанию надо реализовывать, переопределяя метод BaseService.SortDefault
    /// Потестировать сортировку можно здесь: https://github.com/gonzobard777/c_sharp_SortCheck
    /// </summary>
    /// <param name="query">Объект запроса.</param>
    /// <param name="propOrFieldName">Регистронезависимое название поля, по которому надо произвести сортировку.</param>
    /// <param name="desc">Направление сортировки.</param>
    /// <returns>Запрос с сортировкой.</returns>
    public IQueryable<T> Sort(IQueryable<T> query, string propOrFieldName, bool desc)
    {
        Type propType;
        LambdaExpression sortLambda;
        try
        {
            var param = Expression.Parameter(typeof(T));
            var prop = Expression.PropertyOrField(param, propOrFieldName);
            propType = prop.Type;
            sortLambda = Expression.Lambda(prop, param);
        }
        catch (Exception e)
        {
            var message = $"Unable to sort by \"{propOrFieldName}\". No property with this name";
            //TODO Логировать

            // при попытке сортировать по несуществующему свойству
            return SortDefault(query);
        }

        Expression<Func<IOrderedQueryable<T>>> sortMethod;
        bool isFirstSortingCondition = query.Expression.Type != typeof(IOrderedQueryable<T>);

        if (desc)
        {
            sortMethod = isFirstSortingCondition
                ? () => query.OrderByDescending<T, object>(k => null)
                : () => ((IOrderedQueryable<T>)query).ThenByDescending<T, object>(k => null);
        }
        else
        {
            sortMethod = isFirstSortingCondition
                ? () => query.OrderBy<T, object>(k => null)
                : () => ((IOrderedQueryable<T>)query).ThenBy<T, object>(k => null);
        }

        var methodCallExpression = sortMethod.Body as MethodCallExpression;
        if (methodCallExpression is null)
            throw new Exception("Sort. MethodCallExpression null");

        var method = methodCallExpression.Method.GetGenericMethodDefinition();
        var genericSortMethod = method.MakeGenericMethod(typeof(T), propType);
        return (IOrderedQueryable<T>)genericSortMethod.Invoke(query, new object[] { query, sortLambda });
    }

    public IQueryable<T> Paginate(IQueryable<T> query, int skip, int take)
    {
        return query
            .Skip(skip)
            .Take(take);
    }

    #endregion Common queries
}
