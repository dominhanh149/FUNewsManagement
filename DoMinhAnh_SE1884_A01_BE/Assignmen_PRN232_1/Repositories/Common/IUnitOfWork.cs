using Microsoft.EntityFrameworkCore.Storage;
using System.Threading.Tasks;

public interface IUnitOfWork
{
    Task<int> SaveChangesAsync();
    Task<IDbContextTransaction> BeginTransactionAsync();
    Task CommitTransactionAsync();
    Task RollbackTransactionAsync();
}
