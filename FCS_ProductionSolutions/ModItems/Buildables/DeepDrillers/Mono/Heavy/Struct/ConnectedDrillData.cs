using FCS_AlterraHub.Models.Structs;
using Newtonsoft.Json;
using System;
using UnityEngine;

namespace FCS_ProductionSolutions.ModItems.Buildables.DeepDrillers.Mono.Heavy.Struct;

public struct ConnectedDrillData
{
    public int Slot { get; set; }
    public string ParentTurbineUnitID { get; set; }
    public Vector2Int HoloGraphPosition { get; set; }
    public Vec3 Position { get; set; }
    public string UnitID { get; set; }
    [JsonProperty] public float OilTimeLeft { get; set; }
}
