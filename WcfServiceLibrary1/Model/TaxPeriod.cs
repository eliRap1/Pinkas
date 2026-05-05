using System.Runtime.Serialization;

namespace Model
{
    /// <summary>
    /// Aggregated monthly snapshot used by the Reports / Tax module.
    /// </summary>
    [DataContract]
    public class TaxPeriod : Base
    {
        [DataMember] public int Year { get; set; }
        [DataMember] public int Month { get; set; }
        [DataMember] public decimal VatCollected { get; set; }
        [DataMember] public decimal VatPaid { get; set; }
        [DataMember] public decimal VatDue { get; set; }
        [DataMember] public decimal IncomeTotal { get; set; }
        [DataMember] public decimal ExpensesTotal { get; set; }
        [DataMember] public decimal ProfitEstimate { get; set; }
        [DataMember] public decimal TaxSetAside { get; set; }
        [DataMember] public string Currency { get; set; } = "ILS";
    }
}
