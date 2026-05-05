using System;
using System.Runtime.Serialization;

namespace Model
{
    /// <summary>
    /// Lightweight in-app message displayed by the polling notification
    /// badge in WPF + Web. Same contract pattern as Driver-moodle.
    /// </summary>
    [DataContract]
    public class Notification : Base
    {
        [DataMember] public int UserId { get; set; }
        [DataMember] public string Title { get; set; }
        [DataMember] public string Message { get; set; }
        [DataMember] public string NotificationType { get; set; } = "Info";
        [DataMember] public bool IsRead { get; set; }
        [DataMember] public DateTime CreatedAt { get; set; } = DateTime.Now;
        [DataMember] public DateTime? ReadAt { get; set; }
    }
}
