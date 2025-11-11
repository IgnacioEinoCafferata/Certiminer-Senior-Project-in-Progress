using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Certiminer.Models;
using Certiminer.Repositories.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Certiminer.Pages.Admin
{
    [Authorize(Policy = "AdminOnly")]
    public class AdminVideosModel : PageModel
    {
        private readonly IVideoRepository _repo;
        public AdminVideosModel(IVideoRepository repo) => _repo = repo;

        public IList<Video> Items { get; private set; } = new List<Video>();
        public string? DbName { get; private set; }
        public string? ErrorMsg { get; private set; }

        public async Task OnGetAsync(CancellationToken ct = default)
        {
            DbName = await _repo.GetDatabaseNameAsync(ct);    // nombre de DB efectiva
            try
            {
                Items = await _repo.GetAdminListAsync(ct);
            }
            catch (Exception ex)
            {
                ErrorMsg = ex.ToString(); // en vez de ex.Message
                Items = new List<Video>();
            }

        }


        public async Task<IActionResult> OnPostDeleteAsync(int id, CancellationToken ct = default)
        {
            var ok = await _repo.DeleteAsync(id, ct);
            if (!ok) return NotFound();
            return RedirectToPage();
        }
    }
}
