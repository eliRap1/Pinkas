using Microsoft.AspNetCore.Http;

namespace BManagedWeb.Helpers
{
    /// <summary>
    /// Tiny localisation helper. Reads `Session["Lang"]` and returns the
    /// matching string. Use from any Razor page:
    ///     @using BManagedWeb.Helpers
    ///     @L.T(HttpContext, "Customers", "לקוחות")
    /// </summary>
    public static class L
    {
        public static string T(HttpContext ctx, string en, string he)
            => (ctx?.Session?.GetString("Lang") ?? "en") == "he" ? he : en;

        public static bool IsHe(HttpContext ctx)
            => (ctx?.Session?.GetString("Lang") ?? "en") == "he";
    }
}
