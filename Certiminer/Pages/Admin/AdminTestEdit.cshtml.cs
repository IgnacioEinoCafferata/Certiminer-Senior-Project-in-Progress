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

        [BindProperty] public InputModel Input { get; set; } = new();

        public class InputModel
        {
            public int Id { get; set; }

            [Required, StringLength(256)]
            public string Title { get; set; } = string.Empty;

            public bool IsActive { get; set; } = true;

            public int? FolderId { get; set; } // Chapter
        }

        public async Task<IActionResult> OnGetAsync(int? id, CancellationToken ct = default)
        {
            Chapters = await _db.Folders.AsNoTracking()
                .Where(f => f.Kind == FolderKind.Chapters)
                .OrderBy(f => f.Name).ToListAsync(ct);

            if (id is null || id <= 0) return Page();

            var t = await _db.Tests.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, ct);
            if (t is null) return NotFound();

            Input = new InputModel
            {
                Id = t.Id,
                Title = t.Title,
                IsActive = t.IsActive,
                FolderId = t.FolderId
            };

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(CancellationToken ct)
        {
            if (!ModelState.IsValid)
            {
                Chapters = await _db.Folders.AsNoTracking()
                    .Where(f => f.Kind == FolderKind.Chapters)
                    .OrderBy(f => f.Name).ToListAsync(ct);
                return Page();
            }

            Test entity;
            if (Input.Id > 0)
            {
                entity = await _db.Tests.FirstAsync(x => x.Id == Input.Id, ct);
            }
            else
            {
                entity = new Test();
                _db.Tests.Add(entity);
            }

            entity.Title = Input.Title.Trim();
            entity.IsActive = Input.IsActive;
            entity.FolderId = Input.FolderId;

            await _db.SaveChangesAsync(ct);
            return RedirectToPage("/Admin/AdminTests");
        }
    }
}
