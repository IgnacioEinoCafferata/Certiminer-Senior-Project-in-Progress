using Certiminer.Data;
using Certiminer.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Certiminer.Infrastructure;
using System.ComponentModel.DataAnnotations;

namespace Certiminer.Pages.Admin
{
    [Authorize(Policy = "AdminOnly")]
    public class AdminVideoCreateModel : PageModel
    {
        private readonly ApplicationDbContext _db;
        public AdminVideoCreateModel(ApplicationDbContext db) => _db = db;

        public List<Test> Tests { get; private set; } = new();

        [BindProperty]
        public InputModel Input { get; set; } = new();

        public class InputModel
        {
            [Required, StringLength(256)]
            public string Title { get; set; } = string.Empty;

            [Required] // acepta URL o ID
            public string Url { get; set; } = string.Empty;

            [Required]
            public VideoSourceType SourceType { get; set; } = VideoSourceType.YouTube;

            public bool IsActive { get; set; } = true;

            public int? TestId { get; set; }
        }

        public async Task OnGetAsync(CancellationToken ct = default)
        {
            Tests = await _db.Tests.AsNoTracking().OrderBy(t => t.Title).ToListAsync(ct);
        }

        public async Task<IActionResult> OnPostAsync(CancellationToken ct = default)
        {
            Tests = await _db.Tests.AsNoTracking().OrderBy(t => t.Title).ToListAsync(ct);

            if (!ModelState.IsValid) return Page();

            var urlToSave = Input.Url;
            if (Input.SourceType == VideoSourceType.YouTube)
            {
                var id = YouTubeId.Extract(Input.Url);
                if (string.IsNullOrWhiteSpace(id))
                {
                    ModelState.AddModelError("Input.Url", "URL/ID de YouTube inválida.");
                    return Page();
                }
                urlToSave = id;
            }

            var entity = new Video
            {
                Title = Input.Title,
                Url = urlToSave,
                SourceType = (int)Input.SourceType,
                IsActive = Input.IsActive,
                TestId = Input.TestId
            };

            _db.Videos.Add(entity);
            await _db.SaveChangesAsync(ct);

            return RedirectToPage("/Admin/AdminVideos");
        }
    }
}

