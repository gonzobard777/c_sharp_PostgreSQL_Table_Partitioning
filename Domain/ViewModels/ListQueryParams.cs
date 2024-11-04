namespace Domain.ViewModels;

public class ListQueryParams
{
    /*
     * Фильтрация
     */
    // public string? Search { get; set; }
    // public int? CompanyId { get; set; }
    // public int? LicenseId { get; set; }
    // public int? WorkplaceId { get; set; }
    // public int? WorkScheduleId { get; set; }

    /*
     * Сортировка
     */
    public string? SortBy { get; set; }
    public bool Desc { get; set; }

    /*
     * Пагинация
     */
    public int Skip { get; set; }
    public int Take { get; set; }
}