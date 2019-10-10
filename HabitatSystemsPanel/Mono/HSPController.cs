using UnityEngine;

namespace AE.HabitatSystemsPanel.Mono
{
    internal class HSPController : MonoBehaviour, IConstructable, IProtoEventListener
    {
        private bool _initialized;

        internal bool IsConstructed { get; private set; }
        public bool CanDeconstruct(out string reason)
        {
            reason = string.Empty;
            return true;
        }

        public void OnConstructedChanged(bool constructed)
        {
            IsConstructed = constructed;

            if (constructed)
            {
                if (!_initialized)
                {
                    Initialized();
                }
            }
        }

        private void Initialized()
        {
            _initialized = true;
        }

        public void OnProtoSerialize(ProtobufSerializer serializer)
        {
            throw new System.NotImplementedException();
        }

        public void OnProtoDeserialize(ProtobufSerializer serializer)
        {
            throw new System.NotImplementedException();
        }
    }
}
