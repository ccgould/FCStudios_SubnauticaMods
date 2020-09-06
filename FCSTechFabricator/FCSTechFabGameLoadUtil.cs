using FCSTechFabricator.Configuration;
using UnityEngine;

namespace FCSTechFabricator
{
    internal class FCSTechFabGameLoadUtil: MonoBehaviour,IProtoEventListener
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
