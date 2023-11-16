using System;
using UnityEngine;

namespace FCS_AlterraHub.Models.Mono;
internal class PortManager : MonoBehaviour
{
    public HabitatManager Manager { get; internal set; }

    internal bool HasAccessPoint()
    {
        return true;
    }
}
