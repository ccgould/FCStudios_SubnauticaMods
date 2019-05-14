using System;
using UnityEngine;

namespace FCSDeepHarvester.Models.Controllers
{
    public class DeepHarvesterController : MonoBehaviour, IProtoEventListener, IProtoTreeEventListener, IHandTarget
    {
        public void OnProtoSerialize(ProtobufSerializer serializer)
        {
            // throw new NotImplementedException();
        }

        public void OnProtoDeserialize(ProtobufSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public void OnProtoSerializeObjectTree(ProtobufSerializer serializer)
        {
            // throw new NotImplementedException();
        }

        public void OnProtoDeserializeObjectTree(ProtobufSerializer serializer)
        {
            // throw new NotImplementedException();
        }

        public void OnHandHover(GUIHand hand)
        {
            HandReticle.main.SetInteractText("Open Deep Harvester", false, HandReticle.Hand.Left);
        }

        public void OnHandClick(GUIHand hand)
        {
            //throw new NotImplementedException();
        }
    }
}
