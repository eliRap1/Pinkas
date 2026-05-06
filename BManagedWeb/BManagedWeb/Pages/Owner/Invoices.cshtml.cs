using System;
using System.Collections.Generic;
using System.Linq;
using BManagedWeb.bsrv;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BManagedWeb.Pages.Owner
{
    public class InvoicesModel : PageModel
    {
        private readonly Service1Client _srv = new Service1Client();

        public Invoice Selected { get; set; }
        public List<InvoiceLine> Lines { get; set; } = new();
        public List<Customer> Customers { get; set; } = new();
        public List<(Invoice, string)> AllInvoices { get; set; } = new();

        [BindProperty] public int    NewCustomerId { get; set; }
        [BindProperty] public DateTime NewDueDate  { get; set; } = DateTime.Today.AddDays(30);
        [BindProperty] public string NewCurrency   { get; set; } = "ILS";

        [BindProperty] public string  LineDescription { get; set; }
        [BindProperty] public double  LineQuantity    { get; set; } = 1.0;
        [BindProperty] public decimal LineUnitPrice   { get; set; }

        [BindProperty(SupportsGet = true)] public string Q { get; set; }
        [BindProperty(SupportsGet = true)] public string StatusFilter { get; set; }

        public IActionResult OnGet(int? id)
        {
            var role = HttpContext.Session.GetString("Role");
            if (role != "Owner") return RedirectToPage("/Login");
            int ownerId = HttpContext.Session.GetInt32("UserId") ?? 0;

            var custArr = _srv.GetCustomersForOwner(ownerId);
            Customers = custArr?.ToList() ?? new List<Customer>();

            if (id.HasValue && id.Value > 0)
            {
                Selected = _srv.GetInvoiceById(id.Value);
                Lines = (_srv.GetInvoiceLines(id.Value) ?? new InvoiceLine[0]).ToList();
            }
            else
            {
                foreach (var c in Customers)
                {
                    var arr = _srv.GetInvoicesByCustomer(c.Id) ?? new Invoice[0];
                    foreach (var inv in arr) AllInvoices.Add((inv, c.BusinessName));
                }
                if (!string.IsNullOrWhiteSpace(Q))
                {
                    var q = Q.Trim().ToLowerInvariant();
                    AllInvoices = AllInvoices
                        .Where(x => (x.Item1.InvoiceNumber ?? "").ToLowerInvariant().Contains(q) ||
                                    (x.Item2 ?? "").ToLowerInvariant().Contains(q))
                        .ToList();
                }
                if (!string.IsNullOrEmpty(StatusFilter))
                    AllInvoices = AllInvoices.Where(x => x.Item1.Status == StatusFilter).ToList();
                AllInvoices = AllInvoices.OrderByDescending(x => x.Item1.IssueDate).Take(40).ToList();
            }
            return Page();
        }

        public IActionResult OnPostCreate()
        {
            var role = HttpContext.Session.GetString("Role");
            if (role != "Owner") return RedirectToPage("/Login");
            if (NewCustomerId <= 0) return RedirectToPage();
            int newId = _srv.CreateInvoice(new Invoice
            {
                CustomerId = NewCustomerId,
                IssueDate = DateTime.Today,
                DueDate = NewDueDate,
                Currency = NewCurrency ?? "ILS",
                Status = "Draft",
                VatRate = 0.17,
            });
            return RedirectToPage("/Owner/Invoices", new { id = newId });
        }

        public IActionResult OnPostAddLine(int id)
        {
            if (HttpContext.Session.GetString("Role") != "Owner") return RedirectToPage("/Login");
            _srv.AddInvoiceLine(new InvoiceLine
            {
                InvoiceId = id,
                Description = LineDescription ?? "",
                Quantity = LineQuantity,
                UnitPrice = LineUnitPrice,
                LineTotal = (decimal)LineQuantity * LineUnitPrice,
                Currency = "ILS"
            });
            return RedirectToPage("/Owner/Invoices", new { id });
        }

        public IActionResult OnPostMarkSent(int id)
        {
            _srv.UpdateInvoiceStatus(id, "Sent");
            return RedirectToPage("/Owner/Invoices", new { id });
        }

        public IActionResult OnPostMarkPaid(int id)
        {
            _srv.MarkInvoicePaid(id, DateTime.Today);
            return RedirectToPage("/Owner/Invoices", new { id });
        }

        public IActionResult OnGetPdf(int id)
        {
            var bytes = _srv.GenerateInvoicePdf(id);
            return File(bytes, "application/pdf", $"INV-{id}.pdf");
        }
    }
}
