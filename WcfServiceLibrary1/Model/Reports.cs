using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Model
{
    /// <summary>
    /// VAT summary returned by GetVatSummary. Server normalises every row
    /// to the requested DisplayCurrency before aggregating.
    /// </summary>
    [DataContract]
    public class VatSummary
    {
        [DataMember] public int Year { get; set; }
        [DataMember] public int Month { get; set; }
        [DataMember] public decimal VatCollected { get; set; }
        [DataMember] public decimal VatPaid { get; set; }
        [DataMember] public decimal VatDue { get; set; }
        [DataMember] public decimal IncomeTotal { get; set; }
        [DataMember] public decimal ExpensesTotal { get; set; }
        [DataMember] public string DisplayCurrency { get; set; } = "ILS";
    }

    /// <summary>P&amp;L for a date range.</summary>
    [DataContract]
    public class ProfitLoss
    {
        [DataMember] public decimal Income { get; set; }
        [DataMember] public decimal Expenses { get; set; }
        [DataMember] public decimal Profit { get; set; }
        [DataMember] public decimal TaxSetAsideEstimate { get; set; }
        [DataMember] public string DisplayCurrency { get; set; } = "ILS";
    }

    /// <summary>Single row in the expense breakdown report (per-category).</summary>
    [DataContract]
    public class ExpenseBreakdownRow
    {
        [DataMember] public string CategoryName { get; set; }
        [DataMember] public decimal Total { get; set; }
        [DataMember] public bool IsVatDeductible { get; set; }
        [DataMember] public string DisplayCurrency { get; set; } = "ILS";
    }

    /// <summary>Top-customer-by-revenue row.</summary>
    [DataContract]
    public class CustomerRevenueRow
    {
        [DataMember] public int CustomerId { get; set; }
        [DataMember] public string BusinessName { get; set; }
        [DataMember] public decimal TotalInvoiced { get; set; }
        [DataMember] public decimal TotalPaid { get; set; }
        [DataMember] public string DisplayCurrency { get; set; } = "ILS";
    }

    /// <summary>Employee-driven revenue row (INNER JOIN Invoices+Projects+Users).</summary>
    [DataContract]
    public class EmployeeRevenueRow
    {
        [DataMember] public int EmployeeId { get; set; }
        [DataMember] public string EmployeeName { get; set; }
        [DataMember] public int ProjectsCount { get; set; }
        [DataMember] public decimal RevenueGenerated { get; set; }
        [DataMember] public string DisplayCurrency { get; set; } = "ILS";
    }
}
