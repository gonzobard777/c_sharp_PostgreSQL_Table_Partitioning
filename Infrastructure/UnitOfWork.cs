using Domain.Contract;

namespace Infrastructure;

public class UnitOfWork : IUnitOfWork
{
    private MyDbContext DbContext { get; }

    public UnitOfWork(MyDbContext dbContext) =>
        DbContext = dbContext;

    public void Commit() =>
        DbContext.SaveChanges();

    public async Task CommitAsync() =>
        await DbContext.SaveChangesAsync();
}