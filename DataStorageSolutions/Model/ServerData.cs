using System.Collections.Generic;
using FCSTechFabricator.Objects;

namespace DataStorageSolutions.Model
{
    internal class ServerData
    {
        public int SlotID { get; set; }
        public string PrefabID { get; set; }
        public HashSet<Filter> ServerFilters { get; set; }
    }
}
