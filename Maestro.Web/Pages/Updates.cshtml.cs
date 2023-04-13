using Maestro.Common;
using Maestro.Web;
using Maestro.Web.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Maestro.Web.Pages
{
    [IgnoreAntiforgeryToken]
    public class UpdatesModel : PageModel
    {
        public void OnGet()
        {

        }

        public void OnPost([FromBody] Aircraft aircraft)
        {
            Functions.Update(aircraft);
        }
    }
}
