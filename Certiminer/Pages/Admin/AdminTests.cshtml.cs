using Certiminer.Data;
using Certiminer.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Certiminer.Repositories.Interfaces;

namespace Certiminer.Pages.Admin
{
    [Authorize(Policy = "AdminOnly")]
    public class AdminTestsModel : PageModel
    {
        private readonly ITestRepository _tests;
        public AdminTestsModel(ITestRepository tests) => _tests = tests;

        public List<Test> Items { get; private set; } = new();

        public async Task OnGetAsync(CancellationToken ct)
        {
            var all = await _tests.ListAsync(ct);
            Items = all.ToList();
        }
    }
}
