using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Certiminer.Data;
using Certiminer.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Certiminer.Pages
{
    public class TestsModel : PageModel
    {
        private readonly ApplicationDbContext _db;
        public TestsModel(ApplicationDbContext db) => _db = db;

        [FromQuery] public int? folderId { get; set; }   // ?folderId=...
        public List<Folder> Chapters { get; private set; } = new();
        public List<Test> Tests { get; private set; } = new();
        public Folder? Current { get; private set; }

        public async Task OnGetAsync(CancellationToken ct = default)
        {
            if (folderId is int fid)
            {
                Current = await _db.Folders.AsNoTracking()
                    .FirstOrDefaultAsync(f => f.Id == fid && f.Kind == FolderKind.Chapters, ct);

                Tests = await _db.Tests.AsNoTracking()
                    .Where(t => t.IsActive && t.FolderId == fid)
                    .OrderBy(t => t.Title)
                    .ToListAsync(ct);
            }
            else
            {
                Chapters = await _db.Folders.AsNoTracking()
                    .Where(f => f.Kind == FolderKind.Chapters)
                    .OrderBy(f => f.Name)
                    .ToListAsync(ct);
            }
        }
    }
}

