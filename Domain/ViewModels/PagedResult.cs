namespace Domain.ViewModels;

public class PagedResult<T> where T : class
{
    /*
     * Список сущностей.
     */
    public IEnumerable<T> List { get; set; }

    /*
     * Количество строк в результате после фильтров.
     */
    public int Total { get; set; }

    public PagedResult(IEnumerable<T> list, int total)
    {
        List = list;
        Total = total;
    }
}