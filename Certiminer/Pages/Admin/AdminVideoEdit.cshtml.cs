using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Certiminer.Data;
using Certiminer.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace Certiminer.Pages.Admin
{
    [Authorize(Policy = "AdminOnly")]
    public class AdminVideoEditModel : PageModel
    {
        private readonly ApplicationDbContext _db;
        public AdminVideoEditModel(ApplicationDbContext db) => _db = db;

        public string PageTitle => Input.Id > 0 ? "Edit Video" : "Add Video";
        public List<Folder> Chapters { get; private set; } = new();

        [BindProperty]
        public InputModel Input { get; set; } = new();

        public class InputModel
        {
            public int Id { get; set; }

            [Required, StringLength(256)]
            public string Title { get; set; } = string.Empty;

            [Required, Url]
            public string Url { get; set; } = string.Empty;

            [Required]
            public int SourceType { get; set; } = 1; // 1=YouTube

            public bool IsActive { get; set; } = true;

            // Chapter
            public int? FolderId { get; set; }
        }

        public async Task<IActionResult> OnGetAsync(int? id, CancellationToken ct = default)
        {
            Chapters = await _db.Folders
                .AsNoTracking()
                .Where(f => f.Kind == FolderKind.Chapters)
                .OrderBy(f => f.Name)
                .ToListAsync(ct);

            if (id is null || id <= 0)
                return Page();

            var v = await _db.Videos.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, ct);
            if (v is null) return NotFound();

            Input = new InputModel
            {
                Id = v.Id,
                Title = v.Title,
                Url = v.Url,
                SourceType = v.SourceType,
                IsActive = v.IsActive,
                FolderId = v.FolderId
            };

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(CancellationToken ct)
        {
            if (!ModelState.IsValid) return Page();

            // Normalizar ID de YouTube si usás helper (opcional)
            // Input.Url = YouTubeId.Extract(Input.Url) ?? Input.Url;

            Video entity;
            if (Input.Id > 0)
            {
                entity = await _db.Videos.FirstAsync(v => v.Id == Input.Id, ct);
            }
            else
            {
                entity = new Video();
                _db.Videos.Add(entity);
            }

            entity.Title = Input.Title.Trim();
            entity.Url = Input.Url.Trim();
            entity.SourceType = Input.SourceType;
            entity.IsActive = Input.IsActive;
            entity.FolderId = Input.FolderId; // Chapter

            await _db.SaveChangesAsync(ct);
            return RedirectToPage("/Admin/AdminVideos");
        }
    }
}
