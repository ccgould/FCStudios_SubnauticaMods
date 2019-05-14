using FCS_Alterra_Refrigeration_Solutions.Configuration;
using FCS_Alterra_Refrigeration_Solutions.Enums;
using FCS_Alterra_Refrigeration_Solutions.Logging;
using FCS_Alterra_Refrigeration_Solutions.Models.Base;
using FCS_Alterra_Refrigeration_Solutions.Utilities;
using FCSCommon.Extensions;
using Oculus.Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using SaveData = FCS_Alterra_Refrigeration_Solutions.Models.Base.SaveData;

namespace FCS_Alterra_Refrigeration_Solutions.Models.Components
{
    public class ARSolutionsSeaBreezeController : MonoBehaviour, IConstructable, IProtoEventListener, IProtoTreeEventListener
    {
        #region Pubic Properties
        public PowerState PowerState { get; private set; } = PowerState.None;
        public PowerState PrevPowerState { get; set; } = PowerState.None;
        public StorageContainer Container { get; set; }
        public bool IsBeingDeleted { get; set; }
        public bool PowerAvaliable { get; set; }
        public bool LastPowerAvaliable { get; set; }
        public bool HasBreakerTripped
        {
            get => _hasBreakerTripped;
            set
            {
                _hasBreakerTripped = value;

                Container.enabled = !_hasBreakerTripped;
            }
        }
        public Dictionary<TechType, int> StorageItems { get; set; } = new Dictionary<TechType, int>();
        #endregion

        #region Private Members
        private GameObject _seaBase;
        private bool _isEnabled;
        private SeaBreezeDisplay _seaBreezeDisplay;
        private PowerRelay _powerRelay;
        private ItemsContainer _itemContainer;
        private bool _hasBreakerTripped;
        private bool _containerHasBeenOpened;
        private bool _containerHasBeenClosed;
        private bool _decomposing;
        private bool _oldFilter;
        private Filter _filter;
        private bool _isUsingFilter;
        private bool _previousDecomposingState;
        private bool _allowTick = true;
        private InventoryItem _currentInventoryItem;
        private bool _constructed;
        private Dictionary<string, EatableData> _seaBreezeTracking = new Dictionary<string, EatableData>();

        #endregion

        #region Unity Methods

        private void Awake()
        {
            _itemContainer = new ItemsContainer(1, 1, gameObject.transform, "Filter", null);
            _itemContainer.isAllowedToAdd += ItemContainerIsAllowedToAdd;
            _itemContainer.onAddItem += ItemContainerOnAddItem;
            _itemContainer.onRemoveItem += ItemContainerOnRemoveItem;
        }

        private void Start()
        {
            Log.Info("// == == == == == == == == == == == == == == == //");

            foreach (Transform obj in gameObject.transform)
            {
                Log.Info(obj.name);
            }

            Log.Info("// == == == == == == == == == == == == == == == //");



            Container = GetComponent<StorageContainer>();

            if (Container != null)
            {
                Log.Info("FOUND LOCKER");
                Container.container.isAllowedToAdd += IsAllowedToAdd;
                Container.container.onAddItem += OnAddItem;
                Container.container.onRemoveItem += OnRemoveItem;
                Container.container.isAllowedToRemove += IsAllowedToRemove;
                Container.enabled = true;
            }
            else
            {
                Log.Info("Container is null");
            }
            //TurnDisplayOn();

            _powerRelay = gameObject.GetComponentInParent<PowerRelay>();
            Utils.Assert(_powerRelay != null, "SeaBreeze FCS32 could not find power relay", null);
            this.InvokeRepeating("GetPower", 1f, 1f);

            //InvokeRepeating("GetStatus", 5, 1);

            InvokeRepeating("UpdatePowerState", 0, 1);
        }
        #endregion

        #region Public Methods
        public void OnUse()
        {
            PDA pda = Player.main.GetPDA();
            Inventory.main.SetUsedStorage(_itemContainer);
            pda.Open(PDATab.Inventory, gameObject.transform, null, 4f);
        }

