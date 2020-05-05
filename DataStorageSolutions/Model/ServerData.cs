using System.Collections.Generic;
using DataStorageSolutions.Configuration;

namespace DataStorageSolutions.Model
{
    internal class ServerData
    {
        public List<ObjectData> Server { get; set; }
        public List<Filter> ServerFilters { get; set; }
    }
}
