using System;
using System.Collections.Generic;
using System.Linq;
using BManagedWeb.bsrv;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace BManagedWeb.Pages.Owner
{
    public class ReportsModel : PageModel
    {
        private readonly Service1Client _srv = new Service1Client();

        [BindProperty(SupportsGet = true)] public int Year  { get; set; } = DateTime.Today.Year;
        [BindProperty(SupportsGet = true)] public int Month { get; set; } = DateTime.Today.Month;
        [BindProperty(SupportsGet = true)] public string DisplayCurrency { get; set; } = "ILS";

        public VatSummary Vat { get; set; } = new VatSummary();
        public List<CustomerRevenueRow> TopCustomers { get; set; } = new();
        public List<ExpenseBreakdownRow> ExpenseBreakdown { get; set; } = new();

        public IEnumerable<SelectListItem> YearOptions =>
            Enumerable.Range(DateTime.Today.Year - 4, 5)
                      .Select(y => new SelectListItem(y.ToString(), y.ToString()));
        public IEnumerable<SelectListItem> MonthOptions =>
            Enumerable.Range(1, 12).Select(m => new SelectListItem(
                System.Globalization.DateTimeFormatInfo.InvariantInfo.GetMonthName(m), m.ToString()));

        public IActionResult OnGet()
        {
            var role = HttpContext.Session.GetString("Role");
            if (role != "Owner") return RedirectToPage("/Login");
            int id = HttpContext.Session.GetInt32("UserId") ?? 0;
            DisplayCurrency = string.IsNullOrEmpty(DisplayCurrency) ? "ILS" : DisplayCurrency;

            try
            {
                Vat = _srv.GetVatSummary(id, Year, Month, DisplayCurrency) ?? new VatSummary();
                var tc = _srv.GetTopCustomersByRevenue(id, DisplayCurrency);
                if (tc != null) TopCustomers = new List<CustomerRevenueRow>(tc);
                var first = new DateTime(Year, Month, 1);
                var last  = first.AddMonths(1).AddDays(-1);
                var bd = _srv.GetExpenseBreakdown(id, first, last, DisplayCurrency);
                if (bd != null) ExpenseBreakdown = new List<ExpenseBreakdownRow>(bd);
            }
            catch { }
            return Page();
        }
    }
}
