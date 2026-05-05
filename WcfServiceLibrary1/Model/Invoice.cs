using System;
using System.Runtime.Serialization;

namespace Model
{
    /// <summary>
    /// Invoice header. Lines stored separately in InvoiceLines, joined by
    /// invoiceId. Status: Draft → Sent → Paid (or Overdue if dueDate &lt; today
    /// and not paid).
    /// </summary>
    [DataContract]
    public class Invoice : Base
    {
        [DataMember] public string InvoiceNumber { get; set; }
        [DataMember] public int? ProjectId { get; set; }
        [DataMember] public int CustomerId { get; set; }
        [DataMember] public DateTime IssueDate { get; set; } = DateTime.Today;
        [DataMember] public DateTime DueDate { get; set; } = DateTime.Today.AddDays(30);
        [DataMember] public decimal Subtotal { get; set; }
        [DataMember] public double VatRate { get; set; } = 0.17;  // Israel default 17%
        [DataMember] public decimal VatAmount { get; set; }
        [DataMember] public decimal Total { get; set; }
        [DataMember] public string Currency { get; set; } = "ILS";
        [DataMember] public string Status { get; set; } = "Draft";
        [DataMember] public DateTime? PaidDate { get; set; }
        [DataMember] public string Notes { get; set; }
    }
}
