using System.Collections.Generic;
using System.Linq;
using BManagedWeb.bsrv;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BManagedWeb.Pages.Owner
{
    public class CustomersModel : PageModel
    {
        private readonly Service1Client _srv = new Service1Client();

        [BindProperty(SupportsGet = true)] public string Q { get; set; }

        [BindProperty] public string NewBusinessName { get; set; }
        [BindProperty] public string NewEmail { get; set; }
        [BindProperty] public string NewPhone { get; set; }
        [BindProperty] public string NewContactName { get; set; }
        [BindProperty] public string NewTaxId { get; set; }

        public List<Customer> Customers { get; set; } = new();
        public string Message { get; set; }
        public bool IsSuccess { get; set; }

        public IActionResult OnGet()
        {
            var role = HttpContext.Session.GetString("Role");
            if (role != "Owner") return RedirectToPage("/Login");
            int id = HttpContext.Session.GetInt32("UserId") ?? 0;
            Customer[] list = string.IsNullOrWhiteSpace(Q)
                ? _srv.GetCustomersForOwner(id)
                : _srv.SearchCustomers(Q, id);
            Customers = new List<Customer>(list ?? new Customer[0]);
            return Page();
        }

        public IActionResult OnPostAdd()
        {
            var role = HttpContext.Session.GetString("Role");
            if (role != "Owner") return RedirectToPage("/Login");
            int id = HttpContext.Session.GetInt32("UserId") ?? 0;
            var currency = HttpContext.Session.GetString("Currency") ?? "ILS";

            if (string.IsNullOrWhiteSpace(NewBusinessName))
            { Message = "Business name required"; IsSuccess = false; return OnGet(); }

            try
            {
                _srv.AddCustomer(new Customer
                {
                    BusinessName      = NewBusinessName,
                    ContactName       = NewContactName,
                    Email             = NewEmail,
                    Phone             = NewPhone,
                    TaxId             = NewTaxId,
                    OwnerId           = id,
                    PreferredCurrency = currency,
                });
                Message = $"Customer {NewBusinessName} added.";
                IsSuccess = true;
                NewBusinessName = NewEmail = NewPhone = NewContactName = NewTaxId = "";
            }
            catch (System.Exception ex) { Message = "Add failed: " + ex.Message; IsSuccess = false; }
            return OnGet();
        }

        // Quick-view modal — returns JSON with customer + projects + invoices + balance.
        public IActionResult OnGetDetail(int customerId)
        {
            var role = HttpContext.Session.GetString("Role");
            if (role != "Owner") return new JsonResult(new { error = "unauthorized" });
            try
            {
                var c = _srv.GetCustomerById(customerId);
                if (c == null) return new JsonResult(new { error = "not found" });

                var projects = _srv.GetProjectsByCustomer(customerId) ?? new Project[0];
                var invoices = _srv.GetInvoicesByCustomer(customerId) ?? new Invoice[0];
                decimal outstanding = invoices.Where(i => i.Status != "Paid").Sum(i => i.Total);
                decimal lifetime = invoices.Where(i => i.Status == "Paid").Sum(i => i.Total);

                return new JsonResult(new
                {
                    id           = c.Id,
                    businessName = c.BusinessName,
                    contactName  = c.ContactName ?? "",
                    email        = c.Email ?? "",
                    phone        = c.Phone ?? "",
                    taxId        = c.TaxId ?? "",
                    currency     = c.PreferredCurrency ?? "ILS",
                    outstanding  = outstanding,
                    lifetime     = lifetime,
                    projects = projects.Select(p => new
                    {
                        id       = p.Id,
                        title    = p.Title,
                        status   = p.Status,
                        budget   = p.TotalBudget,
                    }).ToArray(),
                    invoices = invoices.OrderByDescending(i => i.IssueDate).Take(8).Select(i => new
                    {
                        invoiceNumber = i.InvoiceNumber,
                        total         = i.Total,
                        currency      = i.Currency,
                        status        = i.Status,
                        issueDate     = i.IssueDate.ToString("dd MMM yyyy"),
                        dueDate       = i.DueDate.ToString("dd MMM yyyy"),
                    }).ToArray(),
                });
            }
            catch (System.Exception ex) { return new JsonResult(new { error = ex.Message }); }
        }
    }
}
