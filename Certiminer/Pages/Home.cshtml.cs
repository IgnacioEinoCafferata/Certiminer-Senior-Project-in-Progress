using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Certiminer.Pages
{
    [Authorize]
    public class HomeModel : PageModel
    {
        public void OnGet()
        {
        }
    }
}
