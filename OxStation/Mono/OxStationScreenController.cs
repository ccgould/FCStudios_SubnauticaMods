using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FCSCommon.Utilities;
using FCSTechFabricator.Abstract;
using FCSTechFabricator.Components;
using MAC.OxStation.Config;
using MAC.OxStation.Managers;
using MAC.OxStation.Patches;
using Unity.Collections;

namespace MAC.OxStation.Mono
{
    internal class OxStationScreenController : FCSController
    {
        internal AnimationManager AnimationManager { get; private set; }
        internal ScreenDisplayManager DisplayManager { get; private set; }
        internal SubRoot SubRoot { get; private set; }
        internal BaseManager Manager { get; private set; }
        internal Dictionary<string,OxStationController> TrackedDevices = new Dictionary<string, OxStationController>();
        
        private bool _runStartUpOnEnable;
        private bool _fromSave;
        private object _savedData;

        private void OnEnable()
        {
            if (_runStartUpOnEnable)
            {
                if (!IsInitialized)
                {
                    Initialize();
                }

                if (DisplayManager != null)
                {
                    DisplayManager.Setup(this);
                    _runStartUpOnEnable = false;
                }

                if (_fromSave)
                {
                    if (_savedData == null)
                    {
                        ReadySaveData();
                    }
                    
                    QuickLogger.Info($"Loaded {Mod.FriendlyName}");
                }

                _runStartUpOnEnable = false;
            }
        }

        private void ReadySaveData()
        {
            GetPrefabID();
            _savedData = Mod.GetSaveData(_prefabId);
        }

        public override void Initialize()
        {
            if (!IsInitialized)
            {
                Mod.OnOxstationBuilt += AlertedNewDevicePlaced;
                Mod.OnOxstationDestroyed += AlertedDeviceDestroyed;
                GetOxStations();

                if (AnimationManager == null)
                {
                    AnimationManager = gameObject.AddComponent<AnimationManager>();
                }

                if (DisplayManager == null)
                {
                    DisplayManager = gameObject.AddComponent<ScreenDisplayManager>();
                    DisplayManager.Setup(this);
                }
            }
        }

        private void AlertedDeviceDestroyed(OxStationController obj)
        {
            var prefabId = obj?.GetPrefabIDString();
            if (!string.IsNullOrEmpty(prefabId))
            {
                if (TrackedDevices.ContainsKey(prefabId))
                {
                    UnTrackDevice(obj, prefabId);
                    DisplayManager?.UpdateDisplay();
                }
            }

        }
        
        private void OnDeviceRepaired()
        {
            var amount = TrackedDevices.Count(x => x.Value.HealthManager.IsDamagedFlag());

            if (amount > 0)
            {
                DisplayManager.UpdateDamageAmount(amount);
                return;
            }
            DisplayManager?.ResetDisplay();
        }

        private void OnDeviceDamaged()
        {
            var amount = TrackedDevices.Count(x => x.Value.HealthManager.IsDamagedFlag());
            DisplayManager?.GoToAlertPage(amount);
        }

        private void AlertedNewDevicePlaced(OxStationController obj)
        {
            if (!FindManager()) return;

            var prefabId = obj?.GetPrefabIDString();
            if (!string.IsNullOrEmpty(prefabId))
            {
                if (obj.Manager == Manager && !TrackedDevices.ContainsKey(prefabId))
                {
                    TrackDevice(obj, prefabId);
                    DisplayManager?.UpdateDisplay();
                }
            }
        }

        private bool FindManager()
        {
            SubRoot = GetComponent<SubRoot>() ?? GetComponentInParent<SubRoot>();

            if (SubRoot == null) return false;

            if (Manager == null)
            {
                Manager = BaseManager.FindManager(SubRoot);
            }

            return Manager != null;
        }

        public override void NotifyMe<T>(T obj)
        {
            var controller = obj as OxStationController;

            
        }

        public override void OnProtoSerialize(ProtobufSerializer serializer)
        {
            if (!Mod.IsSaving())
            {
                QuickLogger.Info($"Saving {Mod.FriendlyName}");
                Mod.Save();
                QuickLogger.Info($"Saved {Mod.FriendlyName}");
            }
        }

        public override void OnProtoDeserialize(ProtobufSerializer serializer)
        {
            if (_savedData == null)
            {
                ReadySaveData();
            }

            _fromSave = true;
        }

        public override bool CanDeconstruct(out string reason)
        {
            reason = string.Empty;
            return true;
        }
        
        public override void OnConstructedChanged(bool constructed)
        {
            IsConstructed = constructed;

            if (!constructed) return;

            if (isActiveAndEnabled)
            {
                if (!IsInitialized)
                {
                    Initialize();
                }

                if (DisplayManager != null)
                {
                    DisplayManager.Setup(this);
                    _runStartUpOnEnable = false;
                }
            }
            else
            {
                _runStartUpOnEnable = true;
            }
        }

        private void GetOxStations()
        {
            FindManager();

            //Check if there is a base connected
            if (Manager != null)
            {
                //Clear the list
                TrackedDevices.Clear();

                var connectableDevices = Manager.BaseUnits;

                foreach (var device in connectableDevices)
                {
                    var prefId = device.GetPrefabIDString();
                    if (!string.IsNullOrEmpty(prefId))
                    {
                        TrackDevice(device, prefId);
                    }
                }
                DisplayManager?.UpdateDisplay();
            }
        }

        private void TrackDevice(OxStationController device, string prefabId)
        {
            device.HealthManager.OnDamaged += OnDeviceDamaged;
            device.HealthManager.OnRepaired += OnDeviceRepaired;
            TrackedDevices.Add(prefabId, device);
        }

        private void UnTrackDevice(OxStationController device, string prefabId)
        {
            QuickLogger.Debug($"Removing device: {prefabId} || Device: {device}");
            TrackedDevices.Remove(prefabId);
            device.HealthManager.OnDamaged -= OnDeviceDamaged;
            device.HealthManager.OnRepaired -= OnDeviceRepaired;

        }
    }
}