        public bool CanDeconstruct(out string reason)
        {
            if (Container != null) return Container.CanDeconstruct(out reason);

            reason = string.Empty;
            return true;

        }

        public void OnConstructedChanged(bool constructed)
        {
            _constructed = constructed;
            Log.Info($"Constructed - {constructed} || IsEnabled - {_isEnabled}");

            if (IsBeingDeleted) return;

            if (constructed)
            {
                if (_isEnabled == false)
                {
                    _isEnabled = true;
                    StartCoroutine(Startup());
                }
                else
                {
                    TurnDisplayOn();
                }
            }
            else
            {
                if (_isEnabled)
                {
                    TurnDisplayOff();
                }
            }
        }

        #endregion

        #region Private Methods
        private void Update()
        {
            CloseDrive();
            UpdateFilterData();
        }

        private bool IsAllowedToRemove(Pickupable pickupable, bool verbose)
        {
            return !_hasBreakerTripped && PowerAvaliable;
        }

        private void ItemContainerOnRemoveItem(InventoryItem item)
        {

            _currentInventoryItem = null;
            _isUsingFilter = false;
            _filter.TimerTick -= OnTimerTick;
            _filter.TimerEnd -= OnTimerEnd;
            _filter = null;
            _seaBreezeDisplay.FilterLBL.GetComponent<Text>().text = "Filter Change in: 00h:00m:00s";

            FilterHandler(false, item);

        }

        private void ItemContainerOnAddItem(InventoryItem item)
        {
            _currentInventoryItem = item;
            _isUsingFilter = true;

            Log.Info(item.item.name);

            _filter = item.item.GetComponent<Filter>();
            _filter.TimerTick += OnTimerTick;
            _filter.TimerEnd += OnTimerEnd;
            _filter.StartTimer();
            _oldFilter = _filter.IsExpired;
            Log.Info($"Using Filter: {_filter.FilterName} MaxTime: {_filter.MaxTime}");

            FilterHandler(true, item);
        }

        private void FilterHandler(bool active, InventoryItem item)
        {
            if (item.item.name.Equals("ShortTermFilter(Clone)"))
            {
                foreach (Transform obj in transform)
                {
                    if (obj.name.Equals("SeaBreeze(Clone)"))
                    {
                        var gameObj = obj.Find("model").Find("FilterHolderDoor").Find("ShortTerm_Filter");
                        Renderer sRenderers = gameObj.GetComponent<Renderer>();
                        sRenderers.enabled = active;
                        break;
                    }
                }
            }
            else
            {
                foreach (Transform obj in transform)
                {
                    if (obj.name.Equals("SeaBreeze(Clone)"))
                    {
                        var gameObj = obj.Find("model").Find("FilterHolderDoor").Find("LongTerm_Filter");
                        Renderer sRenderers = gameObj.GetComponent<Renderer>();
                        sRenderers.enabled = active;
                        break;
                    }
                }
            }
        }

        private void OnTimerEnd(object sender, EventArgs eventArgs)
        {
            _oldFilter = true;
            _isUsingFilter = false;
            _seaBreezeDisplay.FilterLBL.GetComponent<Text>().text = $"Filter Needs Changing! Food is now spoiling";
            _seaBreezeDisplay.FilterLBL.GetComponent<Text>().color = Color.red;
        }

        private void OnTimerTick(object sender, FilterArgs filterArgs)
        {
            //Log.Info("Tick");

            if (_seaBreezeDisplay.FilterLBL.GetComponent<Text>().color == Color.red)
            {
                _seaBreezeDisplay.FilterLBL.GetComponent<Text>().color = Color.white;
            }
            _seaBreezeDisplay.FilterLBL.GetComponent<Text>().text = $"Filter Change in: {filterArgs.CurrentTime}";
        }

