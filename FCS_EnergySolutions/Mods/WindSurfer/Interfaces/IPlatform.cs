using FCS_EnergySolutions.Mods.WindSurfer.Mono;

namespace FCS_EnergySolutions.Mods.WindSurfer.Interfaces
{
    internal interface IPlatform
    {
        PlatformController PlatformController { get;}
        string GetUnitID();
        string GetPrefabID();
    }
}