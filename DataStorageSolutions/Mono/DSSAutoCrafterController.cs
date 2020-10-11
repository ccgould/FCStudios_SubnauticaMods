using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using DataStorageSolutions.Abstract;
using DataStorageSolutions.Buildables;
using DataStorageSolutions.Configuration;
using DataStorageSolutions.Helpers;
using DataStorageSolutions.Interfaces;
using DataStorageSolutions.Model;
using FCSCommon.Extensions;
using FCSCommon.Helpers;
using FCSCommon.Utilities;
using FCSTechFabricator.Components;
using FCSTechFabricator.Enums;
using FCSTechFabricator.Interfaces;
using SMLHelper.V2.Crafting;
using UnityEngine;
using UnityEngine.UI;

namespace DataStorageSolutions.Mono
{
    public enum Status
    {
        None,
        Running,
        NotRunning
    }

    internal class DSSAutoCrafterController : DataStorageSolutionsController, IFCSStorage, IMessageDialogSender
    {
        private Dictionary<Status, string> _status = new Dictionary<Status, string>
        {
            {Status.Running, "Running"},
            {Status.NotRunning, "Not Running"},
            {Status.None, "N/A"},
        };

        private IEnumerable<Material> _materials;
        private float _beltSpeed = 0.1f;
        private bool _moveBelt;
        private bool _fromSave;
        private bool _runStartUpOnEnable;
        private SaveDataEntry _savedData;
        private GameObject _grid;
        private bool _stopOperation;
        private AutoCraftingWizardDialog _autoCraftDialog;
        private FCSMessageBoxDialog _messageBoxDialog;
        private AutoCraftOperationData _pendingAutocraftDelete;
        private bool _dialogOpen;
        private Transform[] _crafterBeltPath;
        private Transform[] _crafter2BeltPath;
        private Text _craftingQueueLabel;
        private Text _statusLBL;
        private Status _prevStatus;
        private FCSOperation _operation;
        private Status _currentStatus;

        internal Status CurrentStatus
        {
            get => _currentStatus;
            set
            {
                _currentStatus = value;
                if (value == Status.NotRunning || value == Status.None && _prevStatus != Status.NotRunning &&
                    _prevStatus != Status.None)
                {
                    StopBelt();
                    SetUnitLabel();
                    _prevStatus = value;
                }
                else if (value == Status.Running && _prevStatus != Status.Running)
                {
                    MoveBelt();
                    SetUnitLabel();
                    _prevStatus = value;
                }

            }
        }

        private void SetUnitLabel()
        {
            if (_statusLBL == null || _status == null || FCSConnectableDevice == null)
            {
                QuickLogger.Debug("Failed to set label", true);
                return;
            }

            _status.TryGetValue(CurrentStatus, out var statusStr);
            _statusLBL.text = $"Status {statusStr} | UnitID {FCSConnectableDevice.UnitID}";
        }

        public override BaseManager Manager { get; set; }
        public Action<int, int> OnContainerUpdate { get; set; }
        public Action<FCSConnectableDevice, TechType> OnContainerAddItem { get; set; }
        public Action<FCSConnectableDevice, TechType> OnContainerRemoveItem { get; set; }
        public FCSConnectableDevice FCSConnectableDevice { get; private set; }
        public int GetContainerFreeSpace { get; }
        public bool IsFull { get; }
        public DSSCrafterComponent TopCrafter { get; set; }
        public DSSCrafterComponent BottomCrafter { get; set; }


        public override void Initialize()
        {
            QuickLogger.Debug("Initializing Auto Crafter");

            if (FCSConnectableDevice == null)
            {
                FCSConnectableDevice = gameObject.AddComponent<FCSConnectableDevice>();
                FCSConnectableDevice.Initialize(this, this, PowerManager, "AC", true);
            }

            if (FindComponents())
            {
                AddToBaseManager();

                if (PowerManager == null)
                {
                    PowerManager = gameObject.AddComponent<DSSAutoCraftPowerManager>();
                }

                if (TopCrafter == null)
                {
                    TopCrafter = gameObject.AddComponent<DSSCrafterComponent>();
                    TopCrafter.Initialize(this,_crafterBeltPath,Manager);
                }

                if (BottomCrafter == null)
                {
                    BottomCrafter = gameObject.AddComponent<DSSCrafterComponent>();
                    BottomCrafter.Initialize(this,_crafter2BeltPath,Manager);
                }

                InvokeRepeating(nameof(UpdateStatus), 1, 1);

                IsInitialized = true;
            }
            else
            {
                QuickLogger.Error($"[DSSAutoCrafter] Failed to find all components");
            }
        }

