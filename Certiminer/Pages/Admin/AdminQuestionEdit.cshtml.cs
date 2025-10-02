using System.ComponentModel.DataAnnotations;
using Certiminer.Data;
using Certiminer.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Certiminer.Pages.Admin
{
    [Authorize(Policy = "AdminOnly")]
    public class AdminQuestionEditModel : PageModel
    {
        private readonly ApplicationDbContext _db;
        public AdminQuestionEditModel(ApplicationDbContext db) { _db = db; }

        // UI
        public bool IsEdit => Input.QuestionId.HasValue && Input.QuestionId.Value > 0;
        public string TestTitle { get; private set; } = "";
        public string TempError { get; private set; } = "";

        // ---------------- ViewModel ----------------
        public class OptionVM
        {
            public int Id { get; set; }
            public string Text { get; set; } = "";
        }

        public class InputVM
        {
            [Required] public int TestId { get; set; }
            public int? QuestionId { get; set; }

            [Required, StringLength(1024)]
            public string Prompt { get; set; } = "";

            public bool IsActive { get; set; } = true;

            // índice del radio seleccionado
            public int CorrectIndex { get; set; } = 0;

            // opciones dinámicas
            public List<OptionVM> Options { get; set; } = new();
        }

        [BindProperty] public InputVM Input { get; set; } = new();

        // -------------- GET --------------
        public async Task<IActionResult> OnGetAsync(int testId, int? questionId)
        {
            var test = await _db.Tests.FirstOrDefaultAsync(t => t.Id == testId);
            if (test == null) return NotFound();

            TestTitle = test.Title;

            if (questionId is null)
            {
                // Alta: arranco con 2 opciones vacías
                Input = new InputVM
                {
                    TestId = testId,
                    Prompt = "",
                    IsActive = true,
                    CorrectIndex = 0,
                    Options = new List<OptionVM>
                    {
                        new OptionVM(),
                        new OptionVM()
                    }
                };
            }
            else
            {
                var q = await _db.Questions
                                 .Include(x => x.Options)
                                 .FirstOrDefaultAsync(x => x.Id == questionId && x.TestId == testId);
                if (q == null) return NotFound();

                TestTitle = test.Title;

                var ordered = q.Options.OrderBy(o => o.Id).ToList();
                var correctIdx = ordered.FindIndex(o => o.IsCorrect);

                Input = new InputVM
                {
                    TestId = testId,
                    QuestionId = q.Id,
                    Prompt = q.Prompt,
                    IsActive = q.IsActive,
                    CorrectIndex = correctIdx < 0 ? 0 : correctIdx,
                    Options = ordered.Select(o => new OptionVM { Id = o.Id, Text = o.Text }).ToList()
                };

                if (Input.Options.Count < 2)
                {
                    // garantizo al menos 2 visible
                    while (Input.Options.Count < 2) Input.Options.Add(new OptionVM());
                }
            }

            return Page();
        }

        // -------------- POST --------------
        public async Task<IActionResult> OnPostAsync()
        {
            var test = await _db.Tests.FirstOrDefaultAsync(t => t.Id == Input.TestId);
            if (test == null) return NotFound();
            TestTitle = test.Title;

            // Normalizo/valido opciones
            var opts = (Input.Options ?? new()).Where(o => !string.IsNullOrWhiteSpace(o.Text))
                                               .Select(o => new OptionVM { Id = o.Id, Text = o.Text.Trim() })
                                               .ToList();

            if (opts.Count < 2)
            {
                TempError = "Please add at least 2 options.";
                // re-hidrato para mostrar lo que el usuario escribió
                Input.Options = Input.Options ?? new();
                if (Input.Options.Count < 2)
                    while (Input.Options.Count < 2) Input.Options.Add(new OptionVM());
                return Page();
            }

            if (Input.CorrectIndex < 0 || Input.CorrectIndex >= opts.Count)
            {
                TempError = "Select exactly one correct answer.";
                Input.Options = opts; // mostrar normalizado
                return Page();
            }

            if (!ModelState.IsValid)
            {
                Input.Options = opts;
                return Page();
            }

            if (Input.QuestionId is null)
            {
                // Alta
                var q = new Question
                {
                    TestId = Input.TestId,
                    Prompt = Input.Prompt.Trim(),
                    Type = QuestionType.SingleChoice,
                    Order = await _db.Questions.CountAsync(x => x.TestId == Input.TestId),
                    IsActive = Input.IsActive,
                };

                _db.Questions.Add(q);
                await _db.SaveChangesAsync();

                // opciones
                for (int i = 0; i < opts.Count; i++)
                {
                    _db.AnswerQuestions.Add(new AnswerQuestion
                    {
                        QuestionId = q.Id,
                        Text = opts[i].Text,
                        IsCorrect = (i == Input.CorrectIndex)
                    });
                }

                await _db.SaveChangesAsync();
            }
            else
            {
                // Edición
                var q = await _db.Questions.Include(x => x.Options)
                                           .FirstOrDefaultAsync(x => x.Id == Input.QuestionId && x.TestId == Input.TestId);
                if (q == null) return NotFound();

                q.Prompt = Input.Prompt.Trim();
                q.IsActive = Input.IsActive;

                // mapear existentes
                var postedIds = new HashSet<int>(opts.Select(o => o.Id));

                // actualizar/crear
                for (int i = 0; i < opts.Count; i++)
                {
                    var o = opts[i];
                    if (o.Id > 0)
                    {
                        var dbOpt = q.Options.First(x => x.Id == o.Id);
                        dbOpt.Text = o.Text;
                        dbOpt.IsCorrect = (i == Input.CorrectIndex);
                    }
                    else
                    {
                        _db.AnswerQuestions.Add(new AnswerQuestion
                        {
                            QuestionId = q.Id,
                            Text = o.Text,
                            IsCorrect = (i == Input.CorrectIndex)
                        });
                    }
                }

                // borrar las que ya no están
                var toRemove = q.Options.Where(x => !postedIds.Contains(x.Id)).ToList();
                if (toRemove.Count > 0)
                    _db.AnswerQuestions.RemoveRange(toRemove);

                await _db.SaveChangesAsync();
            }

            return RedirectToPage("/Admin/AdminQuestions", new { testId = Input.TestId });
        }
    }
}
