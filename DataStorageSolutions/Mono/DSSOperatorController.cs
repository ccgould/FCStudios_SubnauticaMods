using System.Collections.Generic;
using DataStorageSolutions.Abstract;
using DataStorageSolutions.Configuration;
using DataStorageSolutions.Model;
using FCSCommon.Controllers;
using FCSCommon.Utilities;

namespace DataStorageSolutions.Mono
{
    internal class DSSOperatorController : DataStorageSolutionsController
    {
        private bool _runStartUpOnEnable;
        private SaveDataEntry _savedData;
        private bool _fromSave;
        public override void Save(SaveData save)
        {
        }

        public override BaseManager Manager { get; set; }
        public DSSOperatorDisplayManager DisplayManager { get; private set; }

        //private void Start()
        //{
        //    InvokeRepeating(nameof(PerformOperation),1f,1f);
        //}

        private void OnEnable()
        {
            if (!_runStartUpOnEnable) return;

            if (!IsInitialized)
            {
                Initialize();
            }

            if (_fromSave)
            {
                DisplayManager.Refresh();
            }

        }  

        private void OnDestroy()
        {
            Manager?.UnRegisterOperator(this);
        }
        
        public override void OnProtoSerialize(ProtobufSerializer serializer)
        {
        }

        public override void OnProtoDeserialize(ProtobufSerializer serializer)
        {
            QuickLogger.Debug("In OnProtoDeserialize");
            _fromSave = true;
        }

        public override void Initialize()
        {
            AddToBaseManager();

            if (AnimationHandler == null)
            {
                AnimationHandler = gameObject.AddComponent<AnimationManager>();
            }

            if (DisplayManager == null)
            {
                DisplayManager = gameObject.AddComponent<DSSOperatorDisplayManager>();
                DisplayManager.Setup(this);
            }
        }

        public AnimationManager AnimationHandler { get; set; }

        internal void AddToBaseManager(BaseManager managers = null)
        {
            Manager = managers ?? BaseManager.FindManager(gameObject);

            Manager?.RegisterOperator(this);
        }

        public override bool CanDeconstruct(out string reason)
        {
            reason = string.Empty;
            return true;
        }

        public override void OnConstructedChanged(bool constructed)
        {
            IsConstructed = constructed;

            if (constructed)
            {
                if (isActiveAndEnabled)
                {
                    Initialize();
                }
                else
                {
                    _runStartUpOnEnable = true;
                }
            }
        }
    }
}
