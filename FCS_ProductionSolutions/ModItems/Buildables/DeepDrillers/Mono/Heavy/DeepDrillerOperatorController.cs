using FCS_AlterraHub.API;
using FCS_AlterraHub.Core.Helpers;
using FCS_AlterraHub.Core.Interfaces;
using FCS_AlterraHub.Models.Abstract;
using FCS_AlterraHub.Models.Grid;
using FCS_AlterraHub.Models.Interfaces;
using FCS_AlterraHub.Models.Mono;
using FCS_ProductionSolutions.Configuration;
using FCS_ProductionSolutions.ModItems.Buildables.DeepDrillers.Models;
using FCS_ProductionSolutions.ModItems.Buildables.DeepDrillers.Mono.Base.Interface;
using FCS_ProductionSolutions.ModItems.Buildables.DeepDrillers.Mono.Heavy.Struct;
using FCS_ProductionSolutions.ModItems.Buildables.DeepDrillers.Mono.Heavy.UGUI;
using FCS_ProductionSolutions.ModItems.Buildables.DeepDrillers.Spawnables;
using FCS_ProductionSolutions.ModItems.Buildables.IonCubeGenerator.Mono.StateMachine;
using FCSCommon.Utilities;
using Mono.Data.Sqlite;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using Unity.Baselib.LowLevel;
using UnityEngine;
using UWE;
using static FCS_ProductionSolutions.Configuration.SaveData;
using static VehicleUpgradeConsoleInput;

namespace FCS_ProductionSolutions.ModItems.Buildables.DeepDrillers.Mono.Heavy;

internal class DeepDrillerOperatorController : FCSDevice, IFCSSave,IDrillSystem
{
    public GameObject DrillsGroupLocation;
    private readonly SortedDictionary<string, DeepDrillerHeavyDutyController> _connectedDrills = new();

    [SerializeField] private GameObject holoGramPrefab;
    internal bool HasRootHolo;
    private Graph<uGUI_DDHoloGraphControl> _graph;
    private List<Graph<uGUI_DDHoloGraphControl>.Edge> _neighbours;
    private const int BUILDING_COMPACITY = 15;
    internal Dictionary<TechType, int> _trackedItems = new();
    internal Action<int> OnStorageCountUpdated;
    private FCSStorage _fcsStorage;
    [SerializeField] private HoverInteraction hoverInteraction;
    [SerializeField] private FCSStorage storage;
    [SerializeField] private DDPlatformController platformController;
    string db_name = "URI=file:data.db";
    [HideInInspector] public Grid2<uGUI_DDHoloGraphControl> Grid { get; private set; }
    public GameObject GameObject => gameObject;

    public override void Start()
    {
        base.Start();
        InitialiseGrid();
        hoverInteraction.onSettingsKeyPressed += HoverInteraction_onSettingsKeyPressed;        
    }

    internal Graph<uGUI_DDHoloGraphControl> GetGraph()
    {
        RefreshMST();
        return _graph;
    }

    public override void OnEnable()
    {
        QuickLogger.Debug($"Start: {GetPrefabID()}");
        base.Start();

        if (_runStartUpOnEnable)
        {
            if (!IsInitialized)
            {
                Initialize();
            }

            if (IsFromSave)
            {
                if (_savedData == null)
                {
                    ReadySaveData();
                }

                QuickLogger.Debug($"Is Save Data Present: {_savedData is not null}");

                if (_savedData is not null)
                {
                    QuickLogger.Debug($"Setting Data");

                    var savedData = _savedData as DeepDrillerOperatorSaveDataEntry;
                    //CoroutineHost.StartCoroutine(LoadDrills());                
                    _colorManager?.LoadTemplate(((ISaveDataEntry)_savedData).ColorTemplate);

                }
            }
            _runStartUpOnEnable = false;
        }
    }

    public override void Initialize()
    {
        if(_fcsStorage is null)
        {
            _fcsStorage = GetComponent<FCSStorage>();
        }
        //_fcsStorage.container.onAddItem += DockDrill;

        //foreach (InventoryItem inventoryItem in _fcsStorage.container)
        //{
        //    var prefabId = inventoryItem.item.gameObject.GetComponent<PrefabIdentifier>()?.id;
        //    var deepDriller = inventoryItem.item.gameObject.GetComponent<DeepDrillerController>();

        //    var drillSave = _savedData as DeepDrillerOperatorSaveDataEntry;
            
        //    if(drillSave != null)
        //    {
        //        var drill = drillSave.Drills.FirstOrDefault(x => x.Value.UnitID == deepDriller.UnitID);
        //        var currentDrill = inventoryItem.item.GetComponent<DeepDrillerHeavyDutyController>();
        //        currentDrill.SetPosition(drill.Value.Position);
        //    }

        //    QuickLogger.Debug($"Loaded Drill {prefabId}");
        //}


        //_fcsStorage.container.onRemoveItem += UnDockServer;
        //_fcsStorage.AddAllowedTech(DeepDrillerHeavyDutySpawnable.PatchedTechType);

        CoroutineHost.StartCoroutine(LoadDrills());
        base.Initialize();
    }


