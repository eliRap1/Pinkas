using System.Runtime.Serialization;

namespace Model
{
    /// <summary>
    /// Common base class for every persisted entity. Carries the auto-number
    /// primary key (`id` column in Access). All concrete entities inherit
    /// from this so generic ViewDB code can work with a single base type.
    /// </summary>
    [DataContract]
    public class Base
    {
        [DataMember] public int Id { get; set; }
    }
}
