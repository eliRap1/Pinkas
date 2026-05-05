using System.Collections.Generic;
using System.Linq;
using BManagedWeb.bsrv;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BManagedWeb.Pages.Client
{
    public class PortalModel : PageModel
    {
        private readonly Service1Client _srv = new Service1Client();

        public string Username { get; set; }
        public List<Invoice> Invoices { get; set; } = new();

        public IActionResult OnGet()
        {
            var role = HttpContext.Session.GetString("Role");
            if (role != "Client") return RedirectToPage("/Login");
            int id = HttpContext.Session.GetInt32("UserId") ?? 0;
            Username = HttpContext.Session.GetString("Username") ?? "";
            try
            {
                // Client.Email -> matched against Customer.Email -> get invoices.
                // For demo, the seeded Client user's id may double as customerId.
                var list = _srv.GetInvoicesByCustomer(id);
                if (list != null) Invoices = list.OrderByDescending(i => i.IssueDate).ToList();
            }
            catch { }
            return Page();
        }
    }
}
