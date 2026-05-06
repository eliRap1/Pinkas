using System.Text.RegularExpressions;
using BManagedWeb.bsrv;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BManagedWeb.Pages
{
    public class SignUpModel : PageModel
    {
        private readonly Service1Client _srv = new Service1Client();
        private static readonly Regex EmailRx = new Regex(@"^[\w\.\-]+@[\w\-]+\.[\w\-\.]+$");
        private static readonly Regex PhoneRx = new Regex(@"^\+?\d{7,15}$");

        [BindProperty] public string Username { get; set; }
        [BindProperty] public string Password { get; set; }
        [BindProperty] public string Email { get; set; }
        [BindProperty] public string Phone { get; set; }
        [BindProperty] public string Currency { get; set; } = "ILS";
        [BindProperty] public string BusinessType { get; set; } = "Individual";
        [BindProperty] public bool   IsZair { get; set; } = false;
        public string ErrorMessage { get; set; }

        public void OnGet() { }

        public IActionResult OnPost()
        {
            if (string.IsNullOrEmpty(Username) || Username.Length < 4 ||
                string.IsNullOrEmpty(Password) || Password.Length < 4)
            { ErrorMessage = "Username + password must be 4+ chars."; return Page(); }
            if (!EmailRx.IsMatch(Email ?? "")) { ErrorMessage = "Invalid email."; return Page(); }
            if (!PhoneRx.IsMatch(Phone ?? "")) { ErrorMessage = "Invalid phone."; return Page(); }

            try
            {
                if (_srv.CheckUserExist(Username))
                { ErrorMessage = "Username already taken."; return Page(); }

                // Patur / Murshe are VAT registrations, so the user runs a real
                // business → Owner role. "Individual" stays as Client. (Osek Zair
                // is no longer a separate option — it is a flag on top of Patur or
                // Murshe, persisted via SetIsZair.)
                bool isBusiness = BusinessType == "Patur" || BusinessType == "Murshe";
                string role = isBusiness ? "Owner" : "Client";
                bool ok = _srv.AddUser(Username, Password, Email, Phone, role, Currency ?? "ILS");
                if (!ok) { ErrorMessage = "Server rejected the request."; return Page(); }

                try
                {
                    int newId = _srv.GetUserId(Username);
                    _srv.SetBusinessType(newId, BusinessType ?? "Individual");
                    if (isBusiness && IsZair) _srv.SetIsZair(newId, true);
                }
                catch { }

                return RedirectToPage("/Login");
            }
            catch (System.Exception ex)
            { ErrorMessage = "Error: " + ex.Message; return Page(); }
        }
    }
}
