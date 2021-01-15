using FCS_AlterraHomeSolutions.Mono.PaintTool;
using FCS_AlterraHub.Extensions;
using FCS_AlterraHub.Helpers;
using FCS_AlterraHub.Interfaces;
using FCS_AlterraHub.Mono;
using FCS_AlterraHub.Registration;
using FCS_StorageSolutions.Configuration;
using FCS_StorageSolutions.Helpers;
using FCS_StorageSolutions.Mods.AlterraStorage.Buildable;
using FCSCommon.Helpers;
using FCSCommon.Utilities;
using UnityEngine;
using UnityEngine.UI;

namespace FCS_StorageSolutions.Mods.DataStorageSolutions.Mono.ItemDisplay
{
    internal class DSSItemDisplayController : FcsDevice, IFCSSave<SaveData>,IFCSDumpContainer
    {
        private bool _runStartUpOnEnable;
        private bool _fromSave;
        private DSSItemDisplayDataEntry _saveData;
        private Text _amountCounter;
        private uGUI_Icon _icon;
        private DumpContainerSimplified _dumpContainer;
        internal TechType currentItem;
        private NetworkDumpStorage _networkDump;
        private InterfaceButton _resetBTN;
        private GameObject _canvas;
        public override bool IsOperational => IsInitialized && IsConstructed;
        public override float GetPowerUsage()
        {
            if (Manager == null || !IsConstructed || Manager.GetBreakerState()) return 0f;
            return 0.01f;
        }

        private void OnBreakerStateChanged(bool value)
        {
            UpdateScreenState();
        }

        private void OnPowerStateChanged(PowerSystem.Status obj)
        {
            UpdateScreenState();
        }

        private void UpdateScreenState()
        {
            if (Manager.GetBreakerState() || Manager.GetPowerState() != PowerSystem.Status.Normal)
            {
                if (_canvas.activeSelf)
                {
                    _canvas.SetActive(false);
                }
            }
            else
            {
                if (!_canvas.activeSelf)
                {
                    _canvas.SetActive(true);
                }
            }
        }

        private void Start()
        {
            FCSAlterraHubService.PublicAPI.RegisterDevice(this, Mod.DSSTabID, Mod.ModName);
            _networkDump.Initialize(Manager);
            Manager.OnPowerStateChanged += OnPowerStateChanged;
            Manager.OnBreakerStateChanged += OnBreakerStateChanged;
            UpdateScreenState();
        }

        public override void OnDestroy()
        {
            if(Manager != null)
            {
                Manager.OnPowerStateChanged -= OnPowerStateChanged;
                Manager.OnBreakerStateChanged -= OnBreakerStateChanged;
            }
            base.OnDestroy();
        }

        private void OnEnable()
        {
            if (!_runStartUpOnEnable) return;

            if (!IsInitialized)
            {
                Initialize();
            }

            if (_saveData == null)
            {
                ReadySaveData();
            }

            if (_fromSave)
            {
                _colorManager.ChangeColor(_saveData.Body.Vector4ToColor());
                _colorManager.ChangeColor(_saveData.SecondaryBody.Vector4ToColor(), ColorTargetMode.Secondary);
                currentItem = _saveData.CurrentItem;
                _fromSave = false;
            }
            
            Refresh();
        }

        private void ReadySaveData()
        {
            QuickLogger.Debug("In OnProtoDeserialize");
            var prefabIdentifier = GetComponentInParent<PrefabIdentifier>() ?? GetComponent<PrefabIdentifier>();
            var id = prefabIdentifier?.Id ?? string.Empty;
            _saveData = Mod.GetDSSItemDisplaySaveData(id);
        }
        
