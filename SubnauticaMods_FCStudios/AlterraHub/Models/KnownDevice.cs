using System;

namespace FCS_AlterraHub.Models;

[Serializable]
public struct KnownDevice
{
    public string PrefabID { get; set; }
    public string DeviceTabId { get; set; }
    public int ID { get; set; }

    public override string ToString()
    {
        return $"{DeviceTabId}{ID:D3}";
    }
}
