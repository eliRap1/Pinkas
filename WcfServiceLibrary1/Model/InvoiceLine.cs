using System.Runtime.Serialization;

namespace Model
{
    /// <summary>
    /// Single line item on an invoice (link table; many lines per invoice).
    /// </summary>
    [DataContract]
    public class InvoiceLine : Base
    {
        [DataMember] public int InvoiceId { get; set; }
        [DataMember] public string Description { get; set; }
        [DataMember] public double Quantity { get; set; } = 1.0;
        [DataMember] public decimal UnitPrice { get; set; }
        [DataMember] public decimal LineTotal { get; set; }
        [DataMember] public string Currency { get; set; } = "ILS";
    }
}
