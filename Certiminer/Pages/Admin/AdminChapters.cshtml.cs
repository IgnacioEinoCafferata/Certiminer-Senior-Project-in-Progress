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

namespace Certiminer.Pages.Admin
{
    [Authorize(Policy = "AdminOnly")]
    public class AdminChaptersModel : PageModel
    {
        private readonly ApplicationDbContext _db;
        public AdminChaptersModel(ApplicationDbContext db) => _db = db;

        public IList<Folder> Items { get; private set; } = new List<Folder>();

        [BindProperty] public string NewName { get; set; } = string.Empty;
        [BindProperty] public int RenameId { get; set; }
        [BindProperty] public string RenameTo { get; set; } = string.Empty;

        public async Task OnGetAsync(CancellationToken ct = default)
        {
            Items = await _db.Folders.AsNoTracking()
                .Where(f => f.Kind == FolderKind.Chapters)
                .OrderBy(f => f.Name)
                .ToListAsync(ct);
        }

        public async Task<IActionResult> OnPostCreateAsync(CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(NewName))
            {
                ModelState.AddModelError(nameof(NewName), "Name required");
                await OnGetAsync(ct);
                return Page();
            }

            var name = NewName.Trim();
            var exists = await _db.Folders
                .AnyAsync(f => f.Kind == FolderKind.Chapters && f.Name == name, ct);
            if (exists)
            {
                ModelState.AddModelError(nameof(NewName), "That chapter already exists.");
                await OnGetAsync(ct);
                return Page();
            }

            _db.Folders.Add(new Folder { Name = name, Kind = FolderKind.Chapters });
            await _db.SaveChangesAsync(ct);
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostRenameAsync(CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(RenameTo))
            {
                ModelState.AddModelError(nameof(RenameTo), "Name required");
                await OnGetAsync(ct);
                return Page();
            }

            var f = await _db.Folders
                .FirstOrDefaultAsync(x => x.Id == RenameId && x.Kind == FolderKind.Chapters, ct);
            if (f is null) return NotFound();

            f.Name = RenameTo.Trim();
            await _db.SaveChangesAsync(ct);
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostDeleteAsync(int id, CancellationToken ct = default)
        {
            var used = await _db.Tests.AnyAsync(t => t.FolderId == id, ct)
                       || await _db.Videos.AnyAsync(v => v.FolderId == id, ct);
            if (used)
            {
                TempData["Err"] = "Cannot delete: chapter is in use by tests and/or videos.";
                return RedirectToPage();
            }

            var f = await _db.Folders.FirstOrDefaultAsync(
                x => x.Id == id && x.Kind == FolderKind.Chapters, ct);
            if (f is null) return NotFound();

            _db.Folders.Remove(f);
            await _db.SaveChangesAsync(ct);
            return RedirectToPage();
        }
    }
}