        private bool FindComponents()
        {
            try
            {
                _crafterBeltPath = new[]
                {
                    gameObject.FindChild("WayPoint_1").transform, gameObject.FindChild("WayPoint_2").transform,
                    gameObject.FindChild("WayPoint_3").transform
                };
                _crafter2BeltPath = new[]
                {
                    gameObject.FindChild("WayPoint_4").transform, gameObject.FindChild("WayPoint_5").transform,
                    gameObject.FindChild("WayPoint_6").transform
                };
                _statusLBL = GameObjectHelpers.FindGameObject(gameObject, "StatusLBL").GetComponent<Text>();

                #region MessageBox

                var messageBox = GameObjectHelpers.FindGameObject(gameObject, "MessageBox");
                _messageBoxDialog = messageBox.AddComponent<FCSMessageBoxDialog>();
                _messageBoxDialog.OnConfirmButtonClick += id => { };

                #endregion

                #region Add Crafting Dialog

                var autoCraftingDialog = GameObjectHelpers.FindGameObject(gameObject, "AutoCraftWizardDialog");
                _autoCraftDialog = autoCraftingDialog.AddComponent<AutoCraftingWizardDialog>();

                #endregion

                #region Crafting Queue Label

                _craftingQueueLabel = GameObjectHelpers.FindGameObject(gameObject, "CraftingQueueLBL")
                    .GetComponent<Text>();

                #endregion

                #region Request Craft Button

                var requestCraftBTN =
                    GameObjectHelpers.FindGameObject(gameObject, "RequestCraft").GetComponent<Button>();
                requestCraftBTN.onClick.AddListener(() =>
                {
                    _autoCraftDialog.ShowDialog(this, _operation, FCSMessageBox.OKCancel);
                });

                #endregion

                var stopStartBTN = GameObjectHelpers.FindGameObject(gameObject, "StopStartBTN").GetComponent<Button>();
                var stopStartText = GameObjectHelpers.FindGameObject(gameObject, "StopStartBTN")
                    .GetComponentInChildren<Text>();
                stopStartBTN.onClick.AddListener(() =>
                {
                    _stopOperation ^= true;
                    stopStartText.text = _stopOperation ? AuxPatchers.Start() : AuxPatchers.Stop();
                });
                _grid = GameObjectHelpers.FindGameObject(gameObject, "Grid");
            }
            catch (Exception e)
            {
                QuickLogger.Error($"[DSSAutoCrafter]{e.Message}");
                return false;
            }

            return true;
        }


        internal bool AddCraftToQueue(AutoCraftOperationData item, out string message)
        {
            message = "Operation Rejected";
            bool result = false;
            if (TopCrafter.CanAcceptCraft())
            {
                result = TopCrafter.AddCraftToQueue(item, out string crafter1);
                message = crafter1;
                QuickLogger.Debug($"TopCrafter: {result} || {crafter1}");
            }

            if (BottomCrafter.CanAcceptCraft() && !result)
            {
                result = BottomCrafter.AddCraftToQueue(item, out string crafter2);
                message = crafter2; 
                QuickLogger.Debug($"BottomCrafter: {result} || {crafter2}");
            }
            return result;
        }


        private void UpdateStatus()
        {
            if (!Manager.BasePowerManager.HasPower() || _stopOperation || !TopCrafter.IsCrafting() && !BottomCrafter.IsCrafting())
            {
                CurrentStatus = Status.NotRunning;
            }
            else
            {
                CurrentStatus = Status.Running;
            }
        }

        #region Unity Methods

        private void Update()
        {
            if (_materials != null && _moveBelt)
            {
                float offset = Time.time * _beltSpeed;
                foreach (Material material in _materials)
                {
                    material.SetTextureOffset("_MainTex", new Vector2(0, offset));
                }
            }
        }

        private void OnEnable()
        {
            if (!_runStartUpOnEnable) return;

            if (!IsInitialized)
            {
                Initialize();
            }

            if (_fromSave)
            {

            }
        }

        private void Start()
        {
            _materials = MaterialHelpers.GetMaterials(gameObject, "DSS_ConveyorBelt");
        }

        private void OnDestroy()
        {
            Manager?.UnRegisterAutoCrafter(this);
        }

        #endregion

        private static void ApplyShader(GameObject inv)
        {
            DSSModelPrefab.ApplyShaders(inv, DSSModelPrefab.Bundle);
            //    SkyApplier skyApplier = inv.EnsureComponent<SkyApplier>();
            //    Shader shader = Shader.Find("MarmosetUBER");
            //    Renderer[] renderers = inv.GetComponentsInChildren<Renderer>();
            //    skyApplier.renderers = renderers;
            //    skyApplier.anchorSky = Skies.Auto;
            //    foreach (Renderer renderer in renderers)
            //    {
            //        renderer.material.shader = shader;
            //    }
        }

        internal void MoveBelt()
        {
            _moveBelt = true;
        }

