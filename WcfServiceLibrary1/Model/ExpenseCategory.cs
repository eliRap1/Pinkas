using System.Runtime.Serialization;

namespace Model
{
    /// <summary>
    /// Bucket used to classify expenses (Equipment, Fuel, Rent…). Drives the
    /// `isVatDeductible` flag used by VAT-due calculation.
    /// </summary>
    [DataContract]
    public class ExpenseCategory : Base
    {
        [DataMember] public string Name { get; set; }
        [DataMember] public bool IsVatDeductible { get; set; }
    }
}
