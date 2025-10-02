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
        public AdminQuestionsModel(ApplicationDbContext db) { _db = db; }

        [BindProperty(SupportsGet = true)] public int TestId { get; set; }
        public string TestTitle { get; set; } = "";
        public List<Question> Items { get; set; } = new();

        public async Task<IActionResult> OnGetAsync()
        {
            var test = await _db.Tests.FirstOrDefaultAsync(x => x.Id == TestId);
            if (test == null) return NotFound();

            TestTitle = test.Title;
            Items = await _db.Questions
                             .Where(q => q.TestId == TestId)
                             .OrderBy(q => q.Order)
                             .ToListAsync();
            return Page();
        }

        public async Task<IActionResult> OnPostDeleteAsync(int questionId)
        {
            var q = await _db.Questions.FirstOrDefaultAsync(x => x.Id == questionId && x.TestId == TestId);
            if (q == null) return RedirectToPage(new { TestId });

            // borro opciones + pregunta
            var opts = _db.AnswerQuestions.Where(o => o.QuestionId == q.Id);
            _db.AnswerQuestions.RemoveRange(opts);
            _db.Questions.Remove(q);
            await _db.SaveChangesAsync();

            // reordenar
            var rest = await _db.Questions.Where(x => x.TestId == TestId)
                                          .OrderBy(x => x.Order)
                                          .ToListAsync();
            for (int i = 0; i < rest.Count; i++) rest[i].Order = i;
            await _db.SaveChangesAsync();

            return RedirectToPage(new { TestId });
        }
    }
}
