using FCS_AlterraHub.Models.Abstract;
using System;

namespace FCS_AlterraHub.ModItems.Buildables.BaseManager.Items.BaseTransmitter.Mono;
internal class BaseTransmitterController : FCSDevice
{
    public override bool CanDeconstruct(out string reason)
    {
        reason = string.Empty;
        return true;
    }

    public override bool IsDeconstructionObstacle()
    {
        return true;
    }

    public override void OnConstructedChanged(bool constructed)
    {
        IsConstructed = constructed;   
    }

    public override void OnProtoDeserialize(ProtobufSerializer serializer)
    {

    }

    public override void OnProtoSerialize(ProtobufSerializer serializer)
    {

    }

    public override void ReadySaveData()
    {

    }
}
