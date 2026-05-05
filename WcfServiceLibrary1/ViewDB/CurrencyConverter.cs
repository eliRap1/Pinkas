using System;

namespace ViewDB
{
    /// <summary>
    /// Converts a money amount from one currency to another using the
    /// most-recent rate stored in the [ExchangeRates] table whose
    /// effectiveDate &lt;= asOfDate.
    /// </summary>
    public class CurrencyConverter
    {
        private readonly ExchangeRateDB _db = new ExchangeRateDB();

        public decimal Convert(decimal amount, string from, string to, DateTime asOfDate)
        {
            if (amount == 0m) return 0m;
            if (string.IsNullOrEmpty(from) || string.IsNullOrEmpty(to)) return amount;
            if (string.Equals(from, to, StringComparison.OrdinalIgnoreCase)) return amount;

            double rate = _db.GetLatestRate(from, to, asOfDate);
            if (rate <= 0) rate = 1.0;
            return Math.Round(amount * (decimal)rate, 2);
        }
    }
}
