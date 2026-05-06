using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using BManagedWeb.bsrv;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BManagedWeb.Pages
{
    public class SignUpModel : PageModel
    {
        private readonly Service1Client _srv = new Service1Client();

        // Username: 4-20 chars, letters/digits/underscores/dots only.
        private static readonly Regex UsernameRx = new Regex(@"^[A-Za-z0-9_.]{4,20}$");
        private static readonly Regex EmailRx    = new Regex(@"^[\w\.\-]+@[\w\-]+\.[\w\-\.]+$");
        private static readonly Regex PhoneRx    = new Regex(@"^\+?\d{7,15}$");
        // Password: 8+ chars with at least one letter and one digit.
        private static readonly Regex PasswordRx = new Regex(@"^(?=.*[A-Za-z])(?=.*\d).{8,}$");

        // Two-step picker: empty until the user clicks 'Owner' or 'Employee'.
        [BindProperty] public string SelectedRole { get; set; } = "";

        [BindProperty] public string Username { get; set; }
        [BindProperty] public string Password { get; set; }
        [BindProperty] public string Email { get; set; }
        [BindProperty] public string Phone { get; set; }
        [BindProperty] public string Currency { get; set; } = "ILS";

        // Owner-only
        [BindProperty] public string BusinessType { get; set; } = "Patur";
        [BindProperty] public bool   IsZair { get; set; } = false;

        // Employee-only — which Owner / company the new employee belongs to.
        [BindProperty] public int    OwnerId { get; set; } = 0;

        public string ErrorMessage { get; set; }
        public List<User> AvailableOwners { get; set; } = new();

        private void LoadOwners()
        {
            try { AvailableOwners = (_srv.GetActiveOwners() ?? new User[0]).ToList(); }
            catch { AvailableOwners = new List<User>(); }
        }

        public void OnGet() { LoadOwners(); }

        public IActionResult OnPost()
        {
            LoadOwners();

            // If the user posted without picking a role yet, just bounce back —
            // the JS hides the form, but server-side guard is cheap insurance.
            if (SelectedRole != "Owner" && SelectedRole != "Employee")
            { ErrorMessage = "Pick a role first."; return Page(); }

            if (string.IsNullOrEmpty(Username) || !UsernameRx.IsMatch(Username))
            { ErrorMessage = "Username must be 4–20 letters / digits / _ / ."; return Page(); }
            if (string.IsNullOrEmpty(Password) || !PasswordRx.IsMatch(Password))
            { ErrorMessage = "Password must be 8+ chars and include at least one letter and one digit."; return Page(); }
            if (string.Equals(Password, Username, System.StringComparison.OrdinalIgnoreCase))
            { ErrorMessage = "Password cannot be the same as username."; return Page(); }
            if (!EmailRx.IsMatch(Email ?? "")) { ErrorMessage = "Email looks invalid (e.g. name@example.com)."; return Page(); }
            if (!PhoneRx.IsMatch(Phone ?? "")) { ErrorMessage = "Phone must be 7–15 digits, optionally + country code."; return Page(); }

            if (SelectedRole == "Employee" && OwnerId <= 0)
            { ErrorMessage = "Please pick the company you work for."; return Page(); }

            try
            {
                if (_srv.CheckUserExist(Username))
                { ErrorMessage = "Username already taken."; return Page(); }

                bool ok = _srv.AddUser(Username, Password, Email, Phone, SelectedRole, Currency ?? "ILS");
                if (!ok) { ErrorMessage = "Server rejected the request."; return Page(); }

                int newId = _srv.GetUserId(Username);

                if (SelectedRole == "Owner")
                {
                    // Persist Patur / Murshe + Zair flag. BusinessType validation:
                    // only the two real values are allowed for Owner.
                    var bt = (BusinessType == "Murshe") ? "Murshe" : "Patur";
                    try { _srv.SetBusinessType(newId, bt); } catch { }
                    if (IsZair)
                    { try { _srv.SetIsZair(newId, true); } catch { } }
                }
                else // Employee
                {
                    try { _srv.SetOwnerId(newId, OwnerId); } catch { }
                }

                return RedirectToPage("/Login");
            }
            catch (System.Exception ex)
            { ErrorMessage = "Error: " + ex.Message; return Page(); }
        }
    }
}
