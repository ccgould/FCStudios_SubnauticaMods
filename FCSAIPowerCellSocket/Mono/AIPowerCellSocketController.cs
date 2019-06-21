using FCSAIPowerCellSocket.Model;
using FCSCommon.Utilities;
using Oculus.Newtonsoft.Json;
using SMLHelper.V2.Handlers;
using SMLHelper.V2.Utility;
using System;
using System.IO;

namespace FCSAIPowerCellSocket.Mono
{
    internal partial class AIPowerCellSocketController : IConstructable, IProtoTreeEventListener
    {
        public bool IsConstructed => _buildable != null && _buildable.constructed;
        public AIPowerCellSocketPowerManager PowerManager { get; set; }
        public AIPowerCellSocketDisplay Display { get; private set; }
        public AIPowerCellSocketAnimator AnimationManager { get; set; }

        private PrefabIdentifier _prefabId;

        private readonly string _saveDirectory = Path.Combine(SaveUtils.GetCurrentSaveDataDir(), "AIPowerCellSocket");

        private Constructable _buildable;
        private string SaveFile => Path.Combine(_saveDirectory, _prefabId.Id + ".json");

        private void Awake()
        {
            _prefabId = GetComponentInParent<PrefabIdentifier>();

            if (_buildable == null)
            {
                _buildable = GetComponentInParent<Constructable>();
            }

            PowerManager = gameObject.GetComponent<AIPowerCellSocketPowerManager>();
            PowerManager.Initialize(this);

            AnimationManager = gameObject.GetComponent<AIPowerCellSocketAnimator>();
            if (AnimationManager == null)
            {
                QuickLogger.Error("Animation Manager not found!");
            }

            bool deepPowerCell = TechTypeHandler.TryGetModdedTechType("DeepPowerCell", out TechType deepPowerCellType);

            if (deepPowerCell)
            {
                PowerManager.CompatibleTech.Add(deepPowerCellType);
                QuickLogger.Debug($"Added {deepPowerCellType}  TechType to compatible tech ");
            }

            bool enzymepowercell = TechTypeHandler.TryGetModdedTechType("EnzymePowerCell", out TechType enzymepowercellType);

            if (enzymepowercell)
            {
                PowerManager.CompatibleTech.Add(enzymepowercellType);
                QuickLogger.Debug($"Added {enzymepowercellType}  TechType to compatible tech ");
            }

        }

        internal void UpdateSlots()
        {
            if (AnimationManager != null && PowerManager != null)
            {
                AnimationManager.SetBatteryState(PowerManager.PowercellTracker.Count);
            }
            else
            {
                QuickLogger.Error("Failed to update slots");
            }
        }

        internal void EmptySlot(int slot)
        {
            Display.EmptyBatteryVisual(slot);
        }

        private void OnDestroy()
        {

        }

        private void OpenStorage()
        {
            PowerManager.OpenSlots();
        }

        public bool CanDeconstruct(out string reason)
        {
            reason = String.Empty;

            if (PowerManager.PowercellTracker.Count == 0) return true;
            reason = "Please remove all powercells";
            return false;
        }

        public void OnConstructedChanged(bool constructed)
        {
            if (constructed)
            {
                if (Display == null)
                {
                    Display = gameObject.AddComponent<AIPowerCellSocketDisplay>();
                    Display.Setup(this);
                }
            }
        }

        #region IPhotoTreeEventListener
        public void OnProtoSerializeObjectTree(ProtobufSerializer serializer)
        {
            QuickLogger.Debug($"Saving {_prefabId.Id} Data");

            if (!Directory.Exists(_saveDirectory))
                Directory.CreateDirectory(_saveDirectory);

            var saveData = new SaveData
            {
                PowercellDatas = PowerManager.GetSaveData()
            };

            var output = JsonConvert.SerializeObject(saveData, Formatting.Indented);
            File.WriteAllText(SaveFile, output);

            QuickLogger.Debug($"Saved {_prefabId.Id} Data");
        }

        public void OnProtoDeserializeObjectTree(ProtobufSerializer serializer)
        {
            QuickLogger.Debug("// ****************************** Load Data *********************************** //");

            if (_prefabId != null)
            {
                QuickLogger.Info($"Loading  {_prefabId.Id}");

                if (File.Exists(SaveFile))
                {
                    string savedDataJson = File.ReadAllText(SaveFile).Trim();

                    //LoadData
                    QuickLogger.Debug("Loading Data");
                    var savedData = JsonConvert.DeserializeObject<SaveData>(savedDataJson);
                    QuickLogger.Debug("Loaded Data");

                    PowerManager.LoadPowercellItems(savedData.PowercellDatas);
                    QuickLogger.Debug("Load Items");
                    UpdateSlots();
                }
            }
            else
            {
                QuickLogger.Error("PrefabIdentifier is null");
            }
            QuickLogger.Debug("// ****************************** Loaded Data *********************************** //");
        }
        #endregion
    }
}
