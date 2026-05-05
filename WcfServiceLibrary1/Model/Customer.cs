using System.Runtime.Serialization;

namespace Model
{
    /// <summary>
    /// CRM customer. Owned by an Owner user (`ownerId`). Bills are issued
    /// to the customer and linked through Projects → Invoices.
    /// </summary>
    [DataContract]
    public class Customer : Base
    {
        [DataMember] public string BusinessName { get; set; }
        [DataMember] public string ContactName { get; set; }
        [DataMember] public string Email { get; set; }
        [DataMember] public string Phone { get; set; }
        [DataMember] public string TaxId { get; set; }
        [DataMember] public string Address { get; set; }
        [DataMember] public int OwnerId { get; set; }
        [DataMember] public string PreferredCurrency { get; set; } = "ILS";
        [DataMember] public string Notes { get; set; }
    }
}
