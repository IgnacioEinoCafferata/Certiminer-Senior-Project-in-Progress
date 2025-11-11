using Certiminer.Models;
using Certiminer.Repositories.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Certiminer.Pages.Admin
{
    [Authorize(Policy = "AdminOnly")]
    public class AdminQuestionsModel : PageModel
    {
        private readonly ITestRepository _tests;
        private readonly IQuestionRepository _questions;
        private readonly IUnitOfWork _uow;                        // <—

        public AdminQuestionsModel(
            ITestRepository tests,
            IQuestionRepository questions,
            IUnitOfWork uow)                                      // <—
        {
            _tests = tests;
            _questions = questions;
            _uow = uow;                                           // <—
        }

        [BindProperty(SupportsGet = true)] public int testId { get; set; }

        public Test? Test { get; private set; }
        public List<Question> Items { get; private set; } = new();

        public async Task<IActionResult> OnGetAsync(CancellationToken ct = default)
        {
            Test = await _tests.GetByIdAsync(testId, includeQuestions: false, ct);
            if (Test is null) return NotFound();

            var list = await _questions.ListByTestAsync(Test.Id, ct);
            Items = list.ToList();
            return Page();
        }

        // NEW: Delete question
        public async Task<IActionResult> OnPostDeleteAsync(int testId, int id, CancellationToken ct = default)
        {
            var q = await _questions.GetByIdAsync(id, ct);
            if (q is null || q.TestId != testId) return NotFound();

            await _questions.DeleteAsync(id, ct);
            await _uow.SaveChangesAsync(ct);

            TempData["ok"] = "Question deleted.";
            return RedirectToPage(new { testId });
        }
    }
}