        private bool ItemContainerIsAllowedToAdd(Pickupable pickupable, bool verbose)
        {
            bool flag = false;
            if (pickupable != null)
            {
                TechType techType = pickupable.GetTechType();

                Log.Info(techType.ToString());
                foreach (var contain in LoadItems.AllowedFilters)
                {
                    Log.Info(contain.ToString());
                }
                if (LoadItems.AllowedFilters.ContainsKey(techType))
                    flag = true;
            }
            Log.Info($"Adding Item {flag} || {verbose}");

            if (!flag && verbose)
                ErrorMessage.AddMessage("Alterra Refrigeration Short/Long Filters allowed only");
            return flag;
        }

        private void GetStatus()
        {
            //Log.Info($"Showing Short Term Filter Model {_seaBreezeDisplay.ShortTermFilter.activeSelf}");
            ///Log.Info($"_hasBreakerTripped = {_hasBreakerTripped} || _noPowerAvaliable = {_noPowerAvaliable} || _oldFilter = {_oldFilter} || !_isUsingFilter = {_isUsingFilter} || _previousDecomposingState = {_previousDecomposingState}");
        }

        private void CloseDrive()
        {
            if (Player.main.GetPDA().isOpen && Inventory.main.IsUsingStorage(_itemContainer))
            {
                _containerHasBeenOpened = true;
                //Log.Info("Opened Container PDA");
            }
            else if (_containerHasBeenOpened)
            {
                _containerHasBeenClosed = true;
                //Log.Info("Closed Container PDA");


            }


            if (_containerHasBeenClosed)
            {
                Log.Info("Closing Drive");
                _seaBreezeDisplay.CloseFilterDrive();
                _containerHasBeenOpened = false;
                _containerHasBeenClosed = false;
            }
        }

        private void GetPower()
        {
            //Log.Info($"NoPowerAvaliable has changed to = {_noPowerAvaliable}");
            //InvokeRepeating(UpdatePowerState());

            var powerRelay = this._powerRelay;


            if (powerRelay != null)
            {
                if (!HasBreakerTripped)
                {
                    float num1 = 1f * DayNightCycle.main.dayNightSpeed;

                    if (!GameModeUtils.RequiresPower() || powerRelay != null &&
                        powerRelay.GetPower() >= 0.05 * num1)
                    {
                        if (GameModeUtils.RequiresPower())
                        {
                            float amountConsumed;
                            powerRelay.ConsumeEnergy(0.05f * num1, out amountConsumed);
                        }
                    }
                }
            }
        }

        private void RemoveStartupDecomposition()
        {
            Log.Info("Cooling");
            _decomposing = false;
            if (Container != null)
            {
                foreach (var item in Container.container)
                {
                    var eatable = item.item.GetComponent<Eatable>();

                    eatable.SetDecomposes(false);
                    var id = eatable.GetComponent<PrefabIdentifier>()?.Id;

                    if (id == null)
                    {
                        Log.Error("Eatable ID is null");
                        eatable.foodValue = eatable.GetFoodValue();
                        eatable.waterValue = eatable.GetWaterValue();
                    }
                    else
                    {
                        eatable.foodValue = _seaBreezeTracking[id].FoodValue;
                        eatable.waterValue = _seaBreezeTracking[id].WaterValue;
                    }
                }
            }
        }

        private void AddDecomposition()
        {
            Log.Info("Decomposing");

            _decomposing = true;
            foreach (InventoryItem item in Container.container)
            {

                var eatable = item.item.GetComponent<Eatable>();
                var id = eatable.GetComponent<PrefabIdentifier>().Id;
                //Log.Info($"{eatable.name} Before: F:{eatable.foodValue} W:{eatable.waterValue} R:{eatable.kDecayRate} ID:{eatable.GetInstanceID()} TechType:{eatable.GetComponent<Pickupable>().GetTechType()}");

                eatable.SetDecomposes(true);
                _seaBreezeTracking[id].Decomposes = true;
                var found = false;

                foreach (var eatableEntities in LoadItems.EatableEnties)
                {
                    if (eatableEntities.Name + "(Clone)" == eatable.name)
                    {
                        eatable.kDecayRate = eatableEntities.kDecayRate;
                        found = true;
                        break;
                    }
                }

                if (found == false)
                {
                    Log.Error($"{eatable.name} was not found in the FoodValues.json. Setting a value of 0.015 for the KDecay");
                    ErrorMessage.AddMessage($"Missing Food Entity for \"{eatable.name}\" please notify FCStudios about the message for ARSolutions");
                }
            }
        }

