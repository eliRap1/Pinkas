using System.Collections.Generic;
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
                    Email             = NewEmail,
                    Phone             = NewPhone,
                    OwnerId           = id,
                    PreferredCurrency = currency,
                });
                Message = $"Customer {NewBusinessName} added.";
                IsSuccess = true;
                NewBusinessName = NewEmail = NewPhone = "";
            }
            catch (System.Exception ex) { Message = "Add failed: " + ex.Message; IsSuccess = false; }
            return OnGet();
        }
    }
}
