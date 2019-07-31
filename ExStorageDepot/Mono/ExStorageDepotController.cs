using ExStorageDepot.Mono.Managers;
using UnityEngine;

namespace ExStorageDepot.Mono
{
    internal class ExStorageDepotController : MonoBehaviour, IProtoEventListener, IConstructable
    {
        #region Unity Methods
        private void Start()
        {
            AnimationManager = gameObject.AddComponent<ExStorageDepotAnimationManager>();

        }
        #endregion

        public void OnProtoSerialize(ProtobufSerializer serializer)
        {

        }

        public void OnProtoDeserialize(ProtobufSerializer serializer)
        {

        }

        internal ExStorageDepotAnimationManager AnimationManager { get; set; }
        public bool CanDeconstruct(out string reason)
        {
            reason = string.Empty;
            return true;
        }

        public void OnConstructedChanged(bool constructed)
        {
            if (constructed)
            {
                var display = gameObject.AddComponent<ExStorageDepotDisplayManager>();
                display.Initialize(this);
            }
        }
    }
}
