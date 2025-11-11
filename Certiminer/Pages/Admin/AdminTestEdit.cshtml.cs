using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Certiminer.Models;
using Certiminer.Repositories.Interfaces;   // <-- repos + UoW
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Certiminer.Pages.Admin
{
    [Authorize(Policy = "AdminOnly")]
    public class AdminTestEditModel : PageModel
    {
        private readonly ITestRepository _tests;
        private readonly IFolderRepository _folders;
        private readonly IUnitOfWork _uow;

        public AdminTestEditModel(ITestRepository tests, IFolderRepository folders, IUnitOfWork uow)
        {
            _tests = tests;
            _folders = folders;
            _uow = uow;
        }

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

        private async Task<List<Folder>> LoadChaptersAsync(CancellationToken ct)
        {
            // Traemos todos los folders con repo y filtramos por Kind = Chapters
            var all = await _folders.ListAsync(ct);
            return all.Where(f => f.Kind == FolderKind.Chapters)
                      .OrderBy(f => f.Name)
                      .ToList();
        }

        public async Task<IActionResult> OnGetAsync(int? id, CancellationToken ct = default)
        {
            Chapters = await LoadChaptersAsync(ct);

            if (id is null || id <= 0) return Page();

            var t = await _tests.GetByIdAsync(id.Value, includeQuestions: false, ct);
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

            if (Input.Id > 0)
            {
                var entity = await _tests.GetByIdAsync(Input.Id, ct: ct);
                if (entity is null) return NotFound();

                entity.Title = Input.Title.Trim();
                entity.IsActive = Input.IsActive;
                entity.FolderId = Input.FolderId;
                entity.ImageUrl = string.IsNullOrWhiteSpace(Input.ImageUrl) ? null : Input.ImageUrl.Trim();

                await _tests.UpdateAsync(entity);
            }
            else
            {
                var entity = new Test
                {
                    Title = Input.Title.Trim(),
                    IsActive = Input.IsActive,
                    FolderId = Input.FolderId,
                    ImageUrl = string.IsNullOrWhiteSpace(Input.ImageUrl) ? null : Input.ImageUrl.Trim()
                };
                await _tests.AddAsync(entity, ct);
            }

            await _uow.SaveChangesAsync(ct);

            if (!string.IsNullOrWhiteSpace(returnUrl))
                return Redirect(returnUrl);

            return RedirectToPage("/Admin/AdminTests");
        }
    }
}
