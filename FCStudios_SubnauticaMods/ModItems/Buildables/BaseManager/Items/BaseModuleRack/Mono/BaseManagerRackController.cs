using FCS_AlterraHub.Models.Abstract;
using System;

namespace FCS_AlterraHub.ModItems.Buildables.BaseManager.Items.BaseModuleRack.Mono;
internal class BaseManagerRackController : FCSDevice
{
    public override bool CanDeconstruct(out string reason)
    {
        throw new NotImplementedException();
    }

    public override bool IsDeconstructionObstacle()
    {
        throw new NotImplementedException();
    }

    public override void OnConstructedChanged(bool constructed)
    {
        throw new NotImplementedException();
    }

    public override void OnProtoDeserialize(ProtobufSerializer serializer)
    {
        throw new NotImplementedException();
    }

    public override void OnProtoSerialize(ProtobufSerializer serializer)
    {
        throw new NotImplementedException();
    }

    public override void ReadySaveData()
    {
        throw new NotImplementedException();
    }
}
