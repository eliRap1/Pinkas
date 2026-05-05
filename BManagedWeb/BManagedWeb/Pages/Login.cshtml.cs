using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using BManagedWeb.bsrv;

namespace BManagedWeb.Pages
{
    public class LoginModel : PageModel
    {
        private readonly Service1Client _srv = new Service1Client();

        [BindProperty] public string Username { get; set; }
        [BindProperty] public string Password { get; set; }
        public string ErrorMessage { get; set; }

        public void OnGet() { }

        public IActionResult OnPost()
        {
            if (string.IsNullOrEmpty(Username) || string.IsNullOrEmpty(Password))
            { ErrorMessage = "Please fill all fields"; return Page(); }

            bool ok = _srv.CheckUserPassword(Username, Password);
            if (!ok) { ErrorMessage = "Invalid username or password"; return Page(); }

            int id = _srv.GetUserId(Username);
            var user = _srv.GetUserById(id);

            HttpContext.Session.SetInt32("UserId", user.Id);
            HttpContext.Session.SetString("Username", user.Username);
            HttpContext.Session.SetString("Role", user.Role);
            HttpContext.Session.SetString("Currency", user.PreferredCurrency ?? "ILS");

            return user.Role switch
            {
                "Owner"    => RedirectToPage("/Owner/Home"),
                "Employee" => RedirectToPage("/Employee/Home"),
                _          => RedirectToPage("/Client/Portal"),
            };
        }
    }
}
