using UnityEngine;

namespace FCS_HomeSolutions.Mods.Elevator.Mono
{
    internal class ElevatorFloorData
    {
        internal FCSElevatorController Controller;
        public string FloorName { get; set; }
        public float Meters { get; set; }
        internal GameObject LevelObject { get; set; }
        public string FloorId { get; set; }
        public int FloorIndex { get; set; }
    }
}