    #region Grid
    private void HoverInteraction_onSettingsKeyPressed(TechType techType)
    {
        if (techType != GetTechType()) return;
        QuickLogger.Debug("Opening Settings", true);
        FCSPDAController.Main.OpenDeviceUI(GetTechType(), this, null);
    }

    private void InitialiseGrid()
    {
        Grid = new Grid2<uGUI_DDHoloGraphControl>((BUILDING_COMPACITY + 1) * 2);
    }

    internal void RefreshMST()
    {
        _graph = Grid.ToGraph();
    }
    #endregion


    internal bool CanRemovePlatform(uGUI_DDHoloGraphControl holoGraphControl)
    {
        RefreshMST();

        _neighbours = _graph.GetVertex(holoGraphControl)?.Neighbours;

        _graph.RemoveVertex(holoGraphControl);

        bool canRemove = true;
        foreach (var neighbour in _neighbours.Select(n => n.To))
        {
            if (_graph.MST(neighbour).Contains(Grid.ElementAt(Grid.Center))) continue;
            canRemove = false;
            break;
        }
        return canRemove;
    }

    /// <summary>
    /// Get all the drills connected to this operator.
    /// </summary>
    /// <returns><see cref="SortedDictionary{TKey, TValue}"/> of <see cref="ConnectedDrillData"/></returns>
    internal SortedDictionary<string, DeepDrillerHeavyDutyController> GetConnectedDrills()
    {
        return _connectedDrills;
    }

    internal int BuildingCapacity()
    {
        return BUILDING_COMPACITY;
    }

    public bool IsBreakSet()
    {
        return false;
    }

    public int GetOresPerDayCountInt()
    {
        throw new NotImplementedException();
    }

    public IEnumerable<UpgradeFunction> GetUpgrades()
    {
        throw new NotImplementedException();
    }

    public string GetOilLevel()
    {
        return string.Empty;
    }

    public void SaveDevice()
    {
       
            QuickLogger.Debug("Saving Deep Driller", true);

            if (!IsInitialized || !IsConstructed) return;

            if (_savedData == null)
            {
                _savedData = new DeepDrillerOperatorSaveDataEntry();
            }

            var save = _savedData as DeepDrillerOperatorSaveDataEntry;

            save.Id = GetPrefabID();
            save.Items = _trackedItems;
            save.ConnectedDrills = new();


        foreach (var item in _connectedDrills) 
        {
            save.ConnectedDrills.Add(item.Value.GetPrefabID());
        }
                  
        
        
        QuickLogger.Debug($"Saving ID {save.Id}", true);
        //save.ColorTemplate = _colorManager.SaveTemplate();
        FCSModsAPI.PublicAPI.PushSaveData(save);
        //newSaveData.Data.Add(save);
    }

    private IEnumerator LoadDrills()
    {
        ReadySaveData();

        var save = _savedData as DeepDrillerOperatorSaveDataEntry;

        if (save != null) 
        {
            if(save.ConnectedDrills is null)
            {
                QuickLogger.DebugError("Save Data Drills Returned Null");

                yield return null;
            }
            else
            {
                QuickLogger.Debug($"Load drills Count: {save.ConnectedDrills.Count}");


                foreach (var drill in save.ConnectedDrills) 
                {
                    //platformController.EnableSlot(drill.Value.Slot - 1);
                    yield return drill; 
                }


                //while (_connectedDrills.Count != save.Drills.Count)
                //{
                //    foreach (KeyValuePair<string, ConnectedDrillData> drillSaveData in save.Drills)
                //    {
                //        if (_connectedDrills.ContainsKey(drillSaveData.Key)) continue;
                //        var parentTurbine = FCSModsAPI.PublicAPI.FindDeviceWithPreFabID(drillSaveData.Value.ParentTurbineUnitID);
                //        var currentTurbine = FCSModsAPI.PublicAPI.FindDeviceWithPreFabID(drillSaveData.Key);



                //        if (parentTurbine != null && currentTurbine != null)
                //        {
                //            MaterialHelpers.ApplyGlassShaderTemplate(currentTurbine.gameObject, "_glass", Plugin.ModSettings.ModPackID);
                //            var parentPlatformController = parentTurbine.GetComponentInChildren<DDPlatformController>();
                //            var turbinePlatformController = currentTurbine.GetComponentInChildren<DDPlatformController>();
                //            currentTurbine.GetComponentInChildren<DDPlatformController>().LoadFromSave(drillSaveData);
                //            QuickLogger.Debug($"Adding From Save Drill {turbinePlatformController.GetMountedDevice().GetPrefabID()} with connection to {parentPlatformController.GetMountedDevice().GetPrefabID()} on port {drillSaveData.Value.Slot}");

                //            var result = AddPlatformFromSave(drillSaveData.Value.Slot, parentPlatformController, turbinePlatformController, drillSaveData.Value.HoloGraphPosition);
                //            QuickLogger.Debug($"LoadDrills Count: {save.Drills.Count} | {_connectedDrills.Count}");
                //        }
                //        else
                //        {
                //            //QuickLogger.DebugError($"Failed to find device with ID: {drillSaveData.Key}");
                //        }
                //    }
                //    yield return null;
                //}
            }
        }
        else
        {
            QuickLogger.DebugError("Save Data Returned Null");
        }

        yield break;
    }

