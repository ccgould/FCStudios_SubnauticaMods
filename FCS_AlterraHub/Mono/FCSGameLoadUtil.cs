using FCS_AlterraHub.Configuration;
using UnityEngine;

namespace FCS_AlterraHub.Mono
{
    internal class FCSGameLoadUtil: MonoBehaviour,IProtoEventListener
    {
        public void OnProtoSerialize(ProtobufSerializer serializer)
        {

        }

        public void OnProtoDeserialize(ProtobufSerializer serializer)
        {
            Mod.LoadDevicesData();
        }
    }
}
