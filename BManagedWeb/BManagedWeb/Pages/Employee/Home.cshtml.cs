using System.Collections.Generic;
using BManagedWeb.bsrv;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BManagedWeb.Pages.Employee
{
    public class HomeModel : PageModel
    {
        private readonly Service1Client _srv = new Service1Client();

        public string Username { get; set; }
        public List<Project> Projects { get; set; } = new();

        public IActionResult OnGet()
        {
            var role = HttpContext.Session.GetString("Role");
            if (role != "Employee") return RedirectToPage("/Login");
            int id = HttpContext.Session.GetInt32("UserId") ?? 0;
            Username = HttpContext.Session.GetString("Username") ?? "";
            try
            {
                var list = _srv.GetProjectsForEmployee(id);
                if (list != null) Projects = new List<Project>(list);
            }
            catch { }
            return Page();
        }
    }
}
