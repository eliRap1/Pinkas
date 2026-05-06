using System.Collections.Generic;
using BManagedWeb.bsrv;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BManagedWeb.Pages
{
    public class NotificationsModel : PageModel
    {
        private readonly Service1Client _srv = new Service1Client();
        public List<Notification> Notifications { get; set; } = new();

        public IActionResult OnGet()
        {
            int id = HttpContext.Session.GetInt32("UserId") ?? 0;
            if (id == 0) return RedirectToPage("/Login");
            try
            {
                var arr = _srv.GetUserNotifications(id);
                if (arr != null) Notifications = new List<Notification>(arr);
            }
            catch { }
            return Page();
        }

        // JSON: GET ?handler=Count -> int (badge polling).
        public IActionResult OnGetCount()
        {
            int id = HttpContext.Session.GetInt32("UserId") ?? 0;
            if (id == 0) return new JsonResult(0);
            try { return new JsonResult(_srv.GetUnreadNotificationCount(id)); }
            catch { return new JsonResult(0); }
        }

        public IActionResult OnPostMarkAll()
        {
            int id = HttpContext.Session.GetInt32("UserId") ?? 0;
            if (id == 0) return RedirectToPage("/Login");
            try { _srv.MarkAllNotificationsAsRead(id); } catch { }
            return RedirectToPage();
        }
    }
}
