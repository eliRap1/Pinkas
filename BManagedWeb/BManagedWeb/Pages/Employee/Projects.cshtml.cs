using System.Collections.Generic;
using System.Linq;
using BManagedWeb.bsrv;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BManagedWeb.Pages.Employee
{
    public class ProjectsModel : PageModel
    {
        private readonly Service1Client _srv = new Service1Client();
        public List<Project> Projects { get; set; } = new();

        public IActionResult OnGet()
        {
            if (HttpContext.Session.GetString("Role") != "Employee") return RedirectToPage("/Login");
            int id = HttpContext.Session.GetInt32("UserId") ?? 0;
            Projects = (_srv.GetProjectsForEmployee(id) ?? new Project[0]).ToList();
            return Page();
        }
    }
}
