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
    public class AdminQuestionsModel : PageModel
    {
        private readonly ApplicationDbContext _db;
        public AdminQuestionsModel(ApplicationDbContext db) => _db = db;

        public Test Test { get; private set; } = default!;
        public List<Question> Items { get; private set; } = new();

        // GET /Admin/AdminQuestions?testId=123
        public async Task<IActionResult> OnGetAsync(int testId, CancellationToken ct = default)
        {
            // Carga el test + preguntas
            Test = await _db.Tests
                .AsNoTracking()
                .Include(t => t.Questions)
                .FirstOrDefaultAsync(t => t.Id == testId, ct)
                ?? null!;

            if (Test == null)
                return NotFound();

            Items = Test.Questions
                .OrderBy(q => q.Order)
                .ThenBy(q => q.Id)
                .ToList();

            return Page();
        }

        // POST /Admin/AdminQuestions?handler=Delete&id=5&testId=123
        public async Task<IActionResult> OnPostDeleteAsync(int id, int testId, CancellationToken ct = default)
        {
            var q = await _db.Questions.FirstOrDefaultAsync(x => x.Id == id && x.TestId == testId, ct);
            if (q == null) return NotFound();

            _db.Questions.Remove(q);
            await _db.SaveChangesAsync(ct);

            return RedirectToPage("/Admin/AdminQuestions", new { testId });
        }
    }
}