        private void UpdateTrackers()
        {
            Log.Info("Updating Trackers");

            foreach (InventoryItem item in Container.container)
            {
                var eatable = item.item.GetComponent<Eatable>();
                var id = eatable.GetComponent<PrefabIdentifier>().Id;

                _seaBreezeTracking[id].Decomposes = eatable.decomposes;
            }
        }

        private void UpdateScreen()
        {
            _seaBreezeDisplay.ItemModified();
        }

        private void OnRemoveItem(InventoryItem item)
        {

            Log.Info($"Removing {item.item.name} Position:{item.x} {item.y}");

            //TODO Update UI on change
            var _found = false;
            var eatable = item.item.GetComponent<Eatable>();
            var id = eatable.GetComponent<PrefabIdentifier>().Id;

            eatable.SetDecomposes(true);

            foreach (var eatableEntities in LoadItems.EatableEnties)
            {
                if (eatableEntities.Name + "(Clone)" == eatable.name)
                {
                    eatable.kDecayRate = eatableEntities.kDecayRate;
                    _found = true;
                    break;
                }
            }

            if (_found == false)
            {
                Log.Error($"{eatable.name} was not found in the FoodValues.json. Setting a value of 0.015 for the KDecay");
                ErrorMessage.AddMessage($"Missing Food Entity for \"{eatable.name}\" please notify FCStudios about the message for ARSolutions");
            }

            UpdateScreen();

            _seaBreezeTracking.Remove(id);

            Log.Info($"Tracking List Count: {_seaBreezeTracking.Count}");

        }

        private void OnAddItem(InventoryItem item)
        {
            var eatable = item.item.GetComponent<Eatable>();
            var id = eatable.GetComponent<PrefabIdentifier>().Id;

            //Log.Info($"Added {eatable.name} || ID: {id} to fridge");

            //Log.Info($"{eatable.name} Before: F:{eatable.foodValue} W:{eatable.waterValue} R:{eatable.kDecayRate} ID:{eatable.GetComponent<Pickupable>().GetTechType()} TechType:{eatable.GetComponent<Pickupable>().GetTechType()}");

            var eFood = eatable.GetFoodValue();
            var eWater = eatable.GetWaterValue();

            eatable.SetDecomposes(false);
            eatable.foodValue = eFood;
            eatable.waterValue = eWater;

            //Log.Info("Add Eatable Data");

            var eatableData = new EatableData
            {
                ID = id,
                FoodValue = eFood,
                WaterValue = eWater,
                KDecayRate = eatable.kDecayRate,
                Decomposes = eatable.decomposes,
                TechType = eatable.GetComponent<Pickupable>().GetTechType()
            };

            //Log.Info("Add tracker");

            //Log.Info($"Eatable Data {eatableData.ID} || {eatableData.FoodValue} || {eatableData.WaterValue} || {eatableData.KDecayRate} || {eatableData.Decomposes}");

            _seaBreezeTracking.Add(id, eatableData);

            //if (_seaBreezeTracking == null)
            //{
            //    Log.Info("SeaBreeze Tracking is null creating new dictionary");
            //    _seaBreezeTracking = new Dictionary<string, EatableData> {{id, eatableData}};

            //}
            //else
            //{
            //    _seaBreezeTracking.Add(id, eatableData);
            //}

            //Log.Info("Update Screen");

            UpdateScreen();

            Log.Info($"Tracking List Count: {_seaBreezeTracking.Count}");
        }

