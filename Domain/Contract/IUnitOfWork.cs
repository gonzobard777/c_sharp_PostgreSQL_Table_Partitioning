namespace Domain.Contract;

public interface IUnitOfWork
{
    void Commit();
    Task CommitAsync();
}