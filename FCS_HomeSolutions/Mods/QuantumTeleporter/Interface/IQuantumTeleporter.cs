using FCS_AlterraHub.Mono;
using FCS_HomeSolutions.Mods.QuantumTeleporter.Enumerators;
using UnityEngine;


namespace FCS_HomeSolutions.Mods.QuantumTeleporter.Interface
{
    internal interface IQuantumTeleporter
    {
        BaseManager Manager { get; set; }
        IQTPower PowerManager { get; set; }
        bool IsOperational { get; }
        Transform GetTarget(TeleportItemType senderType = TeleportItemType.Player, string senderID = null);
        bool IsInside();
    }
}