        private bool IsAllowedToAdd(Pickupable pickupable, bool verbose)
        {
            bool flag = false;
            if (pickupable != null)
            {
                TechType techType = pickupable.GetTechType();
                if (LoadItems.AllowedItems.Contains(techType))
                    flag = true;
            }
            if (!flag && verbose)
                ErrorMessage.AddMessage("Item not allowed");
            return flag;
        }

        private void UpdatePowerState()
        {
            PowerAvaliable = _powerRelay.GetPower() > 0.05;



            //Log.Info($"PowerAvaliable = {PowerAvaliable} || PowerState = {PowerState} || PrevPowerState = {PrevPowerState} || CurrentPower = {powerRelay.GetPower()}");

            if (!PowerAvaliable)
            {
                if (PowerState == PowerState.BlackOut) return;
                Log.Info($"No Power");
                _allowTick = false;
                Container.enabled = false;
                _seaBreezeDisplay.BlackOutMode();
                PowerState = PowerState.BlackOut;
                return;
            }

            if (PowerAvaliable)
            {
                //So we have power available we didn't get returned so lets check the condition of the fridge if it was ran previously
                if (!_hasBreakerTripped)
                {
                    if (PowerState == PowerState.PoweredOn) return;
                    Log.Info($"Breaker has not tripped and power is on");
                    _allowTick = true;
                    _seaBreezeDisplay.PowerOnMode();
                    Container.enabled = true;
                    PowerState = PowerState.PoweredOn;
                    return;
                }

                if (_hasBreakerTripped)
                {
                    if (PowerState == PowerState.PoweredOff) return;
                    Log.Info($"Breaker has Tripped and power is on");
                    _allowTick = false;
                    Container.enabled = false;
                    _seaBreezeDisplay.PowerOffMode();
                    PowerState = PowerState.PoweredOff;
                    return;
                }
            }


            //// Put this coroutine to sleep until the next spawn time.
            //PowerAvaliable = powerRelay.GetPower() > 0.05;

            //Log.Info($"PowerAvaliable = {PowerAvaliable} || LastPowerAvaliable = {LastPowerAvaliable} || PoweredOffMode {_seaBreezeDisplay.PoweredOffMode}");

            ////Lets turn the fridge off if there is no power
            //if (!PowerAvaliable)
            //{
            //    //Log.Info("// *********************** Slot 2 ************************** //");
            //    _allowTick = false;
            //    _seaBreezeDisplay.BlackOutMode();
            //    Container.enabled = false;
            //}

            ////if (PowerAvaliable == LastPowerAvaliable) return;

            //if (PowerAvaliable && !_seaBreezeDisplay.PoweredOffMode)
            //{
            //    //Log.Info("// *********************** Slot 1 ************************** //");

            //    _allowTick = true;
            //    _seaBreezeDisplay.PowerOnMode();
            //    Container.enabled = true;
            //}

            //LastPowerAvaliable = PowerAvaliable;
        }

        private void UpdateFilterData()
        {

            if (_filter != null && _allowTick)
            {
                //Log.Info("UpdateTimer");
                _filter.UpdateTimer();
            }

        }

        private void TurnDisplayOn()
        {
            try
            {
                if (IsBeingDeleted) return;


                Log.Info("SeaBreezeDisplay Add Component");

                _seaBreezeDisplay = gameObject.AddComponent<SeaBreezeDisplay>();

                Log.Info("SeaBreezeDisplay Setup");

                _seaBreezeDisplay.Setup(this);

            }
            catch (Exception e)
            {
                Log.Error($"Error in TurnDisplayOn Method: {e.Message} || {e.InnerException} || {e.Source}");
            }
        }

        private void TurnDisplayOff()
        {
            if (IsBeingDeleted) return;

            if (_seaBreezeDisplay != null)
            {
                _seaBreezeDisplay.TurnDisplayOff();
                Destroy(_seaBreezeDisplay);
                _seaBreezeDisplay = null;
            }
        }
        #endregion

