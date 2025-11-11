using System.ComponentModel.DataAnnotations;
using Certiminer.Data;
using Certiminer.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Certiminer.Repositories.Interfaces;


namespace Certiminer.Pages.Admin
{
    [Authorize(Policy = "AdminOnly")]
    public class AdminQuestionEditModel : PageModel
    {
        private readonly ITestRepository _tests;

        private readonly ApplicationDbContext _db;
        public AdminQuestionEditModel(ApplicationDbContext db, ITestRepository tests)
        {
            _db = db;
            _tests = tests;
        }

        // querystring: /Admin/AdminQuestionEdit?testId=21&id=36
        [BindProperty(SupportsGet = true)] public int testId { get; set; }
        [BindProperty(SupportsGet = true)] public int? id { get; set; }

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

            // índice (0..N-1) de la respuesta correcta
            public int CorrectIndex { get; set; } = 0;

            public List<OptionVM> Options { get; set; } = new();
        }

        [BindProperty] public InputVM Input { get; set; } = new();

        // -------------- GET --------------
        public async Task<IActionResult> OnGetAsync(CancellationToken ct = default)
        {
            // título del test
            TestTitle = await _tests.GetTitleAsync(testId, ct) ?? "";


            if (id is null || id <= 0)
            {
                // Alta: 2 opciones vacías por defecto
                Input = new InputVM
                {
                    TestId = testId,
                    Prompt = "",
                    IsActive = true,
                    CorrectIndex = 0,
                    Options = new List<OptionVM> { new(), new() }
                };
                return Page();
            }

            // Edición: cargar pregunta
            var q = await _db.Questions.AsNoTracking()
                        .FirstOrDefaultAsync(x => x.Id == id.Value && x.TestId == testId, ct);
            if (q is null) return NotFound();

            // Cargar respuestas de AnswerQuestions por FK
            var answers = await _db.AnswerQuestions.AsNoTracking()
                               .Where(a => a.QuestionId == q.Id)
                               .OrderBy(a => a.Id)
                               .ToListAsync(ct);

            var correctIdx = answers.FindIndex(a => a.IsCorrect);

            Input = new InputVM
            {
                TestId = testId,
                QuestionId = q.Id,
                // En tu modelo la pregunta se guarda como "Text"; el VM usa "Prompt"
                Prompt = q.Prompt,
                IsActive = q.IsActive,
                CorrectIndex = correctIdx < 0 ? 0 : correctIdx,
                Options = answers.Select(a => new OptionVM { Id = a.Id, Text = a.Text }).ToList()
            };

            // garantizar al menos 2 opciones visibles
            while (Input.Options.Count < 2) Input.Options.Add(new OptionVM());

            return Page();
        }

        // -------------- POST --------------
        public async Task<IActionResult> OnPostAsync(CancellationToken ct = default)
        {
            TestTitle = await _tests.GetTitleAsync(Input.TestId, ct) ?? "";

            // Normalizar opciones (sólo las con texto)
            var opts = (Input.Options ?? new())
                        .Where(o => !string.IsNullOrWhiteSpace(o.Text))
                        .Select(o => new OptionVM { Id = o.Id, Text = o.Text.Trim() })
                        .ToList();

            if (opts.Count < 2)
            {
                TempError = "Please add at least 2 options.";
                Input.Options ??= new();
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

            // === Alta ===
            if (Input.QuestionId is null || Input.QuestionId <= 0)
            {
                var q = new Question
                {
                    TestId = Input.TestId,
                    Prompt = Input.Prompt.Trim(),             // mapeo Prompt -> Text
                    Type = QuestionType.SingleChoice,
                    Order = await _db.Questions.CountAsync(x => x.TestId == Input.TestId, ct),
                    IsActive = Input.IsActive,
                };

                _db.Questions.Add(q);
                await _db.SaveChangesAsync(ct);            // necesito el Id de la pregunta

                for (int i = 0; i < opts.Count; i++)
                {
                    _db.AnswerQuestions.Add(new AnswerQuestion
                    {
                        QuestionId = q.Id,
                        Text = opts[i].Text,
                        IsCorrect = (i == Input.CorrectIndex)
                    });
                }

                await _db.SaveChangesAsync(ct);
            }
            // === Edición ===
            else
            {
                var q = await _db.Questions
                                 .FirstOrDefaultAsync(x => x.Id == Input.QuestionId && x.TestId == Input.TestId, ct);
                if (q is null) return NotFound();

                q.Prompt = Input.Prompt.Trim();
                q.IsActive = Input.IsActive;

                // Traer opciones existentes desde la BD
                var existing = await _db.AnswerQuestions
                                        .Where(a => a.QuestionId == q.Id)
                                        .OrderBy(a => a.Id)
                                        .ToListAsync(ct);

                // Map para lookup rápido por Id
                var byId = existing.ToDictionary(a => a.Id);

                // IDs posteados (para borrado diferencial)
                var postedIds = new HashSet<int>(opts.Where(o => o.Id > 0).Select(o => o.Id));

                // actualizar o crear
                for (int i = 0; i < opts.Count; i++)
                {
                    var o = opts[i];
                    var isCorrect = (i == Input.CorrectIndex);

                    if (o.Id > 0 && byId.TryGetValue(o.Id, out var dbOpt))
                    {
                        dbOpt.Text = o.Text;
                        dbOpt.IsCorrect = isCorrect;
                    }
                    else
                    {
                        _db.AnswerQuestions.Add(new AnswerQuestion
                        {
                            QuestionId = q.Id,
                            Text = o.Text,
                            IsCorrect = isCorrect
                        });
                    }
                }

                // borrar las que ya no están
                var toRemove = existing.Where(a => !postedIds.Contains(a.Id)).ToList();
                if (toRemove.Count > 0)
                    _db.AnswerQuestions.RemoveRange(toRemove);

                await _db.SaveChangesAsync(ct);
            }

            return RedirectToPage("/Admin/AdminQuestions", new { testId = Input.TestId });
        }
    }
}
