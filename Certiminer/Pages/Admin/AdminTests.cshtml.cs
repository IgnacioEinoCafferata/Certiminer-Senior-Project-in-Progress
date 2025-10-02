using Certiminer.Data;
using Certiminer.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Certiminer.Pages.Admin
{
    [Authorize(Policy = "AdminOnly")]
    public class AdminTestsModel : PageModel
    {
        private readonly ApplicationDbContext _db;
        public AdminTestsModel(ApplicationDbContext db) { _db = db; }

        public List<Test> Items { get; set; } = new();

        public async Task OnGetAsync()
        {
            Items = await _db.Tests.OrderBy(t => t.Title).ToListAsync();
        }
    }
}
