using System;
using FCSCommon.Abstract;
using GasPodCollector.Configuration;

namespace GasPodCollector.Mono
{
    internal class GaspodCollectorController : FCSController
    {
        public override bool IsConstructed { get; }
        public override bool IsInitialized { get; set; }

        #region Unity Methods

        private void OnEnable()
        {

        }

        #endregion

        public override void Initialize()
        {

        }

        public override void OnProtoSerialize(ProtobufSerializer serializer)
        {

        }

        public override void OnProtoDeserialize(ProtobufSerializer serializer)
        {

        }

        public override bool CanDeconstruct(out string reason)
        {
            reason = string.Empty;
            return true;
        }

        public override void OnConstructedChanged(bool constructed)
        {
            if (constructed)
            {

            }
        }

        internal void Save(SaveData saveData)
        {

        }
    }
}
