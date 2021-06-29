using System;
using FCS_AlterraHomeSolutions.Mono.PaintTool;
using FCS_AlterraHub.Mono;
using FCS_AlterraHub.Registration;
using FCS_HomeSolutions.Buildables;
using FCS_HomeSolutions.Configuration;
using UnityEngine;

namespace FCS_HomeSolutions.Mods.TrashReceptacle.Mono
{
    internal class TrashReceptacleController : FcsDevice, IFCSSave<SaveData>, IHandTarget,IConstructable
    {
        private bool _runStartUpOnEnable;
        private DumpContainer _dumpContainer;
        private TrashStorage _storage;

        private void Start()
        {
            FCSAlterraHubService.PublicAPI.RegisterDevice(this, Mod.TrashReceptacleTabID, Mod.ModPackID);
        }

        private void OnEnable()
        {
            if (!_runStartUpOnEnable) return;

            if (!IsInitialized)
            {
                Initialize();
            }

            //TODO Save
            _runStartUpOnEnable = false;
        }

        public override Vector3 GetPosition()
        {
            return transform.position;
        }

        public void OnHandHover(GUIHand hand)
        {
            if(!IsInitialized || Manager == null) return;
            var main = HandReticle.main;
            main.SetIcon(HandReticle.IconType.Info);
            main.SetInteractText( Manager.DeviceBuilt(Mod.RecyclerTabID)
                ? AuxPatchers.ClickToOpenRecycle(Mod.TrashReceptacleFriendly)
                : AuxPatchers.NoRecyclerConnected());
        }

        public void OnHandClick(GUIHand hand)
        {
            if (!IsInitialized || Manager == null) return;

            if (Manager.DeviceBuilt(Mod.RecyclerTabID))
            {
                _dumpContainer.OpenStorage();
            }
        }

        public override void Initialize()
        {
            Manager = BaseManager.FindManager(gameObject);


            if (_dumpContainer == null)
            {
                _dumpContainer = gameObject.AddComponent<DumpContainer>();
            }

            if (_storage == null)
            {
                _storage = gameObject.AddComponent<TrashStorage>();
                _storage.Initialize(this,_dumpContainer);
                _dumpContainer.Initialize(transform, AuxPatchers.TrashReceptacleDumpLabel(), _storage);
            }


            if (_colorManager == null)
            {
                _colorManager = gameObject.AddComponent<ColorManager>();
                _colorManager.Initialize(gameObject,ModelPrefab.BodyMaterial);
            }

            IsInitialized = true;
        }

        public override void OnProtoSerialize(ProtobufSerializer serializer)
        {
            
        }

        public override void OnProtoDeserialize(ProtobufSerializer serializer)
        {

        }

        public override bool CanDeconstruct(out string reason)
        {
            reason = String.Empty;
            return true;
        }

        public override void OnConstructedChanged(bool constructed)
        {
            IsConstructed = true;
            if (isActiveAndEnabled)
            {
                if (!IsInitialized)
                {
                    Initialize();
                }
            }
            else
            {
                _runStartUpOnEnable = true;
            }
        }

        public override bool ChangeBodyColor(Color color, ColorTargetMode mode)
        {
            return _colorManager.ChangeColor(color, mode);
        }

        public void Save(SaveData newSaveData, ProtobufSerializer serializer = null)
        {
            
        }
    }
}
