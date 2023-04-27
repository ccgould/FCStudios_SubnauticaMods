using FCS_AlterraHub.Models.Abstract;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FCS_AlterraHub.ModItems.Buildables.DroneDepotPort.Mono
{
    internal class DroneDepotPortController : FCSDevice
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
}
