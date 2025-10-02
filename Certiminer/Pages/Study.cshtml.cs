using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Certiminer.Data;
using Certiminer.Models;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Certiminer.Pages
{
    public class StudyModel : PageModel
    {
        private readonly ApplicationDbContext _db;
        public StudyModel(ApplicationDbContext db) => _db = db;

        public record ChapterRow(int Id, string Title, int Count);

        public List<ChapterRow> Chapters { get; private set; } = new();
        public int UnassignedCount { get; private set; }

        public async Task OnGetAsync(CancellationToken ct = default)
        {
            // Todos los chapters (Folders.Kind = Chapters)
            var folders = await _db.Folders
                .AsNoTracking()
                .Where(f => f.Kind == FolderKind.Chapters)
                .OrderBy(f => f.Name)
                .ToListAsync(ct);

            // Conteos de videos por Chapter (FolderId)
            var counts = await _db.Videos
                .AsNoTracking()
                .Where(v => v.IsActive && v.FolderId != null)
                .GroupBy(v => v.FolderId!.Value)
                .Select(g => new { FolderId = g.Key, Cnt = g.Count() })
                .ToListAsync(ct);

            var map = counts.ToDictionary(x => x.FolderId, x => x.Cnt);

            Chapters = folders
                .Select(f => new ChapterRow(f.Id, f.Name, map.TryGetValue(f.Id, out var c) ? c : 0))
                .ToList();

            // Videos sin Chapter
            UnassignedCount = await _db.Videos
                .AsNoTracking()
                .CountAsync(v => v.IsActive && v.FolderId == null, ct);
        }
    }
}
