using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Certiminer.Data;
using Certiminer.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Certiminer.Pages.Admin
{
    [Authorize(Policy = "AdminOnly")]
    public class AdminTestEditModel : PageModel
    {
        private readonly ApplicationDbContext _db;
        public AdminTestEditModel(ApplicationDbContext db) => _db = db;

        public string PageTitle => Input.Id > 0 ? "Edit Test" : "Add Test";
        public List<Folder> Chapters { get; private set; } = new();

        // para volver a la lista de preguntas
        [BindProperty(SupportsGet = true)] public string? ReturnUrl { get; set; }

        [BindProperty] public InputModel Input { get; set; } = new();

        public class InputModel
        {
            public int Id { get; set; }

            [Required, StringLength(256)]
            public string Title { get; set; } = string.Empty;

            [StringLength(2048)]
            public string? ImageUrl { get; set; }

            public bool IsActive { get; set; } = true;
            public int? FolderId { get; set; } // Chapter
        }

        private Task<List<Folder>> LoadChaptersAsync(CancellationToken ct) =>
            _db.Folders.AsNoTracking()
               .Where(f => f.Kind == FolderKind.Chapters)
               .OrderBy(f => f.Name)
               .ToListAsync(ct);

        public async Task<IActionResult> OnGetAsync(int? id, CancellationToken ct = default)
        {
            Chapters = await LoadChaptersAsync(ct);

            if (id is null || id <= 0) return Page();

            var t = await _db.Tests.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, ct);
            if (t is null) return NotFound();

            Input = new InputModel
            {
                Id = t.Id,
                Title = t.Title,
                ImageUrl = t.ImageUrl,
                IsActive = t.IsActive,
                FolderId = t.FolderId
            };

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(string? returnUrl, CancellationToken ct)
        {
            Chapters = await LoadChaptersAsync(ct); // por si hay que redisplayear el form

            if (!ModelState.IsValid) return Page();

            Test entity;
            if (Input.Id > 0)
            {
                entity = await _db.Tests.FirstOrDefaultAsync(x => x.Id == Input.Id, ct)
                         ?? throw new KeyNotFoundException($"Test {Input.Id} not found");
            }
            else
            {
                entity = new Test();
                _db.Tests.Add(entity);
            }

            entity.Title = Input.Title.Trim();
            entity.IsActive = Input.IsActive;
            entity.FolderId = Input.FolderId;
            entity.ImageUrl = string.IsNullOrWhiteSpace(Input.ImageUrl) ? null : Input.ImageUrl.Trim();

            await _db.SaveChangesAsync(ct);

            // Volver a donde viniste (por ejemplo /Admin/AdminQuestions?testId=20)
            if (!string.IsNullOrWhiteSpace(returnUrl))
                return Redirect(returnUrl);

            return RedirectToPage("/Admin/AdminTests");
        }
    }
}
