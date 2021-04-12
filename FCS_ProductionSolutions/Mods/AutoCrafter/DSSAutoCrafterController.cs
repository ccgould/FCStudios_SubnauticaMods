using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using FCS_AlterraHomeSolutions.Mono.PaintTool;
using FCS_AlterraHub.Buildables;
using FCS_AlterraHub.Extensions;
using FCS_AlterraHub.Mono;
using FCS_AlterraHub.Registration;
using FCS_ProductionSolutions.Buildable;
using FCS_ProductionSolutions.Configuration;
using FCSCommon.Extensions;
using FCSCommon.Helpers;
using FCSCommon.Utilities;
using rail;
using SMLHelper.V2.Handlers;
using UnityEngine;

namespace FCS_ProductionSolutions.Mods.AutoCrafter
{
    internal class DSSAutoCrafterController : FcsDevice, IFCSSave<SaveData>
    {
        private bool _runStartUpOnEnable;
        private bool _fromSave;
        private DSSAutoCrafterDataEntry _saveData;
        private bool _hasBreakTripped;
        private bool _moveBelt;
        private float _beltSpeed = 0.01f;
        private IEnumerable<Material> _materials;
        private GameObject _canvas;
        private List<TechType> _storedItems = new List<TechType>();
        private float _transferTimer;
        private HashSet<string> _connectedCrafters = new HashSet<string>();
        
        public override bool IsVisible => IsInitialized && IsConstructed;
        public override bool IsOperational => IsInitialized && IsConstructed;
        internal DSSAutoCrafterDisplay DisplayManager;
        internal AutoCrafterMode CurrentCrafterMode = AutoCrafterMode.Automatic;
        internal DSSCraftManager CraftManager;
        private StandByModes _standyByMode = StandByModes.Crafting;


        #region Unit Methods

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

                if (_saveData.IsRunning)
                {
                    DisplayManager.OnButtonClick("StartBTN", null);
                }

                if (_saveData.StoredItems != null)
                {
                    _storedItems = _saveData.StoredItems;
                }

                if (_saveData.ConnectedDevices != null)
                {
                    foreach (string connectedDevice in _saveData.ConnectedDevices)
                    {
                        _connectedCrafters.Add(connectedDevice);
                    }
                }