        #region IEnumerators
        private IEnumerator UpdateCoolerState(int waitForSeconds)
        {
            while (true)
            {
                yield return new WaitForSeconds(waitForSeconds);

                //Log.Info("// ==================== UpdateCoolerState =========================== //");
                //Log.Info($"Using Filter: {_isUsingFilter} || Has Breaker Tripped {_hasBreakerTripped} || Power Avaliable {PowerAvaliable}");
                //Log.Info($"Old Filter {_oldFilter} || Previous Decomposing State {_previousDecomposingState}");

                if (_isUsingFilter && !_hasBreakerTripped && PowerAvaliable && !_oldFilter)
                {
                    _previousDecomposingState = false;
                    RemoveStartupDecomposition();
                }

                if (_hasBreakerTripped || !PowerAvaliable || _oldFilter || !_isUsingFilter)
                {

                    _previousDecomposingState = true;
                    AddDecomposition();
                }

                //Log.Info("// ==================== UpdateCoolerState =========================== //");

            }
        }

        private IEnumerator Startup()
        {
            if (IsBeingDeleted) yield break;
            yield return new WaitForEndOfFrame();
            if (IsBeingDeleted) yield break;

            _seaBase = gameObject?.transform?.parent?.gameObject;
            if (_seaBase == null)
            {
                ErrorMessage.AddMessage($"[{Information.ModFriendly}] ERROR: Can not work out what base it was placed inside.");
                Log.Error("ERROR: Can not work out what base it was placed inside.");
                yield break;
            }

            TurnDisplayOn();
        }
        #endregion

        #region IProtoEventListener
        public void OnProtoSerialize(ProtobufSerializer serializer)
        {
            var id = GetComponentInParent<PrefabIdentifier>();

            string saveFolder = FilesHelper.GetSaveFolderPath();

            if (!Directory.Exists(saveFolder))
                Directory.CreateDirectory(saveFolder);

            UpdateTrackers();

            var saveData = new SaveData
            {
                AllowTick = _allowTick,
                ContainerHasBeenClosed = _containerHasBeenClosed,
                ContainerHasBeenOpened = _containerHasBeenOpened,
                Decomposing = _decomposing,
                HasBreakerTripped = _hasBreakerTripped,
                IsEnabled = _isEnabled,
                IsUsingFilter = _isUsingFilter,
                PowerAvaliable = PowerAvaliable,
                PowerState = PowerState,
                SeaBreezeTracking = _seaBreezeTracking
            };

            if (_filter != null)
            {
                saveData.MaxTime = _filter.MaxTime;
            }

            if (_currentInventoryItem != null)
            {
                saveData.ItemID = _currentInventoryItem.item.GetTechType();
            }


            Log.Info("Output");
            string output = JsonConvert.SerializeObject(saveData, Formatting.Indented);
            File.WriteAllText(Path.Combine(saveFolder, "ARSolutions_" + id.Id + ".json"), output);
        }

