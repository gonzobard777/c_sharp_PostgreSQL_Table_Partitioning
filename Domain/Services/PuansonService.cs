using Domain.Contract;
using Domain.Entities;

namespace Domain.Services;

public class PuansonService : BaseService<Puanson>, IPuansonService
{
    public PuansonService(IPuansonRepository repository, IUnitOfWork unitOfWork)
        : base(repository, unitOfWork)
    {
    }
}