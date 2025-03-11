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

internal class DeepDrillerOperatorController : FCSDevice, IFCSSave<SaveData>,IDrillSystem
{
    public GameObject DrillsGroupLocation;
    private readonly SortedDictionary<string, ConnectedDrillData> _connectedDrills = new();

    [SerializeField] private GameObject holoGramPrefab;


    internal bool HasRootHolo;


    private SortedDictionary<string, DDPlatformController> _connectedDeepDrillerPlatformBase = new();


    private Graph<uGUI_DDHoloGraphControl> _graph;


    private List<Graph<uGUI_DDHoloGraphControl>.Edge> _neighbours;




    private const int BUILDING_COMPACITY = 15;
    internal Dictionary<TechType, int> _trackedItems = new();
    internal Action<int> OnStorageCountUpdated;
    private FCSStorage _fcsStorage;
    [SerializeField] private HoverInteraction hoverInteraction;
    [SerializeField] private FCSStorage storage;
    string db_name = "URI=file:data.db";

    //[SerializeField] private FCSDeepDrillerContainer _deepDrillerContainer;

    [HideInInspector] public Grid2<uGUI_DDHoloGraphControl> Grid { get; private set; }
    public GameObject GameObject => gameObject;

    public override void Start()
    {
        base.Start();
        InitialiseGrid();
        hoverInteraction.onSettingsKeyPressed += HoverInteraction_onSettingsKeyPressed;        
    }

    internal bool GetItemWithId(string prefabID,out GameObject result)
    {
        var childObjects = GetComponentsInChildren<PrefabIdentifier>();

        QuickLogger.Debug($"Child Count: {childObjects.Length}");

        foreach (var prefabIdentifier in childObjects)
        {
            QuickLogger.Debug($"Checking if id: {prefabIdentifier.Id} matches {prefabID}. Result = {prefabIdentifier.id.Equals(prefabID)}");

            if(prefabIdentifier.id.Equals(prefabID))
            {
                result = prefabIdentifier.gameObject;
                return true;
            }
        }

        result = null;
        return false;
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
        _fcsStorage.AddAllowedTech(DeepDrillerHeavyDutySpawnable.PatchedTechType);

        CoroutineHost.StartCoroutine(LoadDrills());
        base.Initialize();
    }

    private void DockDrill(InventoryItem item)
    {
        var go = item.item;
        var prefabId = item.item.gameObject.GetComponent<PrefabIdentifier>().id;
       QuickLogger.Debug($"Added drill {prefabId}",true);
        
        //using(SqliteConnection connection = new SqliteConnection(db_name))
        //{
        //    connection.Open();
        //    using (var command = connection.CreateCommand())
        //    {
        //        command.CommandText = $"INSERT INTO tbl_drills (id,x,y,z) VALUES ('{prefabId}',{go.transform.localPosition.x},{go.transform.localPosition.y},{go.transform.localPosition.z})";
        //        command.ExecuteNonQuery();
        //    }
        //}
    }

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
    internal SortedDictionary<string, ConnectedDrillData> GetConnectedDrills()
    {
        return _connectedDrills;
    }

    internal SortedDictionary<string, DDPlatformController> GetConnectedDrillPlatforms()
    {
        return _connectedDeepDrillerPlatformBase;
    }

    internal int BuildingCapacity()
    {
        return BUILDING_COMPACITY;
    }

    internal void AddPlatformBase(string id, DDPlatformController plateformController)
    {
        _connectedDeepDrillerPlatformBase.Add(id, plateformController);

        var deepDriller = plateformController.GetMountedDevice().GameObject.GetComponent<DeepDrillerHeavyDutyController>();

        var f = deepDriller.GetComponent<Pickupable>();

        storage.container.UnsafeAdd(f.inventoryItem);

        deepDriller.GetStorage().OnContainerAddItem += Subscribe;
    }

