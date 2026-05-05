using System;

namespace BusinessLogic
{
    /// <summary>
    /// Produces the next invoice number in the format INV-YYYY-NNNNN.
    /// Receives the current MAX(id) + 1 from the caller (avoids tight
    /// coupling to a specific DB layer).
    /// </summary>
    public static class InvoiceNumberer
    {
        public static string Next(int sequenceId)
            => $"INV-{DateTime.Today:yyyy}-{sequenceId:D5}";
    }
}
