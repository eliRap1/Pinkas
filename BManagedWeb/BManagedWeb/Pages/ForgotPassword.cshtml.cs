using BManagedWeb.bsrv;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BManagedWeb.Pages
{
    public class ForgotPasswordModel : PageModel
    {
        private readonly Service1Client _srv = new Service1Client();

        [BindProperty] public string Username { get; set; }
        public string Message { get; set; }
        public bool IsSuccess { get; set; }

        public void OnGet() { }

        public IActionResult OnPost()
        {
            if (string.IsNullOrEmpty(Username))
            { Message = "Enter a username."; return Page(); }
            try
            {
                if (!_srv.CheckUserExist(Username))
                { Message = "User not found."; IsSuccess = false; return Page(); }

                int uid = _srv.GetUserId(Username);
                var user = _srv.GetUserById(uid);

                // Notify every active Owner so they can act.
                var all = _srv.GetAllUsers();
                if (all != null)
                {
                    foreach (var owner in all)
                    {
                        if (owner.Role == "Owner" && owner.IsActive)
                        {
                            _srv.SendNotification(new Notification
                            {
                                UserId = owner.Id,
                                Title = "Password reset request",
                                Message = $"User '{user.Username}' ({user.Role}) asked for a password reset. " +
                                          "Open ManageUsers > Reset PW to issue 'reset1234'.",
                                NotificationType = "ResetRequest",
                                IsRead = false,
                                CreatedAt = System.DateTime.Now,
                            });
                        }
                    }
                }
                Message = "Owner notified. They will reset your password to 'reset1234'.";
                IsSuccess = true;
            }
            catch (System.Exception ex) { Message = ex.Message; IsSuccess = false; }
            return Page();
        }
    }
}
