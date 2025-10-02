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
    public class VideosModel : PageModel
    {
        private readonly ApplicationDbContext _db;
        public VideosModel(ApplicationDbContext db) => _db = db;

        [FromQuery] public int? folderId { get; set; }      // /Videos?folderId=5
        [FromQuery] public bool unassigned { get; set; }    // /Videos?unassigned=true

        public Folder? Chapter { get; private set; }
        public List<Video> Items { get; private set; } = new();

        public async Task OnGetAsync(CancellationToken ct = default)
        {
            var q = _db.Videos.AsNoTracking().Where(v => v.IsActive);

            if (unassigned)
            {
                q = q.Where(v => v.FolderId == null);
            }
            else if (folderId is int fid)
            {
                Chapter = await _db.Folders.AsNoTracking()
                    .FirstOrDefaultAsync(f => f.Id == fid && f.Kind == FolderKind.Chapters, ct);

                q = q.Where(v => v.FolderId == fid);
            }

            Items = await q.OrderBy(v => v.Title).ToListAsync(ct);
        }
    }
}
