using FCS_ProductionSolutions.Configuration;
using FCS_ProductionSolutions.ModItems.Buildables.DeepDrillers.Mono.Base;
using System;

namespace FCS_ProductionSolutions.ModItems.Buildables.DeepDrillers.Mono;
internal class DeepDrillerController : DrillSystem
{
    public bool CanBeSeenByTransceiver { get; internal set; }
    public object Manager { get; internal set; }

    internal override bool UseOnScreenUi => throw new NotImplementedException();

    public override void ReadySaveData()
    {

    }

    public override void Save(SaveData saveDataList, ProtobufSerializer serializer = null)
    {
        
    }

    internal override void LoadSave()
    {
    }
}
