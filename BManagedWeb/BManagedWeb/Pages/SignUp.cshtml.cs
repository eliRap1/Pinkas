using System.Text.RegularExpressions;
using BManagedWeb.bsrv;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BManagedWeb.Pages
{
    public class SignUpModel : PageModel
    {
        private readonly Service1Client _srv = new Service1Client();

        // Username: 4-20 chars, letters/digits/underscores/dots only — keeps the
        // value safe to reuse in URLs and the SecurityHelper.IsSafeString check.
        private static readonly Regex UsernameRx = new Regex(@"^[A-Za-z0-9_.]{4,20}$");
        private static readonly Regex EmailRx    = new Regex(@"^[\w\.\-]+@[\w\-]+\.[\w\-\.]+$");
        private static readonly Regex PhoneRx    = new Regex(@"^\+?\d{7,15}$");
        // Password: 8+ chars with at least one letter and one digit.
        private static readonly Regex PasswordRx = new Regex(@"^(?=.*[A-Za-z])(?=.*\d).{8,}$");

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
            if (string.IsNullOrEmpty(Username) || !UsernameRx.IsMatch(Username))
            { ErrorMessage = "Username must be 4–20 letters / digits / _ / ."; return Page(); }
            if (string.IsNullOrEmpty(Password) || !PasswordRx.IsMatch(Password))
            { ErrorMessage = "Password must be 8+ chars and include at least one letter and one digit."; return Page(); }
            if (string.Equals(Password, Username, System.StringComparison.OrdinalIgnoreCase))
            { ErrorMessage = "Password cannot be the same as username."; return Page(); }
            if (!EmailRx.IsMatch(Email ?? "")) { ErrorMessage = "Email looks invalid (e.g. name@example.com)."; return Page(); }
            if (!PhoneRx.IsMatch(Phone ?? "")) { ErrorMessage = "Phone must be 7–15 digits, optionally + country code."; return Page(); }
            if (BusinessType == "Patur" || BusinessType == "Murshe")
            {
                // Business owners must self-identify a currency — Patur invoices
                // depend on it for the VAT-zero default. Default fine if blank.
                if (string.IsNullOrEmpty(Currency)) Currency = "ILS";
            }

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
