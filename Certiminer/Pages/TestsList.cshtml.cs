using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Certiminer.Data;
using Certiminer.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Certiminer.Pages
{
    [Authorize]
    public class TestsListModel : PageModel
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<IdentityUser> _um;
        public TestsListModel(ApplicationDbContext db, UserManager<IdentityUser> um)
        {
            _db = db; _um = um;
        }

        public class TestRow
        {
            public int Id { get; set; }
            public string Title { get; set; } = "";
            public bool IsActive { get; set; }
            public bool Completed { get; set; }
            public int? LastScore { get; set; }
            public int? LastTotal { get; set; }
        }

        public List<TestRow> Items { get; private set; } = new();

        public async Task OnGetAsync()
        {
            var user = await _um.GetUserAsync(User);
            var tests = await _db.Tests
                .OrderBy(t => t.Title)
                .ToListAsync();

            var attempts = await _db.TestAttempts
                .Where(a => a.UserId == user!.Id)
                .GroupBy(a => a.TestId)
                .Select(g => g.OrderByDescending(x => x.Id).FirstOrDefault()!)
                .ToListAsync();

            var byTest = attempts.ToDictionary(a => a.TestId, a => a);

            Items = tests.Select(t =>
            {
                byTest.TryGetValue(t.Id, out var at);
                return new TestRow
                {
                    Id = t.Id,
                    Title = t.Title,
                    IsActive = t.IsActive,
                    Completed = at != null && at.Total > 0, // o tu propia lógica
                    LastScore = at?.Score,
                    LastTotal = at?.Total
                };
            }).ToList();
        }
    }
}
