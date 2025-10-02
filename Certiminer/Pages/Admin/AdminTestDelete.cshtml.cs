using Certiminer.Data;
using Certiminer.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Certiminer.Pages.Admin
{
    [Authorize(Policy = "AdminOnly")]
    public class AdminTestDeleteModel : PageModel
    {
        private readonly ApplicationDbContext _db;
        public AdminTestDeleteModel(ApplicationDbContext db) { _db = db; }

        [BindProperty(SupportsGet = true)] public int TestId { get; set; }
        public string TestTitle { get; set; } = "";
        public int QuestionCount { get; set; }
        public int LinkedVideos { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            var t = await _db.Tests
                .Include(x => x.Questions)
                .Include(x => x.Videos)
                .FirstOrDefaultAsync(x => x.Id == TestId);

            if (t == null) return NotFound();

            TestTitle = t.Title;
            QuestionCount = t.Questions.Count;
            LinkedVideos = t.Videos.Count;
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var t = await _db.Tests.FirstOrDefaultAsync(x => x.Id == TestId);
            if (t != null)
            {
                _db.Tests.Remove(t);   // Cascade: borra Questions y AnswerQuestions; Videos quedan con TestId = null
                await _db.SaveChangesAsync();
            }
            return RedirectToPage("/Admin/AdminTests");
        }
    }
}
