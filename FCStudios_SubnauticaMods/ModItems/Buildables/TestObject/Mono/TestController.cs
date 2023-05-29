using FCS_AlterraHub.Models.Abstract;

namespace FCS_AlterraHub.ModItems.TestObject.Mono;

internal class TestController : FCSDevice
{
    public override void ReadySaveData()
    {

    }

    public override bool IsDeconstructionObstacle()
    {
        return true;
    }

    public override void OnConstructedChanged(bool constructed)
    {
        IsConstructed = constructed;
        if (constructed)
        {
            if (base.isActiveAndEnabled)
            {
                if (!this.IsInitialized)
                {
                    this.Initialize();
                }
                this.IsInitialized = true;
                return;
            }
            _runStartUpOnEnable = true;
        }
    }

    public override void OnProtoDeserialize(ProtobufSerializer serializer)
    {

    }

    public override void OnProtoSerialize(ProtobufSerializer serializer)
    {

    }

    public override bool CanDeconstruct(out string reason)
    {
        reason = string.Empty;
        return true;
    }
}
