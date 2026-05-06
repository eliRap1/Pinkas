using System;
using System.Runtime.Serialization;

namespace Model
{
    /// <summary>
    /// Service / engagement contract between Owner and Customer for a Project.
    /// Generated as PDF for signature. Once signed, an Invoice can be issued
    /// against it (Invoice.ContractId).
    /// </summary>
    [DataContract]
    public class Contract : Base
    {
        [DataMember] public string ContractNumber { get; set; }
        [DataMember] public int ProjectId  { get; set; }
        [DataMember] public int CustomerId { get; set; }
        [DataMember] public string Title       { get; set; }
        [DataMember] public string Body        { get; set; }
        [DataMember] public decimal TotalAmount { get; set; }
        [DataMember] public string Currency    { get; set; } = "ILS";
        [DataMember] public string Status      { get; set; } = "Draft";  // Draft / Sent / Signed / Cancelled
        [DataMember] public DateTime CreatedAt { get; set; } = DateTime.Now;
        [DataMember] public DateTime? SignedDate { get; set; }
        [DataMember] public string PdfPath     { get; set; }
    }
}
