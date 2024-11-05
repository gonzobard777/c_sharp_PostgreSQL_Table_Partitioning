using Domain.Contract;
using Domain.Entities;
using Domain.ViewModels;

namespace Domain.Services;

public class PuansonService : BaseService<Puanson>, IPuansonService
{
    public PuansonService(IPuansonRepository repository, IUnitOfWork unitOfWork)
        : base(repository, unitOfWork)
    {
    }


    public override IQueryable<Puanson> FilterList(IQueryable<Puanson> query, ListQueryParams queryParams)
    {
        if (queryParams.FromDate != null && queryParams.ToDate != null)
        {
            query = query
                .Where(x => x.Date >= queryParams.FromDate && x.Date <= queryParams.ToDate);
        }
        else if (queryParams.FromDate != null)
        {
            query = query
                .Where(x => x.Date == queryParams.FromDate);
        }

        if (queryParams.StationIds != null && queryParams.StationIds.Count != 0)
        {
            query = query
                .Where(x => queryParams.StationIds.Contains(x.Id));
        }

        return query;
    }
}