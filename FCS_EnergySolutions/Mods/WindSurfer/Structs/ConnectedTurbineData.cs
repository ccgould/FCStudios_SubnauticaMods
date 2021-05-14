using UnityEngine;

namespace FCS_EnergySolutions.Mods.WindSurfer.Structs
{
    internal struct ConnectedTurbineData
    {
        public int Slot { get; set; }
        public string ParentTurbineUnitID { get; set; }
        public Vector2Int HoloGraphPosition { get; set; }
    }
}