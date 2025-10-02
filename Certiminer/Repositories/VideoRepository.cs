using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Certiminer.Data;
using Certiminer.Models;
using Microsoft.EntityFrameworkCore;

namespace Certiminer.Repositories
{
    public class VideoRepository : IVideoRepository
    {
        private readonly ApplicationDbContext _db;
        public VideoRepository(ApplicationDbContext db) => _db = db;

        public async Task<IList<Video>> GetAdminListAsync(CancellationToken ct = default)
        {
            return await _db.Videos
                            .AsNoTracking()
                            .OrderBy(v => v.Title)
                            .ToListAsync(ct);
        }

        public async Task<IList<Video>> GetAdminListAsync(int? folderId, CancellationToken ct = default)
        {
            var q = _db.Videos.AsNoTracking();
            if (folderId != null)
                q = q.Where(v => v.FolderId == folderId);
            return await q.OrderBy(v => v.Title).ToListAsync(ct);
        }

        public async Task<bool> DeleteAsync(int id, CancellationToken ct = default)
        {
            var v = await _db.Videos.FindAsync(new object[] { id }, ct);
            if (v == null) return false;
            _db.Videos.Remove(v);
            await _db.SaveChangesAsync(ct);
            return true;
        }

        public async Task<List<ChapterCount>> GetChaptersWithCountsAsync(CancellationToken ct = default)
        {
            var tests = await _db.Tests
                                 .AsNoTracking()
                                 .Where(t => t.IsActive)
                                 .OrderBy(t => t.Title)
                                 .ToListAsync(ct);

            var counts = await _db.Videos
                                  .AsNoTracking()
                                  .Where(v => v.IsActive && v.TestId != null)
                                  .GroupBy(v => v.TestId!.Value)
                                  .Select(g => new { TestId = g.Key, Cnt = g.Count() })
                                  .ToListAsync(ct);

            var map = counts.ToDictionary(x => x.TestId, x => x.Cnt);

            var result = new List<ChapterCount>(tests.Count);
            foreach (var t in tests)
            {
                map.TryGetValue(t.Id, out var cnt);
                result.Add(new ChapterCount { Test = t, Count = cnt });
            }
            return result;
        }

        public async Task<Test?> GetActiveTestByIdAsync(int id, CancellationToken ct = default)
        {
            return await _db.Tests
                            .AsNoTracking()
                            .FirstOrDefaultAsync(t => t.Id == id && t.IsActive, ct);
        }

        public async Task<List<Video>> GetVideosForTestAsync(int testId, CancellationToken ct = default)
        {
            return await _db.Videos
                            .AsNoTracking()
                            .Where(v => v.IsActive && v.TestId == testId)
                            .OrderBy(v => v.Title)
                            .ToListAsync(ct);
        }

        public Task<string> GetDatabaseNameAsync(CancellationToken ct = default)
        {
            // Works for SQL Server
            var conn = _db.Database.GetDbConnection();
            return Task.FromResult(conn.Database);
        }
    }
}
