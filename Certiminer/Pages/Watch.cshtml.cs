using Certiminer.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Certiminer.Infrastructure;
using System.Threading;
using System.Threading.Tasks;

namespace Certiminer.Pages
{
    public class WatchModel : PageModel
    {
        private readonly ApplicationDbContext _db;
        public WatchModel(ApplicationDbContext db) => _db = db;

        public string Title { get; private set; } = "";
        public string? EmbedUrl { get; private set; }
        public int? TestId { get; private set; }

        public async Task<IActionResult> OnGetAsync(int id, CancellationToken ct = default)
        {
            var v = await _db.Videos
                            .AsNoTracking()
                            .Include(x => x.Test)
                            .FirstOrDefaultAsync(x => x.Id == id && x.IsActive, ct);

            if (v is null) return NotFound();

            Title = v.Title;
            TestId = v.TestId;

            if (v.SourceType == 1) // YouTube
            {
                var ytId = YouTubeId.Extract(v.Url);
                if (!string.IsNullOrWhiteSpace(ytId))
                {
                    var origin = $"{Request.Scheme}://{Request.Host}";
                    EmbedUrl = $"https://www.youtube-nocookie.com/embed/{ytId}?rel=0&modestbranding=1&origin={System.Uri.EscapeDataString(origin)}";
                }
            }

            return Page();
        }
    }
}
