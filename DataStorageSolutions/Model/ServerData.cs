using System.Collections.Generic;
using FCSTechFabricator.Objects;

namespace DataStorageSolutions.Model
{
    internal class ServerData
    {
        public int SlotID { get; set; }
        public HashSet<ObjectData> Server { get; set; }
        public List<Filter> ServerFilters { get; set; }
    }
}