    private void Subscribe(FCSDevice device, TechType type)
    {
        QuickLogger.Debug("It Works", true);
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

    public void Save(SaveData newSaveData, ProtobufSerializer serializer = null)
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

        foreach (var item in GetConnectedDrillPlatforms())
        {
            var drill = item.Value.gameObject.GetComponent<FCSDeepDrillerOilHandler>();
            //var j = _connectedDrills.FirstOrDefault(x => x.Value);
        }   

            save.Drills = _connectedDrills;
        
        
            QuickLogger.Debug($"Saving ID {save.Id}", true);
            //save.ColorTemplate = _colorManager.SaveTemplate();

            newSaveData.Data.Add(save);
    }

    private IEnumerator LoadDrills()
    {
        ReadySaveData();

        var save = _savedData as DeepDrillerOperatorSaveDataEntry;

        if (save != null) 
        {
            if(save.Drills is null)
            {
                QuickLogger.DebugError("Save Data Drills Returned Null");

                yield return null;
            }
            else
            {
                QuickLogger.Debug($"Load drills Count: {save.Drills.Count}");


                while (_connectedDrills.Count != save.Drills.Count)
                {
                    foreach (KeyValuePair<string, ConnectedDrillData> drillSaveData in save.Drills)
                    {
                        if (_connectedDrills.ContainsKey(drillSaveData.Key)) continue;
                        var parentTurbine = FCSModsAPI.PublicAPI.FindDeviceWithPreFabID(drillSaveData.Value.ParentTurbineUnitID);
                        var currentTurbine = FCSModsAPI.PublicAPI.FindDeviceWithPreFabID(drillSaveData.Key);



                        if (parentTurbine != null && currentTurbine != null)
                        {
                            MaterialHelpers.ApplyGlassShaderTemplate(currentTurbine.gameObject, "_glass", Plugin.ModSettings.ModPackID);
                            var parentPlatformController = parentTurbine.GetComponentInChildren<DDPlatformController>();
                            var turbinePlatformController = currentTurbine.GetComponentInChildren<DDPlatformController>();
                            currentTurbine.GetComponentInChildren<DDPlatformController>().LoadFromSave(drillSaveData);
                            QuickLogger.Debug($"Adding From Save Drill {turbinePlatformController.GetMountedDevice().GetPrefabID()} with connection to {parentPlatformController.GetMountedDevice().GetPrefabID()} on port {drillSaveData.Value.Slot}");

                            var result = AddPlatformFromSave(drillSaveData.Value.Slot, parentPlatformController, turbinePlatformController, drillSaveData.Value.HoloGraphPosition);
                            QuickLogger.Debug($"LoadDrills Count: {save.Drills.Count} | {_connectedDrills.Count}");
                        }
                        else
                        {
                            QuickLogger.DebugError($"Failed to find device with ID: {drillSaveData.Key}");
                        }
                    }
                    yield return null;
                }
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

        _connectedDrills.Add(platform.GetMountedDevice().GetPrefabID(), platform.ConnectionData);
        QuickLogger.Debug("AddPlatformFromSave : 7");

        _connectedDeepDrillerPlatformBase.Add(platform.GetMountedDevice().GetPrefabID(), platform);
        QuickLogger.Debug("AddPlatformFromSave : 8");

        return true;
    }

    public override void ReadySaveData()
    {
        string id = (GetComponentInParent<PrefabIdentifier>() ?? GetComponent<PrefabIdentifier>()).Id;
        _savedData = ModSaveManager.GetSaveData<DeepDrillerOperatorSaveDataEntry>(id);
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
 
    internal bool GetPosition(DeepDrillerHeavyDutyController dDrill,out Vector3 result)
    {
        QuickLogger.Debug($"Controller ID: {dDrill.GetPrefabID()}");

        result = new();

        var drill = ((DeepDrillerOperatorSaveDataEntry)_savedData)?.Drills?.FirstOrDefault(x => x.Key == dDrill.GetPrefabID());

        if (drill is null) return false;

        QuickLogger.Debug($"Is dril null: {drill is null}");

        var data = drill.Value.Value;

        QuickLogger.Debug($"Result: {new Vector3(data.Position.X, data.Position.Y, data.Position.Z)}");

        result = new Vector3(data.Position.X, data.Position.Y, data.Position.Z);
        return true;
    }
}
