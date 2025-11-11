using Certiminer.Models;

namespace Certiminer.Repositories.Interfaces;

public interface IFolderRepository
{
    Task<Folder?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<IReadOnlyList<Folder>> ListAsync(CancellationToken ct = default);
    Task AddAsync(Folder entity, CancellationToken ct = default);
    Task UpdateAsync(Folder entity);
    Task DeleteAsync(int id, CancellationToken ct = default);
}
