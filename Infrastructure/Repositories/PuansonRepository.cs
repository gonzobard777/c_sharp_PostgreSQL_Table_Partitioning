using Domain.Contract;
using Domain.Entities;

namespace Infrastructure.Repositories;

public class PuansonRepository : BaseRepository<Puanson>, IPuansonRepository
{
    public PuansonRepository(MyDbContext dbContext) : base(dbContext)
    {
    }
}