                CurrentCrafterMode = _saveData.CurrentCrafterMode;
                _standyByMode = _saveData.StandyMode;
                DisplayManager.SetStandByState(CurrentCrafterMode == AutoCrafterMode.StandBy);
                _fromSave = false;
            }
        }

        private void Start()
        {
            FCSAlterraHubService.PublicAPI.RegisterDevice(this, Mod.DSSAutoCrafterTabID, Mod.ModName);
            _materials = MaterialHelpers.GetMaterials(GameObjectHelpers.FindGameObject(gameObject, "ConveyorBelts"), "fcs01_BD");
            if (Manager != null)
            {
                Manager.OnPowerStateChanged += OnPowerStateChanged;
                Manager.OnBreakerStateChanged += OnBreakerStateChanged;
                OnPowerStateChanged(Manager.GetPowerState());
                Manager.NotifyByID(Mod.DSSAutoCrafterTabID, "RefreshAutoCrafterList");
            }
            DisplayManager?.OnLoadComplete?.Invoke();
            InvokeRepeating(nameof(CheckStatus), 1, 1);
            InvokeRepeating(nameof(UpdateOperationTotalCounter), .5f, .5f);
        }

        #endregion

        public override float GetPowerUsage()
        {
            if (Manager == null || !IsConstructed || Manager.GetBreakerState() || CraftManager == null || !CraftManager.IsRunning()) return 0f;
            return 0.01f;
        }

        private void OnBreakerStateChanged(bool value)
        {
            //OnPowerStateChanged(!value ? PowerSystem.Status.Offline : Manager.GetPowerState());
        }

        private void OnPowerStateChanged(PowerSystem.Status obj)
        {
            UpdateScreenState();
            if (obj == PowerSystem.Status.Offline)
            {
                StopBelt();
            }
            else
            {
                MoveBelt();
            }
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
        
        private void CheckStatus()
        {
            if (CurrentCrafterMode == AutoCrafterMode.StandBy)
            {
                UpdateStatus(AutocrafterStatus.StandBy);
            }
            else if (CurrentCrafterMode == AutoCrafterMode.Automatic && CraftManager.IsRunning())
            {
                UpdateStatus(AutocrafterStatus.Working);
            }
            else
            {
                UpdateStatus(AutocrafterStatus.Waiting);
            }
        }

        internal void GetCraftables()
        {
            Mod.Craftables.Clear();

            var fabricator = CraftTree.GetTree(CraftTree.Type.Fabricator);
            GetCraftTreeData(fabricator.nodes);

            var cyclopsFabricator = CraftTree.GetTree(CraftTree.Type.CyclopsFabricator);
            GetCraftTreeData(cyclopsFabricator.nodes);

            var workbench = CraftTree.GetTree(CraftTree.Type.Workbench);
            GetCraftTreeData(workbench.nodes);

            var maproom = CraftTree.GetTree(CraftTree.Type.MapRoom);
            GetCraftTreeData(maproom.nodes);

            var seamothUpgrades = CraftTree.GetTree(CraftTree.Type.SeamothUpgrades);
            GetCraftTreeData(seamothUpgrades.nodes);
        }

        private void Update()
        {
            if (Time.timeScale == 0) return;

            MoveBeltMaterial();
            
            _transferTimer += DayNightCycle.main.deltaTime;

            if (_transferTimer >= 1f)
            {
                for (int i = _storedItems.Count - 1; i >= 0; i--)
                {
                    var inventoryItem = _storedItems[i].ToInventoryItemLegacy();
                    var result = BaseManager.AddItemToNetwork(Manager, inventoryItem, true);
                    if (result)
                    {
                        _storedItems.RemoveAt(i);
                    }
                    else
                    {
                        Destroy(inventoryItem.item.gameObject);
                    }
                }

                _transferTimer = 0f;
            }
        }

        private void MoveBeltMaterial()
        {
            if (_materials != null && _moveBelt)
            {
                float offset = Time.time * _beltSpeed;
                foreach (Material material in _materials)
                {
                    material.SetTextureOffset("_MainTex", new Vector2(-offset, 0));
                }
            }
        }

        public override void OnDestroy()
        {
            if (Manager != null)
            {
                Manager.OnPowerStateChanged -= OnPowerStateChanged;
                Manager.OnBreakerStateChanged -= OnBreakerStateChanged;
                Manager.NotifyByID(Mod.DSSAutoCrafterTabID, "RefreshAutoCrafterList");
            }
            base.OnDestroy();
        }

        private void GetCraftTreeData(CraftNode innerNodes)
        {
            foreach (CraftNode craftNode in innerNodes)
            {
                //QuickLogger.Debug($"Craftable: {craftNode.id} | {craftNode.string0} | {craftNode.string1} | {craftNode.techType0}");

                if (string.IsNullOrWhiteSpace(craftNode.id)) continue;
                if (craftNode.id.Equals("CookedFood") || craftNode.id.Equals("CuredFood")) return;
                if (craftNode.techType0 != TechType.None)
                {
                    if (!CrafterLogicHelper.IsItemUnlocked(craftNode.techType0)) continue;
                    Mod.Craftables.Add(craftNode.techType0);
                }

                if (craftNode.childCount > 0)
                {
                    GetCraftTreeData(craftNode);
                }
            }
        }
        
        private void ReadySaveData()
        {
            QuickLogger.Debug("In OnProtoDeserialize");
            var prefabIdentifier = GetComponentInParent<PrefabIdentifier>() ?? GetComponent<PrefabIdentifier>();
            var id = prefabIdentifier?.Id ?? string.Empty;
            _saveData = Mod.GetDSSAutoCrafterSaveData(id);
        }

        public override void Initialize()
        {
            _canvas = gameObject.GetComponentInChildren<Canvas>()?.gameObject;

            IPCMessage += message =>
            {
                switch (message)
                {
                    case "RefreshAutoCrafterList":
                        DisplayManager.GetPageController(AutoCrafterPages.StandBy)?.Refresh();
                        break;
                }
            };

            if (DisplayManager == null)
            {
                DisplayManager = gameObject.EnsureComponent<DSSAutoCrafterDisplay>();
                DisplayManager.Setup(this);
                DisplayManager.OnCancelBtnClick += OnCancelBtnClick;
            }

            if (CraftManager == null)
            {
                CraftManager = gameObject.EnsureComponent<DSSCraftManager>();
                CraftManager.Initialize(this);
            }

            if (_colorManager == null)
            {
                _colorManager = gameObject.AddComponent<ColorManager>();
                _colorManager.Initialize(gameObject, AlterraHub.BasePrimaryCol, AlterraHub.BaseSecondaryCol);
            }

            MoveBelt();

            MaterialHelpers.ChangeEmissionStrength(AlterraHub.BaseEmissiveDecals, gameObject, 4f);

            MaterialHelpers.ChangeEmissionColor(AlterraHub.BaseEmissiveDecalsController, gameObject,Color.cyan);

            InvokeRepeating(nameof(CheckForAvailableCrafts), 1f, 1f);

            IsInitialized = true;

            QuickLogger.Debug($"Initialized - {GetPrefabID()}");

        }

        private void OnCancelBtnClick()
        {
            Manager.RemoveCraftingOperation(CraftManager.GetCraftingOperation());
            DisplayManager.Clear();
            CraftManager.StopOperation();
            CraftManager.Reset(true);
        }

        private void CheckForAvailableCrafts()
        {
            if (Manager == null || CraftManager.IsRunning() || CurrentCrafterMode != AutoCrafterMode.Automatic) return;

            //Check if already has operation
            var operation = Manager.GetBaseCraftingOperations().FirstOrDefault(x => x.Devices.Contains(UnitID));

            if (operation != null)
            {
                CraftItem(operation);
                return;
            }

            foreach (CraftingOperation baseCraftingOperation in Manager.GetBaseCraftingOperations())
            {
                if (!baseCraftingOperation.CanCraft()) continue;
                CraftItem(baseCraftingOperation);
                break;
            }
        }

        internal void CraftItem(CraftingOperation operation)
        {
            try
            {
                //Check if craftable
                GetCraftables();
                if (!Mod.Craftables.Contains(operation.TechType)) return;

                //Check for additional help
                DistributeLoad(operation,operation.Amount);

                CraftManager.StartOperation(operation);
                DisplayManager.LoadCraft(operation);
                operation.IsBeingCrafted = true;
                operation.Mount(this);
            }
            catch (Exception e)
            {
                QuickLogger.DebugError(e.Message);
                QuickLogger.DebugError(e.StackTrace);
            }
        }

        private void DistributeLoad(CraftingOperation operation, int originalAmount)
        {
            foreach (string connectedCrafter in _connectedCrafters)
            {
                var crafter = IDToAutoCrafter(connectedCrafter);
                if (!crafter.CraftManager.IsRunning() && crafter.GetStandByMode() == StandByModes.Load)
                {
                    crafter.CraftItem(operation);
                }
            }
        }

        internal CraftingOperation GetOperation()
        {
            return CraftManager.GetCraftingOperation();
        }

        public override void OnProtoSerialize(ProtobufSerializer serializer)
        {
            if (!Mod.IsSaving())
            {
                QuickLogger.Info($"Saving {Mod.DSSAutoCrafterFriendlyName}");
                Mod.Save(serializer);
                QuickLogger.Info($"Saved {Mod.DSSAutoCrafterFriendlyName}");
            }
        }

        public override void OnProtoDeserialize(ProtobufSerializer serializer)
        {
            _fromSave = true;

            if (_saveData == null)
            {
                ReadySaveData();
            }

            if (!IsInitialized)
            {
                Initialize();
            }
        }

        public void Save(SaveData newSaveData, ProtobufSerializer serializer = null)
        {
            try
            {
                var prefabIdentifier = GetComponent<PrefabIdentifier>();
                var id = prefabIdentifier.Id;

                if (_saveData == null)
                {
                    _saveData = new DSSAutoCrafterDataEntry();
                }

                _saveData.ID = id;
                _saveData.Body = _colorManager.GetColor().ColorToVector4();
                _saveData.SecondaryBody = _colorManager.GetSecondaryColor().ColorToVector4();
                //_saveData.CurrentProcess = CraftingItems;
                _saveData.ConnectedDevices = _connectedCrafters.ToList();
                _saveData.StoredItems = _storedItems;
                _saveData.CurrentCrafterMode = CurrentCrafterMode;
                _saveData.IsRunning = CraftManager.IsRunning();
                _saveData.StandyMode = _standyByMode;
                newSaveData.DSSAutoCrafterDataEntries.Add(_saveData);
            }
            catch (Exception e)
            {
                QuickLogger.Error($"Failed to save {UnitID}:");
                QuickLogger.Error(e.Message);
                QuickLogger.Error(e.StackTrace);
                QuickLogger.Error(e.InnerException);
            }
        }

        public override bool ChangeBodyColor(Color color, ColorTargetMode mode)
        {
            return _colorManager.ChangeColor(color, mode);
        }

        public override bool CanDeconstruct(out string reason)
        {
            reason = string.Empty;

            if (CraftManager == null) return true;

            if (CraftManager.IsRunning())
            {
                reason = AuxPatchers.AutocrafterItemIsBeingCrafted();
                return false;
            }

            if (CraftManager.ItemsOnBelt())
            {
                reason = AuxPatchers.AutocrafterItemsOnBelt();
                return false;
            }

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

        internal void MoveBelt()
        {
            _moveBelt = true;
        }

        internal void StopBelt()
        {
            _moveBelt = false;
        }

        public bool IsBeltMoving()
        {
            return _moveBelt;
        }

        public void ShowMessage(string message)
        {
            DisplayManager.OnMessageReceived?.Invoke(message);
        }

        public void ClearMissingItems()
        {
            DisplayManager?.ClearMissingItem();
        }

        public void AddMissingItem(string item, int amount)
        {
            DisplayManager?.AddMissingItem(item, amount);
        }

        public void AskForCraftingAssistance(TechType techType)
        {
            //Check if there are any crafters listening and if this
            if(!_connectedCrafters.Any()) return;

            //Check if already being crafted
            foreach (string connectedCrafter in _connectedCrafters)
            {
                var fcsDevice = Manager.FindDeviceById(connectedCrafter);
                if (fcsDevice != null)
                {
                    var crafter = (DSSAutoCrafterController)fcsDevice;
                    if (crafter.GetStandByMode() == StandByModes.Crafting && crafter.CraftManager.IsRunning())
                    {
                        if(crafter.IsCraftingItem(techType)) return;
                    }
                }
            }

            foreach (string connectedCrafter in _connectedCrafters)
            {
                var crafter = IDToAutoCrafter(connectedCrafter);
                if(crafter == null)continue;
                if (!crafter.CraftManager.IsRunning())
                {
                    crafter.CraftItem(new CraftingOperation(techType, 1, false));
                    break;
                }
            }
            
            ////QuickLogger.Debug($" ==== Ask For Crafting Assistance {UnitID} | {Language.main.Get(techType)} ====", true);
            //GetCraftables();
            //if (!Mod.Craftables.Contains(techType) || !Manager.HasIngredientsFor(techType))
            //{
            //    //QuickLogger.Debug($"Base doesnt have ingredients for {Language.main.Get(techType)}", true);
            //    return;
            //}

            //var crafters = Manager.GetDevices(Mod.DSSAutoCrafterTabID);
            ////QuickLogger.Debug($"Crafters Found: {crafters.Count()}", true);
            //foreach (DSSAutoCrafterController crafter in crafters)
            //{
            //    if (crafter.CurrentCrafterMode == AutoCrafterMode.StandBy && !crafter.CraftManager.IsRunning())
            //    {
            //        //QuickLogger.Debug($"Crafting {Language.main.Get(techType)}", true);
            //        crafter.CraftItem(new CraftingOperation(techType, 1, false));
            //    }
            //}

            ////QuickLogger.Debug($" ==== Ask For Crafting Assistance {UnitID} | {Language.main.Get(techType)} ====");
        }

        private DSSAutoCrafterController IDToAutoCrafter(string connectedCrafter)
        {
            var fcsDevice = Manager.FindDeviceById(connectedCrafter);
            if (fcsDevice != null)
            {
                return (DSSAutoCrafterController) fcsDevice;
            }

            return null;
        }

        private bool IsCraftingItem(TechType techType)
        {
            return CraftManager.GetCraftingOperation().TechType == techType;
        }

        internal void SetStandBy()
        {
            CurrentCrafterMode = AutoCrafterMode.StandBy;
        }

        internal void SetManual()
        {
            CurrentCrafterMode = AutoCrafterMode.Manual;
        }

        internal void SetAutomatic()
        {
            CurrentCrafterMode = AutoCrafterMode.Automatic;
        }

        internal void UpdateStatus(AutocrafterStatus status)
        {
            DisplayManager.OnStatusUpdate?.Invoke(status.ToString());
        }

        internal void UpdateOperationTotalCounter()
        {
            if(GetOperation() == null)
                DisplayManager.OnTotalUpdate?.Invoke(0,0);
            else
                DisplayManager.OnTotalUpdate?.Invoke(CraftManager.GetCraftingOperation().AmountCompleted, CraftManager.GetCraftingOperation().Amount);
        }

        public void AddItemToStorage(TechType techType)
        {
            _storedItems.Add(techType);
        }

        public bool CheckIfConnected(string crafterUnitID)
        {
            return _connectedCrafters.Contains(crafterUnitID);
        }
        
        internal void AddConnectedCrafter(string unitId)
        {
            _connectedCrafters.Add(unitId);
        }

        internal void RemoveAutoCrafter(string unitId)
        {
            _connectedCrafters.Remove(unitId);
        }

        public void SetStandByMode(StandByModes crafting)
        {
            _standyByMode = crafting;
        }

        internal StandByModes GetStandByMode()
        {
            return _standyByMode;
        }

        public bool HasConnectedCrafter()
        {
            return _connectedCrafters?.Any() ?? false;
        }

        public IEnumerable GetConnections()
        {
            for (int i = 0; i < _connectedCrafters.Count; i++)
            {
                yield return _connectedCrafters.ElementAt(i);
            }
        }
    }

    internal enum AutocrafterStatus
    {
        Waiting = 0,
        Working = 1,
        StandBy = 2,
        Idle = 3
    }

    internal enum AutoCrafterMode
    {
        Automatic = 0,
        Manual = 1,
        StandBy = 2
    }
}