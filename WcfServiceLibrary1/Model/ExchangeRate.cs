using System;
using System.Runtime.Serialization;

namespace Model
{
    /// <summary>
    /// Manual FX rate row. CurrencyConverter picks the latest row whose
    /// EffectiveDate &lt;= asOfDate. Seeded with ILS&lt;-&gt;USD pair.
    /// </summary>
    [DataContract]
    public class ExchangeRate : Base
    {
        [DataMember] public string FromCurrency { get; set; }
        [DataMember] public string ToCurrency { get; set; }
        [DataMember] public double Rate { get; set; }
        [DataMember] public DateTime EffectiveDate { get; set; } = DateTime.Today;
    }
}
