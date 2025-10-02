using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Certiminer.Models;

namespace Certiminer.Repositories
{
    public interface IVideoRepository
    {
        Task<IList<Video>> GetAdminListAsync(CancellationToken ct = default);
        Task<IList<Video>> GetAdminListAsync(int? folderId, CancellationToken ct = default);
        Task<bool> DeleteAsync(int id, CancellationToken ct = default);

        Task<List<ChapterCount>> GetChaptersWithCountsAsync(CancellationToken ct = default);
        Task<Test?> GetActiveTestByIdAsync(int id, CancellationToken ct = default);
        Task<List<Video>> GetVideosForTestAsync(int testId, CancellationToken ct = default);

        Task<string> GetDatabaseNameAsync(CancellationToken ct = default);
    }
}
