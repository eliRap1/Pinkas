using System;

namespace BusinessLogic
{
    /// <summary>
    /// Israel VAT defaults. 18% standard rate (raised from 17% in Jan 2025),
    /// with helper methods to recompute VAT amount + gross from a subtotal.
    /// </summary>
    public static class VatCalculator
    {
        public const double DefaultRate = 0.18;

        public static decimal VatOn(decimal subtotal, double rate = DefaultRate)
            => Math.Round(subtotal * (decimal)rate, 2);

        public static decimal GrossOf(decimal subtotal, double rate = DefaultRate)
            => subtotal + VatOn(subtotal, rate);

        public static decimal SubtotalFromGross(decimal gross, double rate = DefaultRate)
            => Math.Round(gross / (1m + (decimal)rate), 2);
    }
}