        internal void StopBelt()
        {
            _moveBelt = false;
        }

        public override bool CanDeconstruct(out string reason)
        {
            //TODO Redo
            reason = String.Empty;
            if (CurrentStatus == Status.NotRunning || CurrentStatus == Status.None)
            {
                return true;
            }

            reason = "Crafter is crafting items";
            return false;
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

                GameObjectHelpers.FindGameObject(gameObject, "Canvas").SetActive(true);
            }
        }

        public override void OnProtoDeserialize(ProtobufSerializer serializer)
        {
            QuickLogger.Debug("In OnProtoDeserialize");
            _fromSave = true;
        }

        public override void OnProtoSerialize(ProtobufSerializer serializer)
        {
            QuickLogger.Debug("In OnProtoSerialize");

            if (!Mod.IsSaving())
            {
                var id = GetPrefabID();
                QuickLogger.Info($"Saving {id}");
                Mod.Save(serializer);
                QuickLogger.Info($"Saved {id}");
            }
        }

        private void ReadySaveData()
        {
            QuickLogger.Debug("In OnProtoDeserialize");
            _savedData = Mod.GetSaveData(GetPrefabID());
        }

        public override void UpdateScreen()
        {
            //Clear the Grid first;
            foreach (Transform child in _grid.transform)
            {
                Destroy(child.gameObject);
            }

            var craftingQueue = new List<AutoCraftOperationData>();
            craftingQueue.AddRange(TopCrafter.GetCraftingQueue());
            craftingQueue.AddRange(BottomCrafter.GetCraftingQueue());

            foreach (AutoCraftOperationData autoCraftOperationData in craftingQueue)
            {
                var item = GameObject.Instantiate(DSSModelPrefab.AutoCrafterItemPrefab);
                item.transform.SetParent(_grid.transform, false);
                item.GetComponentInChildren<Text>().text =
                    $"Item: {Language.main.Get(autoCraftOperationData.AutoCraftRequestItem)} | Amount: {autoCraftOperationData.AutoCraftMaxAmount}";
                item.GetComponentInChildren<Button>().onClick.AddListener((() =>
                {
                    ShowMessageBox(string.Format(AuxPatchers.DeleteConfirmationMessageFormat(),Language.main.Get(autoCraftOperationData.AutoCraftRequestItem)), AutoCraftCallback, FCSMessageBox.YesNo);
                    _pendingAutocraftDelete = autoCraftOperationData;
                    _dialogOpen = true;
                }));
            }
        }

        public override void Save(SaveData save)
        {
            if (!IsInitialized || !IsConstructed) return;

            var id = GetPrefabID();

            if (_savedData == null)
            {
                _savedData = new SaveDataEntry();
            }

            _savedData.ID = id;
            save.Entries.Add(_savedData);
        }

        internal void AddToBaseManager(BaseManager managers = null)
        {
            Manager = managers ?? BaseManager.FindManager(gameObject);

            Manager?.RegisterAutoCrafter(this);
        }

        public bool CanBeStored(int amount, TechType techType)
        {
            return true;
        }

        public bool AddItemToContainer(InventoryItem item)
        {
            QuickLogger.Debug($"Added {item.item.GetTechType().AsString()} to the container", true);
            //TODO Remove
            //Destroy(item.item.gameObject);
            Manager.StorageManager.AddItemToContainer(item);
            return true;
        }

        public bool IsAllowedToAdd(Pickupable pickupable, bool verbose)
        {
            return true;
        }

        public bool IsAllowedToRemoveItems()
        {
            return true;
        }

        public Pickupable RemoveItemFromContainer(TechType techType, int amount)
        {
            return null;
        }

        public Dictionary<TechType, int> GetItemsWithin()
        {
            return new Dictionary<TechType, int>();
        }

        public bool ContainsItem(TechType techType)
        {
            return false;
        }

        public void LinkOperation(FCSOperation operation)
        {
            _operation = operation;
            TopCrafter.LinkOperation(operation);
            BottomCrafter.LinkOperation(operation);
        }

        public bool IsLinked()
        {
            return _operation != null;
        }

        public void ShowMessageBox(string message, Action<FCSDialogResult> callback = null,
            FCSMessageBox mode = FCSMessageBox.OK)
        {
            _messageBoxDialog.ShowMessageBox(message, "", callback, mode);
        }

        private void AutoCraftCallback(FCSDialogResult result)
        {
            if (result == FCSDialogResult.Yes)
            {
                _operation.AutoCraftRequestOperations.Remove(_pendingAutocraftDelete);
            }

            _dialogOpen = false;
        }

        public void RemoveQueue(AutoCraftOperationData operation)
        {
            TopCrafter.RemoveQueue(operation);
            BottomCrafter.RemoveQueue(operation);
        }
    }
}