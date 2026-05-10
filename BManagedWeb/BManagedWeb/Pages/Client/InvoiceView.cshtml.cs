using System.Collections.Generic;
using System.Linq;
using BManagedWeb.bsrv;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BManagedWeb.Pages.Client
{
    public class InvoiceViewModel : PageModel
    {
        private readonly Service1Client _srv = new Service1Client();
        public Invoice Invoice { get; set; }
        public List<InvoiceLine> Lines { get; set; } = new();

        public IActionResult OnGet(int id)
        {
            if (HttpContext.Session.GetString("Role") != "Client") return RedirectToPage("/Login");
            int clientId = HttpContext.Session.GetInt32("UserId") ?? 0;

            var candidate = _srv.GetInvoiceById(id);
            // Verify the invoice belongs to the logged-in client's customer account
            // before returning any data. The seeded demo links Client.Id == CustomerId;
            // a mismatch means the client is attempting to view another customer's invoice.
            if (candidate == null || candidate.CustomerId != clientId)
                return RedirectToPage("/Client/Portal");

            Invoice = candidate;
            Lines = (_srv.GetInvoiceLines(id) ?? new InvoiceLine[0]).ToList();
            return Page();
        }
    }
}