        public override void Initialize()
        {
            _canvas = gameObject.GetComponentInChildren<Canvas>()?.gameObject;


            if (_colorManager == null)
            {
                _colorManager = gameObject.AddComponent<ColorManager>();
                _colorManager.Initialize(gameObject, ModelPrefab.BodyMaterial, ModelPrefab.SecondaryMaterial);
            }

            if (_dumpContainer == null)
            {
                _dumpContainer = gameObject.AddComponent<DumpContainerSimplified>();
                _dumpContainer.Initialize(transform,AuxPatchers.AddItemToItemDisplay(),this,1,1,"ItemDisplayDump");
            }

            if (_networkDump == null)
            {
                _networkDump = gameObject.EnsureComponent<NetworkDumpStorage>();
            }

            var addToNetworkBTN = GameObjectHelpers.FindGameObject(gameObject, "AddBTN").AddComponent<InterfaceButton>();
            addToNetworkBTN.TextLineOne = AuxPatchers.AddItemToNetwork();
            addToNetworkBTN.TextLineTwo = AuxPatchers.AddItemToNetworkDesc();
            addToNetworkBTN.OnButtonClick += (s, o) =>
            {
                _networkDump.OpenStorage();
            };            
            
            _resetBTN = GameObjectHelpers.FindGameObject(gameObject, "ResetBTN").AddComponent<InterfaceButton>();
            _resetBTN.TextLineOne = AuxPatchers.Reset();
            _resetBTN.TextLineTwo = AuxPatchers.ItemDisplayResetDesc();
            _resetBTN.OnButtonClick += (s, o) => { Reset(); };

            var iconBTN = GameObjectHelpers.FindGameObject(gameObject, "Icon").AddComponent<InterfaceButton>();
            iconBTN.GetAdditionalDataFromString = true;
            iconBTN.Tag = this;
            iconBTN.GetAdditionalString += GetAdditionalString;
            iconBTN.OnButtonClick += (s, o) =>
            {
                if (currentItem != TechType.None)
                {
                    if (Manager.HasItem(currentItem) && PlayerInteractionHelper.CanPlayerHold(currentItem))
                    {
                        var item = Manager.TakeItem(currentItem);
                        if (item != null)
                        {
                            PlayerInteractionHelper.GivePlayerItem(item);
                            Refresh();
                        }
                    }
                }
                else
                {
                    _dumpContainer.OpenStorage();
                }
            };

            _amountCounter = GameObjectHelpers.FindGameObject(gameObject, "Text").EnsureComponent<Text>();
            _icon = GameObjectHelpers.FindGameObject(gameObject, "Icon").EnsureComponent<uGUI_Icon>();
            _icon.sprite = SpriteManager.defaultSprite;

            InvokeRepeating(nameof(Refresh), 1, 1);
            InvokeRepeating(nameof(UpdateScreenState), 1, 1);
            IsInitialized = true;

            QuickLogger.Debug($"Initialized");
        }

        private void Reset()
        {
            _icon.sprite = SpriteManager.defaultSprite;
            _amountCounter.text = "0";
            currentItem = TechType.None;
            Refresh();
        }

        private string GetAdditionalString(object arg)
        {
            var techType = ((DSSItemDisplayController) arg).currentItem;
            return techType != TechType.None ? AuxPatchers.TakeFormatted(Language.main.Get(techType)) : AuxPatchers.NoItemToTake();
        }

        public void Refresh()
        {
            if (_amountCounter == null || _icon == null || Manager == null) return;
            _amountCounter.text = Manager.GetItemCount(currentItem).ToString();
            _icon.sprite = SpriteManager.Get(currentItem);
            _resetBTN.gameObject.SetActive(currentItem != TechType.None);
        }
        
        public override void OnProtoSerialize(ProtobufSerializer serializer)
        {
            if (!Mod.IsSaving())
            {
                QuickLogger.Info($"Saving {Mod.DSSItemDisplayFriendlyName}");
                Mod.Save(serializer);
                QuickLogger.Info($"Saved {Mod.DSSItemDisplayFriendlyName}");
            }
        }

        public override void OnProtoDeserialize(ProtobufSerializer serializer)
        {
            _fromSave = true;
        }

        public void Save(SaveData newSaveData, ProtobufSerializer serializer = null)
        {
            QuickLogger.Debug($"In device {UnitID} save");
            var prefabIdentifier = GetComponent<PrefabIdentifier>();
            var id = prefabIdentifier.Id;

            if (_saveData == null)
            {
                _saveData = new DSSItemDisplayDataEntry();
            }
            _saveData.ID = id;
            _saveData.Body = _colorManager.GetColor().ColorToVector4();
            _saveData.SecondaryBody = _colorManager.GetSecondaryColor().ColorToVector4();
            _saveData.CurrentItem = currentItem;
            QuickLogger.Debug($"Adding device {UnitID} to {nameof(newSaveData.DSSItemDisplayDataEntries)} save");
            newSaveData.DSSItemDisplayDataEntries.Add(_saveData);
            QuickLogger.Debug($"Save Count {newSaveData.DSSItemDisplayDataEntries.Count}");
        }

        public override bool ChangeBodyColor(Color color, ColorTargetMode mode)
        {
            return _colorManager.ChangeColor(color, mode);
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
        }

        public bool IsAllowedToAdd(Pickupable pickupable, bool verbose)
        {
            return true;
        }

        public override bool AddItemToContainer(InventoryItem item)
        {
            currentItem = item.item.GetTechType();
            Refresh();
            PlayerInteractionHelper.GivePlayerItem(item);
            return true;
        }
    }
}