using ExStorageDepot.Configuration;
using ExStorageDepot.Enumerators;
using ExStorageDepot.Mono.Managers;
using FCSCommon.Utilities;
using UnityEngine;

namespace ExStorageDepot.Mono
{
    internal class ExStorageDepotController : MonoBehaviour, IProtoEventListener, IConstructable
    {
        internal ExStorageDepotDisplayManager Display { get; private set; }
        private ExStorageDepotSaveDataEntry _saveData;
        private bool _initialized;
        internal ExStorageDepotNameManager NameController { get; private set; }
        internal ExStorageDepotAnimationManager AnimationManager { get; private set; }
        internal ExStorageDepotStorageManager Storage { get; private set; }
        public BulkMultipliers BulkMultiplier { get; set; }

        public void OnProtoSerialize(ProtobufSerializer serializer)
        {
            if (!Mod.IsSaving())
            {
                QuickLogger.Info("Saving Ex Storage Depot");
                Mod.SaveExStorageDepot();
            }
        }

        public void OnProtoDeserialize(ProtobufSerializer serializer)
        {
            QuickLogger.Debug("In OnProtoDeserialize");
            var prefabIdentifier = GetComponent<PrefabIdentifier>();
            var id = prefabIdentifier?.Id ?? string.Empty;
            var data = Mod.GetExStorageDepotSaveData(id);
            NameController.SetCurrentName(data.UnitName);
            Storage.LoadFromSave(data.StorageItems);
        }

        public bool CanDeconstruct(out string reason)
        {
            reason = string.Empty;
            return true;
        }

        public void OnConstructedChanged(bool constructed)
        {
            if (constructed)
            {
                if (!_initialized)
                {
                    Initialize();
                }
            }
        }

        private void Initialize()
        {
            if (NameController == null)
            {
                NameController = new ExStorageDepotNameManager();
                NameController.Initialize(this);
            }

            AnimationManager = gameObject.AddComponent<ExStorageDepotAnimationManager>();
            Storage = gameObject.AddComponent<ExStorageDepotStorageManager>();
            Storage.Initialize(this);

            if (Display == null)
            {
                Display = gameObject.AddComponent<ExStorageDepotDisplayManager>();
                Display.Initialize(this);
            }
            _initialized = true;
        }

        public void Save(ExStorageDepotSaveData saveDataList)
        {
            var prefabIdentifier = GetComponent<PrefabIdentifier>();
            var id = prefabIdentifier.Id;

            if (_saveData == null)
            {
                _saveData = new ExStorageDepotSaveDataEntry();
            }
            _saveData.Id = id;
            _saveData.UnitName = NameController.GetCurrentName();
            _saveData.StorageItems = Storage.TrackedItems;

            saveDataList.Entries.Add(_saveData);
        }
    }
}
