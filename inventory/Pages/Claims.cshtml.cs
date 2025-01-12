
using System.Collections.Generic;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace inventory.Pages
{
    public class ClaimsModel : PageModel
    {
        public IEnumerable<Claim> UserClaims { get; set; }
        public void OnGet()
        {
            UserClaims = HttpContext.User.Claims;
        }
    }
}   