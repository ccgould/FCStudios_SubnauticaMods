using FCS_AlterraHub.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FCS_AlterraHub.Core.Services
{
    internal static class PingTypeService
    {
        private static Dictionary<string, PingType> _pingTypes = new();
        internal static  void AddPingType(string pingName, PingType pingType)
        {
            if (_pingTypes.ContainsKey(pingName)) return;
            _pingTypes.Add(pingName, pingType);
        }
    }
}