    public bool AddPlatformFromSave(int slotID, DDPlatformController parentPlatform, DDPlatformController platform, Vector2Int position)
    {
        QuickLogger.Debug("AddPlatformFromSave : 1");
        if (Grid.Count() > BuildingCapacity() || Grid.ElementAt(position) != null) { return false; } // Grid.ElementAt(position) != null tells us if the "slot" is filled
        QuickLogger.Debug("AddPlatformFromSave : 2");

        if (platform == null) { return false; }
        QuickLogger.Debug("AddPlatformFromSave : 3");

        //var turbineController = platform.GetComponent<DDPlatformController>();

        var device = parentPlatform.GetComponentInParent<FCSDevice>();
        QuickLogger.Debug("AddPlatformFromSave : 4");

        platform.ConnectionData = new ConnectedDrillData
        {
            Slot = slotID,
            ParentTurbineUnitID = device.GetPrefabID(),
            HoloGraphPosition = position,
            Position = new FCS_AlterraHub.Models.Structs.Vec3(platform.GetComponentInParent<FCSDevice>().GetPosition()),
            UnitID = device.GetPrefabID()
        };
        QuickLogger.Debug("AddPlatformFromSave : 5");


        //MoveTurbineToPosition(platform.transform); Renable

        //var port = FindHoloPort(parentPlatform.GetUnitID(), slotID);

        //if (port == null)
        //{
        //    QuickLogger.DebugError("Failed to find hologram port");
        //    return false;
        //}  Disabled due to not controlling holograms from here

        //var holo = AddNewHolograph(port, turbine, position);

        platform.name = $"{position} {platform.name}";
        QuickLogger.Debug("AddPlatformFromSave : 6");

        //_connectedDrills.Add(platform.GetMountedDevice().GetPrefabID(), platform.ConnectionData);
        QuickLogger.Debug("AddPlatformFromSave : 7");

        //_connectedDeepDrillerPlatformBase.Add(platform.GetMountedDevice().GetPrefabID(), platform);
        QuickLogger.Debug("AddPlatformFromSave : 8");

        return true;
    }

    public override void ReadySaveData()
    {
        string id = (GetComponentInParent<PrefabIdentifier>() ?? GetComponent<PrefabIdentifier>()).Id;
        _savedData = FCSModsAPI.PublicAPI.GetSaveData<DeepDrillerOperatorSaveDataEntry>(id);
        QuickLogger.Debug($"Prefab Id : {GetPrefabID()} || SaveData Is Null: {_savedData is null}");
    }

    internal Dictionary<TechType,int> GetDDContainer()
    {
        return _trackedItems;
    }

    public void AddItemToContainer(TechType techType)
    {
        QuickLogger.Debug($"Trying to add {techType.AsString()} to drill", true);

        if (_trackedItems is not null)
        {
            if (_trackedItems.ContainsKey(techType))
            {
                _trackedItems[techType] += 1;
            }
            else
            {
                _trackedItems.Add(techType, 1);
            }

            OnStorageCountUpdated?.Invoke(GetDDContainerTotal());
        }
    }

    public void RemoveItemFromContainer(TechType techType)
    {
        QuickLogger.Debug($"Trying to remove {techType.AsString()} from drill",true);

        if(_trackedItems.ContainsKey(techType))
        {
            _trackedItems[techType] -= 1;

            if (_trackedItems[techType] <= 0)
            {
                _trackedItems.Remove(techType);
                PlayerInteractionHelper.GivePlayerItem(techType);
                OnStorageCountUpdated?.Invoke(GetDDContainerTotal());
            }
        }
        else
        {
            QuickLogger.Debug($"Item {techType.AsString()} not found.");
        }
    }

    internal int GetItemCount(TechType techType)
    {
        return _trackedItems[techType];
    }

    internal int GetDDContainerTotal()
    {
        int amount = 0;

        foreach (var item in _trackedItems)
        {
            amount += item.Value;
        }

        return amount;
    }

    public DDPlatformController GetPlatformController()
    {
        return platformController;
    }
}
