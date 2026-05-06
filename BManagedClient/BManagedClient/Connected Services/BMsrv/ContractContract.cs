// Hand-written DataContract for Contract — mirrors Model.Contract.
using System;
using System.Runtime.Serialization;

namespace BManagedClient.BMsrv
{
    [DataContract(Name = "Contract", Namespace = "http://schemas.datacontract.org/2004/07/Model")]
    public class Contract : Base
    {
        [DataMember] public string   ContractNumber { get; set; }
        [DataMember] public int      ProjectId      { get; set; }
        [DataMember] public int      CustomerId     { get; set; }
        [DataMember] public string   Title          { get; set; }
        [DataMember] public string   Body           { get; set; }
        [DataMember] public decimal  TotalAmount    { get; set; }
        [DataMember] public string   Currency       { get; set; }
        [DataMember] public string   Status         { get; set; }
        [DataMember] public DateTime CreatedAt      { get; set; }
        [DataMember] public DateTime? SignedDate    { get; set; }
        [DataMember] public string   PdfPath        { get; set; }
    }
}
