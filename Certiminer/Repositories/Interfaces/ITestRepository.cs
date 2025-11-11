using Certiminer.Models;

namespace Certiminer.Repositories.Interfaces;

public interface ITestRepository
{
    Task<string?> GetTitleAsync(int id, CancellationToken ct = default);
    Task<Test?> GetByIdAsync(int id, bool includeQuestions = false, CancellationToken ct = default);
    Task<IReadOnlyList<Test>> ListAsync(CancellationToken ct = default);
    Task<IReadOnlyList<Test>> ListByFolderAsync(int folderId, CancellationToken ct = default); // usa Test.FolderId
    Task AddAsync(Test entity, CancellationToken ct = default);
    Task UpdateAsync(Test entity);
    Task DeleteAsync(int id, CancellationToken ct = default);
}
