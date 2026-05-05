using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace Model
{
    /// <summary>
    /// Strongly-typed CollectionDataContract wrapper around a list of
    /// users. Lets the WCF client deserialise the result as an array
    /// without inventing per-call list types.
    /// </summary>
    [CollectionDataContract]
    public class AllUsers : List<User>
    {
        public AllUsers() { }
        public AllUsers(IEnumerable<Base> list) : base(list.Cast<User>().ToList()) { }
    }
}
