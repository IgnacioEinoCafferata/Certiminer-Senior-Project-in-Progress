using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Certiminer.Data;
using Certiminer.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Certiminer.Pages
{
    [Authorize]
    public class TakeTestModel : PageModel
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<IdentityUser> _um;

        public TakeTestModel(ApplicationDbContext db, UserManager<IdentityUser> um)
        {
            _db = db;
            _um = um;
        }

        public TestDto? Test { get; private set; }
        public List<QuestionVm> Questions { get; private set; } = new();

        public bool Submitted { get; private set; }
        public int Score { get; private set; }
        public int TotalQuestions { get; private set; }

        // ---------- GET ----------
        public async Task<IActionResult> OnGetAsync(int testId, CancellationToken ct = default)
        {
            await LoadTestAsync(testId, ct);
            if (Test is null) return NotFound();

            return Page();
        }

        // ---------- POST ----------
        public async Task<IActionResult> OnPostAsync(int testId, CancellationToken ct = default)
        {
            // Obtener usuario actual
            var user = await _um.GetUserAsync(User);
            if (user is null) return Unauthorized();

            await LoadTestAsync(testId, ct);
            if (Test is null) return NotFound();

            // Leer respuestas del form: ans_{questionId} = optionId
            var selected = new Dictionary<int, int>();
            foreach (var key in Request.Form.Keys)
            {
                if (key.StartsWith("ans_", StringComparison.OrdinalIgnoreCase))
                {
                    var qIdStr = key.Substring(4);
                    if (int.TryParse(qIdStr, out var qId) &&
                        int.TryParse(Request.Form[key], out var optId))
                    {
                        selected[qId] = optId;
                    }
                }
            }

            // Calcular puntaje
            Score = 0;
            TotalQuestions = Questions.Count;

            foreach (var q in Questions)
            {
                if (selected.TryGetValue(q.Id, out var chosen))
                    q.SelectedOptionId = chosen;

                if (q.SelectedOptionId.HasValue && q.SelectedOptionId == q.CorrectOptionId)
                    Score++;
            }

            // *** GUARDAR EN BASE DE DATOS ***
            var attempt = new TestAttempt
            {
                UserId = user.Id,
                TestId = testId,
                Score = Score,
                Total = TotalQuestions,
                CreatedAt = DateTime.UtcNow
            };

            _db.TestAttempts.Add(attempt);
            await _db.SaveChangesAsync(ct);

            // *** REDIRIGIR A PROGRESS ***
            TempData["LastTestScore"] = Score;
            TempData["LastTestTotal"] = TotalQuestions;
            TempData["LastTestTitle"] = Test.Title;

            return RedirectToPage("/Progress");
        }

        // ---------- Helpers ----------
        private async Task LoadTestAsync(int testId, CancellationToken ct)
        {
            // Cargar test activo
            var test = await _db.Tests
                .AsNoTracking()
                .Where(t => t.Id == testId && t.IsActive)
                .Select(t => new TestDto { Id = t.Id, Title = t.Title })
                .FirstOrDefaultAsync(ct);

            Test = test;
            if (Test is null)
            {
                Questions.Clear();
                return;
            }

            // Preguntas del test
            var qList = await _db.Questions
                .AsNoTracking()
                .Where(q => q.TestId == testId && q.IsActive)
                .OrderBy(q => q.Order)
                .Select(q => new QuestionVm
                {
                    Id = q.Id,
                    Text = q.Prompt
                })
                .ToListAsync(ct);

            Questions = qList;

            if (Questions.Count == 0) return;

            var qIds = Questions.Select(q => q.Id).ToList();

            // Opciones
            var options = await _db.AnswerQuestions
                .AsNoTracking()
                .Where(a => qIds.Contains(a.QuestionId))
                .OrderBy(a => a.Id)
                .Select(a => new OptionVm
                {
                    Id = a.Id,
                    QuestionId = a.QuestionId,
                    Text = a.Text,
                    IsCorrect = a.IsCorrect
                })
                .ToListAsync(ct);

            var byQ = options.GroupBy(o => o.QuestionId)
                             .ToDictionary(g => g.Key, g => g.ToList());

            foreach (var q in Questions)
            {
                if (byQ.TryGetValue(q.Id, out var list))
                {
                    q.Options = list;
                    q.CorrectOptionId = list.FirstOrDefault(o => o.IsCorrect)?.Id;
                }
            }
        }

        // ---------- VMs ----------
        public class TestDto
        {
            public int Id { get; set; }
            public string Title { get; set; } = string.Empty;
        }

        public class QuestionVm
        {
            public int Id { get; set; }
            public string Text { get; set; } = string.Empty;
            public List<OptionVm> Options { get; set; } = new();
            public int? CorrectOptionId { get; set; }
            public int? SelectedOptionId { get; set; }
        }

        public class OptionVm
        {
            public int Id { get; set; }
            public int QuestionId { get; set; }
            public string Text { get; set; } = string.Empty;
            public bool IsCorrect { get; set; }
        }
    }
}