using System;
using System.Runtime.Serialization;

namespace Model
{
    /// <summary>
    /// A piece of work for a customer. Status flow:
    /// Active → AwaitingPayment → Done.
    /// </summary>
    [DataContract]
    public class Project : Base
    {
        [DataMember] public int CustomerId { get; set; }
        [DataMember] public string Title { get; set; }
        [DataMember] public string Description { get; set; }
        [DataMember] public string Status { get; set; } = "Active";
        [DataMember] public DateTime? StartDate { get; set; }
        [DataMember] public DateTime? DueDate { get; set; }
        [DataMember] public int? AssignedEmployeeId { get; set; }
        [DataMember] public decimal TotalBudget { get; set; }
        [DataMember] public string Currency { get; set; } = "ILS";
    }
}
