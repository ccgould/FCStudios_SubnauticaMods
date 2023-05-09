using System.Collections.Generic;

namespace FCS_AlterraHub.Core.Services;

internal static class PingTypeService
{
    private static Dictionary<string, PingType> _pingTypes = new();
    internal static  void AddPingType(string pingName, PingType pingType)
    {
        if (_pingTypes.ContainsKey(pingName)) return;
        _pingTypes.Add(pingName, pingType);
    }
}
