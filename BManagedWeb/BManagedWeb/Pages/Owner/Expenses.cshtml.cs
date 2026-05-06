using System;
using System.Collections.Generic;
using System.Linq;
using BManagedWeb.bsrv;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BManagedWeb.Pages.Owner
{
    public class ExpensesModel : PageModel
    {
        private readonly Service1Client _srv = new Service1Client();

        public List<Expense> Expenses { get; set; } = new();
        public List<ExpenseCategory> Categories { get; set; } = new();

        [BindProperty] public int NewCategoryId { get; set; }
        [BindProperty] public string NewVendor { get; set; }
        [BindProperty] public decimal NewAmount { get; set; }
        [BindProperty] public decimal NewVat { get; set; }
        [BindProperty] public string NewDescription { get; set; }
        [BindProperty] public string NewCurrency { get; set; } = "ILS";
        [BindProperty(SupportsGet = true)] public string Q { get; set; }

        public IActionResult OnGet()
        {
            var role = HttpContext.Session.GetString("Role");
            if (role != "Owner") return RedirectToPage("/Login");
            int id = HttpContext.Session.GetInt32("UserId") ?? 0;

            Categories = (_srv.GetExpenseCategories() ?? new ExpenseCategory[0]).ToList();
            Expenses   = (_srv.GetExpensesByOwner(id) ?? new Expense[0]).ToList();
            if (!string.IsNullOrWhiteSpace(Q))
            {
                var q = Q.Trim().ToLowerInvariant();
                Expenses = Expenses.Where(e =>
                    (e.Vendor ?? "").ToLowerInvariant().Contains(q) ||
                    (e.Description ?? "").ToLowerInvariant().Contains(q)).ToList();
            }
            return Page();
        }

        public IActionResult OnPostAdd()
        {
            var role = HttpContext.Session.GetString("Role");
            if (role != "Owner") return RedirectToPage("/Login");
            int id = HttpContext.Session.GetInt32("UserId") ?? 0;
            if (NewAmount <= 0) return RedirectToPage();

            _srv.AddExpense(new Expense
            {
                OwnerId = id,
                CategoryId = NewCategoryId > 0 ? NewCategoryId : (int?)null,
                Date = DateTime.Today,
                Amount = NewAmount,
                VatPaid = NewVat,
                Vendor = NewVendor ?? "",
                Description = NewDescription ?? "",
                Currency = NewCurrency ?? "ILS"
            });
            return RedirectToPage();
        }
    }
}