        public void OnProtoDeserialize(ProtobufSerializer serializer)
        {
            Log.Info("// ****************************** Load Data *********************************** //");
            var id = GetComponentInParent<PrefabIdentifier>();
            if (id != null)
            {
                Log.Info($"Loading ARSolutions {id.Id}");

                string filePath = Path.Combine(FilesHelper.GetSaveFolderPath(), "ARSolutions_" + id.Id + ".json");
                if (File.Exists(filePath))
                {
                    string savedDataJson = File.ReadAllText(filePath).Trim();

                    //LoadData
                    var savedData = JsonConvert.DeserializeObject<SaveData>(savedDataJson);

                    _allowTick = savedData.AllowTick;
                    _containerHasBeenClosed = savedData.ContainerHasBeenClosed;
                    _containerHasBeenOpened = savedData.ContainerHasBeenOpened;
                    _decomposing = savedData.Decomposing;
                    _hasBreakerTripped = savedData.HasBreakerTripped;
                    _isEnabled = savedData.IsEnabled;

                    Log.Info($"In De serialized {_isEnabled}");
                    _isUsingFilter = savedData.IsUsingFilter;
                    PowerAvaliable = savedData.PowerAvaliable;
                    PowerState = savedData.PowerState;

                    if (savedData.SeaBreezeTracking == null)
                    {
                        Log.Info("SeaBreezeTracking save is null");
                    }

                    _seaBreezeTracking = savedData.SeaBreezeTracking;


                    if (_constructed)
                    {
                        StartCoroutine(UpdateCoolerState(1));
                    }

                    if (!LoadItems.AllowedFilters.ContainsKey(savedData.ItemID))
                        return; // error condition


                    var obj = LoadItems.AllowedFilters[savedData.ItemID].GetGameObject();

                    // Set the TechType value on the TechTag
                    obj.GetOrAddComponent<TechTag>().type = savedData.ItemID;


                    // Set the ClassId
                    obj.GetComponent<PrefabIdentifier>().ClassId = LoadItems.AllowedFilters[savedData.ItemID].ClassID;

                    var filter = obj.GetComponent<Filter>();
                    filter.MaxTime = savedData.MaxTime;

                    Pickupable pickupable = obj.GetComponent<Pickupable>().Pickup(false);



                    _itemContainer.AddItem(pickupable);
                }
            }
            else
            {
                Log.Error("PrefabIdentifier is null");
            }
            Log.Info("// ****************************** Loaded Data *********************************** //");
        }
        #endregion

        #region IProtoTreeEventListener
        public void OnProtoSerializeObjectTree(ProtobufSerializer serializer)
        {

        }

        public void OnProtoDeserializeObjectTree(ProtobufSerializer serializer)
        {
            Log.Info("OnProtoDeserializeObjectTree");

            //var id = GetComponentInParent<PrefabIdentifier>();
            //if (id != null)
            //{
            //    Log.Info($"Loading ARSolutions {id.Id}");

            //    string filePath = Path.Combine(FilesHelper.GetSaveFolderPath(), "ARSolutions_" + id.Id + ".json");
            //    if (File.Exists(filePath))
            //    {
            //        string savedDataJson = File.ReadAllText(filePath).Trim();

            //        //LoadData
            //        var savedData = JsonConvert.DeserializeObject<SaveData>(savedDataJson);

            //        _allowTick = savedData.AllowTick;
            //        _containerHasBeenClosed = savedData.ContainerHasBeenClosed;
            //        _containerHasBeenOpened = savedData.ContainerHasBeenOpened;
            //        _decomposing = savedData.Decomposing;
            //        _hasBreakerTripped = savedData.HasBreakerTripped;
            //        _isEnabled = savedData.IsEnabled;
            //        _isUsingFilter = savedData.IsUsingFilter;
            //        _noPowerAvaliable = savedData.NoPowerAvaliable;
            //        _noPowerAvaliablePrev = savedData.NoPowerAvaliablePrev;

            //        var gm = GameObject.Instantiate(CraftData.GetPrefabForTechType(savedData.ItemID));
            //        var pickupable = gm.GetComponent<Pickupable>().Pickup(false);

            //        _itemContainer.AddItem(pickupable);

            //        Log.Info("// ****************************** Load Data *********************************** //");
            //        Log.Info(_itemContainer.count.ToString());
            //        Log.Info(_itemContainer.GetItems(LoadItems.LongTermFilterTechType).Count.ToString());
            //        Log.Info(_itemContainer.GetItems(LoadItems.ShortTermFilterTechType).Count.ToString());
            //        Log.Info("// ****************************** Load Data *********************************** //");

            //        if (_filter != null)
            //        {
            //            _filter.MaxTime = savedData.MaxTime;
            //        }
            //    }


            //}
            //else
            //{
            //    Log.Error("PrefabIdentifier is null");
            //}
        }
        #endregion
    }
}
