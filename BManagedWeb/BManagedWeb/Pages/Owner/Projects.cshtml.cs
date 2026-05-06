using System.Collections.Generic;
using System.Linq;
using BManagedWeb.bsrv;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BManagedWeb.Pages.Owner
{
    public class ProjectsModel : PageModel
    {
        private readonly Service1Client _srv = new Service1Client();

        [BindProperty(SupportsGet = true)] public string StatusFilter { get; set; }
        [BindProperty] public int NewCustomerId { get; set; }
        [BindProperty] public string NewTitle { get; set; }
        [BindProperty] public decimal NewBudget { get; set; }

        public List<Project> Projects { get; set; } = new();
        public List<Customer> Customers { get; set; } = new();

        public IActionResult OnGet()
        {
            var role = HttpContext.Session.GetString("Role");
            if (role != "Owner") return RedirectToPage("/Login");
            int id = HttpContext.Session.GetInt32("UserId") ?? 0;

            var custArr = _srv.GetCustomersForOwner(id);
            Customers = custArr?.ToList() ?? new List<Customer>();

            Project[] arr = string.IsNullOrEmpty(StatusFilter)
                ? Customers.SelectMany(c => _srv.GetProjectsByCustomer(c.Id) ?? new Project[0]).ToArray()
                : _srv.GetProjectsByStatus(StatusFilter, id);
            Projects = arr?.ToList() ?? new List<Project>();
            return Page();
        }

        public IActionResult OnPostAdd()
        {
            var role = HttpContext.Session.GetString("Role");
            if (role != "Owner") return RedirectToPage("/Login");
            if (NewCustomerId <= 0 || string.IsNullOrWhiteSpace(NewTitle)) return OnGet();
            var currency = HttpContext.Session.GetString("Currency") ?? "ILS";

            _srv.AddProject(new Project
            {
                CustomerId = NewCustomerId,
                Title = NewTitle,
                Status = "Active",
                StartDate = System.DateTime.Today,
                DueDate = System.DateTime.Today.AddDays(30),
                TotalBudget = NewBudget,
                Currency = currency
            });
            return RedirectToPage();
        }
    }
}
