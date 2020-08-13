using System.Collections.Generic;
using FCSTechFabricator.Objects;

namespace DataStorageSolutions.Model
{
    internal struct ObjectDataTransferData
    {
        public object data { get; set; }

        public List<Filter> Filters { get; set; }
        public bool IsServer { get; set; }
        public Vehicle Vehicle { get; set; }
    }
}
