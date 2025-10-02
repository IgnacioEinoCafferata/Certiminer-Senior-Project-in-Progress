using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Certiminer.Data;
using Certiminer.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Certiminer.Pages
{
    [Authorize]
    public class ProgressModel : PageModel
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<IdentityUser> _um;
        public ProgressModel(ApplicationDbContext db, UserManager<IdentityUser> um) { _db = db; _um = um; }

        public int PercentComplete { get; private set; }

        public List<TestItem> Done { get; private set; } = new();
        public List<TestItem> Pending { get; private set; } = new();

        public class TestItem
        {
            public int TestId { get; set; }
            public string Title { get; set; } = "";
            public bool Taken { get; set; }
            public int? Score { get; set; }
            public int? Total { get; set; }
        }

        public async Task OnGetAsync()
        {
            var user = await _um.GetUserAsync(User);
            var uid = user!.Id;

            var tests = await _db.Tests
                                 .Where(t => t.IsActive)
                                 .OrderBy(t => t.Title)
                                 .ToListAsync();

            var attempts = await _db.TestAttempts
                                    .Where(a => a.UserId == uid)
                                    .ToListAsync();

            foreach (var t in tests)
            {
                var att = attempts.LastOrDefault(a => a.TestId == t.Id);
                var item = new TestItem
                {
                    TestId = t.Id,
                    Title = t.Title,
                    Taken = att != null,
                    Score = att?.Score,
                    Total = att?.Total
                };
                if (item.Taken) Done.Add(item); else Pending.Add(item);
            }

            var total = tests.Count;
            var done = Done.Count;
            PercentComplete = total == 0 ? 0 : (int)System.Math.Round(done * 100.0 / total);
        }
    }
}
