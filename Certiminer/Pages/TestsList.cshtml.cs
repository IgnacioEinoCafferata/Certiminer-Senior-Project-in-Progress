using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Certiminer.Pages
{
    public class TestsListModel : PageModel
    {
        public IActionResult OnGet()
        {
            // Redirige siempre a la página nueva con fotos
            return RedirectToPage("/Tests");
        }
    }
}
