using System;
using System.Runtime.Serialization;

namespace Model
{
    /// <summary>
    /// System user. Roles: "Owner" (full admin), "Employee" (works on
    /// assigned projects + logs expenses), "Client" (view own invoices
    /// + projects).
    /// </summary>
    [DataContract]
    public class User : Base
    {
        [DataMember] public string Username { get; set; }
        [DataMember] public string PasswordHash { get; set; }
        [DataMember] public string Email { get; set; }
        [DataMember] public string Phone { get; set; }
        [DataMember] public string Role { get; set; } = "Client";
        [DataMember] public bool IsActive { get; set; } = true;
        [DataMember] public DateTime CreatedAt { get; set; } = DateTime.Now;
        [DataMember] public string PreferredCurrency { get; set; } = "ILS";

        /// <summary>
        /// Israeli VAT classification (registration with VAT authority):
        ///   "Patur"      = Osek Patur — VAT-exempt, revenue ≤ ~120k₪/yr.
        ///   "Murshe"     = Osek Murshe — collects/deducts 18% VAT.
        ///   "Individual" = private (no VAT filing).
        /// (Osek Zair is NOT a VAT classification — it is a separate income-tax
        ///  status that can be opted into on top of Patur or Murshe; see IsZair.)
        /// </summary>
        [DataMember] public string BusinessType { get; set; } = "Individual";

        /// <summary>
        /// Osek Zair income-tax status (חישוב נורמטיבי) — 2024 reform.
        /// Available to Patur or Murshe with annual revenue ≤ ~122,833₪.
        /// When on, income tax = revenue × 70 % through the progressive
        /// brackets (flat 30 % deemed-expenses, no receipts required).
        /// Does NOT change VAT obligations — those follow BusinessType.
        /// </summary>
        [DataMember] public bool IsZair { get; set; } = false;
    }
}
