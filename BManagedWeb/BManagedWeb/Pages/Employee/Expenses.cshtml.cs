using System;
using System.Collections.Generic;
using System.Linq;
using BManagedWeb.bsrv;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BManagedWeb.Pages.Employee
{
    public class ExpensesModel : PageModel
    {
        private readonly Service1Client _srv = new Service1Client();

        public List<Expense> Mine { get; set; } = new();
        public List<ExpenseCategory> Categories { get; set; } = new();

        [BindProperty] public int NewCategoryId { get; set; }
        [BindProperty] public string NewVendor { get; set; }
        [BindProperty] public decimal NewAmount { get; set; }

        public IActionResult OnGet()
        {
            if (HttpContext.Session.GetString("Role") != "Employee") return RedirectToPage("/Login");
            int id = HttpContext.Session.GetInt32("UserId") ?? 0;
            Categories = (_srv.GetExpenseCategories() ?? new ExpenseCategory[0]).ToList();
            Mine       = (_srv.GetExpensesByOwner(id) ?? new Expense[0]).ToList();
            return Page();
        }

        public IActionResult OnPostAdd()
        {
            if (HttpContext.Session.GetString("Role") != "Employee") return RedirectToPage("/Login");
            int id = HttpContext.Session.GetInt32("UserId") ?? 0;
            if (NewAmount <= 0) return RedirectToPage();
            _srv.AddExpense(new Expense
            {
                OwnerId = id,
                CategoryId = NewCategoryId > 0 ? NewCategoryId : (int?)null,
                Date = DateTime.Today,
                Amount = NewAmount,
                Vendor = NewVendor ?? "",
                Currency = HttpContext.Session.GetString("Currency") ?? "ILS",
            });
            return RedirectToPage();
        }
    }
}
