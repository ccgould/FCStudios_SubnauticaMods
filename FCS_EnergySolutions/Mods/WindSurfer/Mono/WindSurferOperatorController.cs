using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using FCS_AlterraHub.Buildables;
using FCS_AlterraHub.Extensions;
using FCS_AlterraHub.Helpers;
using FCS_AlterraHub.Model;
using FCS_AlterraHub.Mono;
using FCS_AlterraHub.Registration;
using FCS_EnergySolutions.Buildable;
using FCS_EnergySolutions.Configuration;
using FCS_EnergySolutions.Mods.TelepowerPylon.Model;
using FCS_EnergySolutions.Mods.TelepowerPylon.Mono;
using FCS_EnergySolutions.Mods.WindSurfer.Enums;
using FCS_EnergySolutions.Mods.WindSurfer.Model;
using FCS_EnergySolutions.Mods.WindSurfer.Structs;
using FCSCommon.Helpers;
using FCSCommon.Utilities;
using UnityEngine;
using UnityEngine.UI;

namespace FCS_EnergySolutions.Mods.WindSurfer.Mono
{
    internal class WindSurferOperatorController : WindSurferPlatformBase, ITelepowerPylonConnection
    {
        private WindSurferOperatorDataEntry _savedData;
        private bool _fromSave;
        private DoorSensor _doorSensor;
        private MotorHandler _motor;
        private Rigidbody _rb;
        private int BuildingCapacity = 15;
        private PlatformController _platformController;
        private float _timeBuildCompleted = -1f;
        private bool _isBuilding;

        private SortedDictionary<string, ConnectedTurbineData> _connectedTurbines = new SortedDictionary<string, ConnectedTurbineData>();
        private SortedDictionary<string, WindSurferPlatformBase> _connectedTurbinesController = new SortedDictionary<string, WindSurferPlatformBase>();
        private bool _saveTurbinesLoaded;
        private List<HoloGraphControl> _holograms = new List<HoloGraphControl>();
        private List<Graph<HoloGraphControl>.Edge> _neighbours;
        private Graph<HoloGraphControl> _graph;
        private readonly Dictionary<string, ITelepowerPylonConnection> _currentConnections = new Dictionary<string, ITelepowerPylonConnection>();

        public Grid2<HoloGraphControl> Grid { get; private set; }
        public GameObject HologramsGrid;
        private Text _unitIDText;
        private Text _powerInfoText;
        private Text _turbineCountText;
        private BasePowerRelay _powerRelay;
        internal ScreenTrigger ScreenTrigger { get; private set; }
        public override PlatformController PlatformController => _platformController ?? (_platformController = GetComponent<PlatformController>());
        public string GetUnitID()
        {
            if (string.IsNullOrWhiteSpace(UnitID))
            {
                FCSAlterraHubService.PublicAPI.RegisterDevice(this, Mod.WindSurferOperatorTabID, Mod.ModPackID);
            }

            return UnitID;
        }
        public GameObject TurbinesGroupLocation { get; set; }
        public override bool BypassRegisterCheck { get; } = true;


        private void Start()
        {
            GetUnitID();
            _rb = gameObject.GetComponent<Rigidbody>();

            InitialiseGrid();

            var holo = AddMainHolograph();
            holo.gameObject.name = $"{Grid.Center} {holo.gameObject.name}";
            Grid.Add(holo, Grid.Center);
        }

        private void OnEnable()
        {
            if (!IsInitialized)
            {
                Initialize();
            }

            if (_savedData == null)
            {
                ReadySaveData();
            }

            if (_fromSave)
            {
                if (_savedData.CurrentConnections != null)
                {
                    //_connectedTurbines = _savedData.CurrentConnections;
                }
                
                //_colorManager.ChangeColor(_saveData.Body.Vector4ToColor());
               //_colorManager.ChangeColor(_saveData.SecondaryBody.Vector4ToColor(), ColorTargetMode.Secondary);
                _fromSave = false;
            }
        }

        private void FixedUpdate()
        {
            if (!_rb.isKinematic && !_isBuilding && _timeBuildCompleted + 3f < Time.time &&_rb.velocity.sqrMagnitude < 0.1f)
            {
                _rb.isKinematic = true;
            }
        }

        public void SubConstructionComplete()
        {
            _rb.isKinematic = false;
            _isBuilding = false;
            _timeBuildCompleted = Time.time;
        }

        private void ReadySaveData()
        {
            QuickLogger.Debug("In OnProtoDeserialize");
            var prefabIdentifier = GetComponentInParent<PrefabIdentifier>() ?? GetComponent<PrefabIdentifier>();
            var id = prefabIdentifier?.Id ?? string.Empty;
            _savedData = Mod.GetWindSurferOperatorSaveData(id);
        }

        public override void Initialize()
        {
            //Get ladders
            CreateLadders();
            
            if (_doorSensor == null)
            {
                _doorSensor = gameObject.FindChild("DoorTrigger").EnsureComponent<DoorSensor>();
                _doorSensor.Initialize(GameObjectHelpers.FindGameObject(gameObject, "DoorController"));
            }

            if (_unitIDText == null)
            {
                _unitIDText = GameObjectHelpers.FindGameObject(gameObject,"UnitID").GetComponent<Text>();
            }

            if (_powerInfoText == null)
            {
                _powerInfoText = GameObjectHelpers.FindGameObject(gameObject, "PowerInfoText").GetComponent<Text>();
            }

            if (_turbineCountText == null)
            {
                _turbineCountText = GameObjectHelpers.FindGameObject(gameObject, "TurbineCountText").GetComponent<Text>();
            }

            if (_powerRelay == null)
            {
                _powerRelay = gameObject.GetComponent<BasePowerRelay>();
            }

            if (_motor == null)
            {
                _motor = GameObjectHelpers.FindGameObject(gameObject, "Antenna_anim_mesh").EnsureComponent<MotorHandler>();
                _motor.Initialize(30);
            }

            if (HologramsGrid == null)
            {
                HologramsGrid = GameObjectHelpers.FindGameObject(gameObject, "Holograms");
            }

            if (TurbinesGroupLocation == null)
            {
                TurbinesGroupLocation = GameObjectHelpers.FindGameObject(gameObject, "Turbines");
            }

            if (ScreenTrigger == null)
            {
                ScreenTrigger = gameObject.GetComponentInChildren<Canvas>().gameObject.EnsureComponent<ScreenTrigger>();
            }

            var kitModeToggle = gameObject.GetComponentInChildren<Toggle>();
            kitModeToggle.onValueChanged.AddListener((value =>
            {
                PlatFormKitMode = value ? PlatformKitModes.TurbinePlatform : PlatformKitModes.EmptyPlatform;
            }));

            if (_messageBox == null)
            {
                _messageBox = GameObjectHelpers.FindGameObject(gameObject, "MessageBox").AddComponent<FCSMessageBox>();
            }

            _windSurferKitTechType = Mod.WindSurferKitClassID.ToTechType();
            _operatorKitTechType = Mod.WindSurferOperatorKitClassID.ToTechType();
            _windSurferPlatformKitTechType = Mod.WindSurferPlatformKitClassID.ToTechType();
            
            InvokeRepeating(nameof(CheckTeleportationComplete), 0.2f, 0.2f);
            InvokeRepeating(nameof(UpdateData), 0.1f, 0.1f);
            
            _turbinesTransform = GameObjectHelpers.FindGameObject(gameObject, "Turbines").transform;

            IsInitialized = true;

            QuickLogger.Debug($"Initialized");

        }

        public PlatformKitModes PlatFormKitMode = PlatformKitModes.TurbinePlatform;
        private FCSMessageBox _messageBox;
        private TechType _windSurferKitTechType;
        private TechType _operatorKitTechType;
        private TechType _windSurferPlatformKitTechType;
        private Transform _turbinesTransform;
        private int _childPlacementsCorrect;
        private bool _removingPlatform;
        private FCSStorage _storage;


        private void UpdateData()
        {
            if (_powerInfoText != null && _unitIDText != null && _turbineCountText != null)
            {
                _powerInfoText.text = $"{Mathf.RoundToInt(_powerRelay.GetPower())}/{Mathf.RoundToInt(_powerRelay.GetMaxPower())}";
                _unitIDText.text = UnitID;
                _turbineCountText.text = $"{_connectedTurbines.Count}/{BuildingCapacity}";
            }
        }

        private void CheckTeleportationComplete()
        {
            QuickLogger.Debug("Checking if world is settled");
            if (LargeWorldStreamer.main.IsWorldSettled())
            {
                if(_savedData?.CurrentConnections != null)
                    StartCoroutine(LoadTurbines());
                CancelInvoke(nameof(CheckTeleportationComplete));
            }
        }

        private IEnumerator LoadTurbines()
        {
            QuickLogger.Debug($"LoadTurbines Count: {_savedData.CurrentConnections.Count}");

            while (_connectedTurbines.Count != _savedData.CurrentConnections.Count)
            {
                foreach (KeyValuePair<string, ConnectedTurbineData> turbineSaveData in _savedData.CurrentConnections)
                {
                    if(_connectedTurbines.ContainsKey(turbineSaveData.Key)) continue;
                    var parentTurbine = FCSAlterraHubService.PublicAPI.FindDeviceWithPreFabID(turbineSaveData.Value.ParentTurbineUnitID);
                    var currentTurbine = FCSAlterraHubService.PublicAPI.FindDeviceWithPreFabID(turbineSaveData.Key);

                    if (parentTurbine.Value != null && currentTurbine.Value != null)
                    {
                        var parentPlatformController = ((WindSurferPlatformBase)parentTurbine.Value).PlatformController;
                        var turbinePlatformController = ((WindSurferPlatformBase)currentTurbine.Value).PlatformController;

                        QuickLogger.Debug($"Adding From Save Turbine {turbinePlatformController.GetUnitID()} with connection to {parentPlatformController.GetUnitID()} on port {turbineSaveData.Value.Slot}");

                        var result = AddPlatformFromSave(turbineSaveData.Value.Slot, parentPlatformController, turbinePlatformController, turbineSaveData.Value.HoloGraphPosition);
                        QuickLogger.Debug($"LoadTurbines Count: {_savedData.CurrentConnections.Count} | {_connectedTurbines.Count}");
                    }
                    else
                    {
                        QuickLogger.DebugError($"Failed to find device with ID: {turbineSaveData.Key}");
                    }
                }
                yield return null;
            }

            //InvokeRepeating(nameof(CheckPlatformPlacements), 1f, 1f);
            RefreshHoloGrams();
            yield break;
        }

        private void CreateLadders()
        {
            var t01 = GameObjectHelpers.FindGameObject(gameObject, "T01").EnsureComponent<LadderController>();
            t01.Set(GameObjectHelpers.FindGameObject(gameObject, "T01_Top"));

            var t02 = GameObjectHelpers.FindGameObject(gameObject, "T02").EnsureComponent<LadderController>();
            t02.Set(GameObjectHelpers.FindGameObject(gameObject, "T02_Top"));

            var t03 = GameObjectHelpers.FindGameObject(gameObject, "T03").EnsureComponent<LadderController>();
            t03.Set(GameObjectHelpers.FindGameObject(gameObject, "T03_Top"));

            var t04 = GameObjectHelpers.FindGameObject(gameObject, "T04").EnsureComponent<LadderController>();
            t04.Set(GameObjectHelpers.FindGameObject(gameObject, "T04_Top"));
        }

        public override void OnProtoSerialize(ProtobufSerializer serializer)
        {
            if (!Mod.IsSaving())
            {
                QuickLogger.Info($"Saving {Mod.WindSurferOperatorFriendlyName}");
                Mod.Save(serializer);
                QuickLogger.Info($"Saved {Mod.WindSurferOperatorFriendlyName}");
            }
        }

        public override void OnProtoDeserialize(ProtobufSerializer serializer)
        {
            _fromSave = true;
        }

        public override bool CanDeconstruct(out string reason)
        {
            reason = string.Empty;
            return true;
        }

        public override void OnConstructedChanged(bool constructed)
        {
            
        }

        public override void Save(SaveData newSaveData, ProtobufSerializer serializer = null)
        {
            if (!IsInitialized) return;

            if (_savedData == null)
            {
                _savedData = new WindSurferOperatorDataEntry();
            }

            _savedData.Id = GetPrefabID();

            QuickLogger.Debug($"Saving ID {_savedData.Id}", true);
            //_savedData.Body = _colorManager.GetColor().ColorToVector4();
            _savedData.BaseId = BaseId;
            _savedData.CurrentConnections = _connectedTurbines;
            newSaveData.WindSurferOperatorEntries.Add(_savedData);
        }

        private void InitialiseGrid()
        {
            Grid = new Grid2<HoloGraphControl>((BuildingCapacity + 1) * 2);
        }

        public bool AddPlatform(HolographSlot slot, Vector2Int position)
        {
            if (!CheckForKit())
            {
                var kit = PlatFormKitMode == PlatformKitModes.TurbinePlatform
                    ? Language.main.Get(_windSurferKitTechType)
                    : Language.main.Get(_windSurferPlatformKitTechType);
                _messageBox.Show($"No {kit} found in your inventory.", FCSMessageButton.OK, null);
                return false;
            }

           var kitPickupable =  Inventory.main.container.RemoveItem(PlatFormKitMode == PlatformKitModes.TurbinePlatform
                ? _windSurferKitTechType
                : _windSurferPlatformKitTechType);

           Destroy(kitPickupable.gameObject);

            if (Grid.Count() > BuildingCapacity || Grid.ElementAt(position) != null) { return false; } // Grid.ElementAt(position) != null tells us if the "slot" is filled
            
            QuickLogger.Debug($"Adding Turbine to Port: {slot.Target.name}|{slot.ID}");
            
            var turbine = slot.PlatformController.AddNewPlatForm(slot.Target, slot.ID, CraftData.GetPrefabForTechType(PlatFormKitMode ==  PlatformKitModes.TurbinePlatform ? Mod.WindSurferClassName.ToTechType() : Mod.WindSurferPlatformClassName.ToTechType()));
            if (turbine == null) { return false; }

            foreach (Collider builtCollider in turbine.GetComponentsInChildren<Collider>() ?? new Collider[0])
            {
                foreach (Collider collider in gameObject.GetComponentsInChildren<Collider>() ?? new Collider[0])
                {
                    Physics.IgnoreCollision(collider, builtCollider);
                }
            }


            var turbineController = turbine.GetComponent<WindSurferPlatformBase>();

            slot.PlatformController.ConnectionData = new ConnectedTurbineData
            {
                    Slot = slot.ID,
                    ParentTurbineUnitID = slot.Target.GetComponentInParent<FcsDevice>().GetPrefabID(),
                    HoloGraphPosition = position
            };
            
            var holo = AddNewHolograph(slot, turbine);

            holo.gameObject.name = $"{position} {holo.gameObject.name}";

            turbine.name = $"{position} {turbine.name}";

            Grid.Add(holo, position);

            _connectedTurbines.Add(turbineController.GetPrefabID(),slot.PlatformController.ConnectionData);

            _connectedTurbinesController.Add(turbineController.GetPrefabID(),turbineController);

            _powerRelay.AddInboundPower(turbineController.PlatformController.GetPowerSource());
            return true;
        }

        private bool CheckForKit()
        {
            return PlayerInteractionHelper.HasItem(PlatFormKitMode == PlatformKitModes.TurbinePlatform ? _windSurferKitTechType : _windSurferPlatformKitTechType);
        }

        public bool AddPlatformFromSave(int slotID,PlatformController parentPlatform, PlatformController turbine, Vector2Int position)
        {
            if (Grid.Count() > BuildingCapacity || Grid.ElementAt(position) != null) { return false; } // Grid.ElementAt(position) != null tells us if the "slot" is filled
            
            if (turbine == null) { return false; }
            
            var turbineController = turbine.GetComponent<WindSurferPlatformBase>();


            turbine.ConnectionData = new ConnectedTurbineData
            {
                Slot = slotID,
                ParentTurbineUnitID = parentPlatform.GetComponentInParent<FcsDevice>().GetPrefabID(),
                HoloGraphPosition = position
            };

            MoveTurbineToPosition(turbine.transform);

            var port = FindHoloPort(parentPlatform.GetUnitID(), slotID);

            if (port == null)
            {
                QuickLogger.Error("Failed to find hologram port");
                return false;
            }

            var holo = AddNewHolograph(port, turbine,position);
            turbine.name = $"{position} {turbine.name}";
            _connectedTurbines.Add(turbineController.GetPrefabID(), turbine.ConnectionData);
            _connectedTurbinesController.Add(turbineController.GetPrefabID(), turbineController);
            return true;
        }

        private void MoveTurbineToPosition(Transform turbineTransform)
        {
            QuickLogger.Debug($"Moving Platform: {turbineTransform.name}");
            EnsureIsKinematic();
            var turbineController = turbineTransform.GetComponent<WindSurferPlatformBase>();
            turbineController.TryMoveToPosition();
            turbineController.PoleState(true);
            QuickLogger.Debug($"Setting as child. Platform: {turbineTransform.name}");
            MoveToTurbinesLocation(turbineTransform);
            QuickLogger.Debug($"Platform: {turbineTransform.name} was set as child. Parent: {turbineTransform.parent?.name}");
        }

        public void MoveToTurbinesLocation(Transform turbineTransform)
        {
            turbineTransform.SetParent(_turbinesTransform);
        }

        private Transform FindHoloPort(string holoPortUnitId,int slotID)
        {
            foreach (HoloGraphControl holoGraphControl in _holograms)
            {
                if (holoGraphControl.PlatFormController.GetUnitID().Equals(holoPortUnitId))
                {
                    QuickLogger.Debug($"Found Hologram: {holoPortUnitId}");
                    holoGraphControl.FindSlots();
                    var trans = holoGraphControl.Slots.FirstOrDefault(x=>x.ID==slotID)?.transform;
                    var result = trans != null ? "Found" : "Cannot find";
                    QuickLogger.Debug($"{result} Hologram Port: {slotID}");
                    return trans;
                }
            }

            return null;
        }

        public bool TryRemovePlatform(PlatformController controller)
        {
            if (_removingPlatform) return false;
            TechType kit;

            switch (controller.GetPlatformType())
            {
                case HolographIconType.Operator:
                    kit = _operatorKitTechType;
                    break;
                case HolographIconType.Turbine:
                    kit = _windSurferKitTechType;
                    break;
                case HolographIconType.Platform:
                    kit = _windSurferPlatformKitTechType;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            if (!PlayerInteractionHelper.CanPlayerHold(kit))
            {
                _messageBox.Show(AlterraHub.InventoryFull(),FCSMessageButton.OK,null);
                return false;
            }

            PlayerInteractionHelper.GivePlayerItem(kit);

            RefreshMST();

            var holoGraphControl = controller.HoloGraphControl;

            var canRemove = CanRemovePlatform(holoGraphControl);

            if (canRemove)
            {
                if (controller.GetUnitID().Equals(UnitID,StringComparison.OrdinalIgnoreCase))
                {
                    StartCoroutine(DestroyOperator(controller, holoGraphControl));
                }
                else
                {
                    RemovePlatform(controller, holoGraphControl);
                }

                return true;
            }

            return false;
        }

        private void RemovePlatform(PlatformController controller, HoloGraphControl holoGraphControl)
        {
            _powerRelay.RemoveInboundPower(controller.GetPowerSource());
            _connectedTurbines.Remove(controller.GetPrefabID());
            _connectedTurbinesController.Remove(controller.GetPrefabID());
            Grid.Remove(holoGraphControl);
            _holograms.Remove(holoGraphControl);
            Destroy(controller.gameObject);
            Destroy(holoGraphControl.gameObject);
        }

        private IEnumerator DestroyOperator(PlatformController controller, HoloGraphControl holoGraphControl)
        {
            QuickLogger.ModMessage( $"Please leave Wind Surfer Operator. This building will deconstruct in 5 seconds.");
            _removingPlatform = true;
            yield return new WaitForSeconds(5);
            RemovePlatform(controller, holoGraphControl);
        }

        internal bool CanRemovePlatform(HoloGraphControl holoGraphControl)
        {
            _neighbours = _graph.GetVertex(holoGraphControl)?.Neighbours;

            _graph.RemoveVertex(holoGraphControl);

            bool canRemove = true;
            foreach (var neighbour in _neighbours.Select(n => n.To))
            {
                if (!_graph.MST(neighbour).Contains(Grid.ElementAt(Grid.Center)))
                {
                    canRemove = false;
                    break;
                }
            }
            return canRemove;
        }

        private void RefreshMST()
        {
            _graph = Grid.ToGraph();
        }

        private void CheckPlatformPlacements()
        {
            foreach (var controller in _connectedTurbinesController)
            {
                if (controller.Value.transform.parent != TurbinesGroupLocation.transform)
                {
                    MoveToTurbinesLocation(controller.Value.transform);
                    _childPlacementsCorrect += 1;
                }
            }

            if (_childPlacementsCorrect == _connectedTurbinesController.Count)
            {
                CancelInvoke(nameof(CheckPlatformPlacements));
            }
        }

        internal void RefreshHoloGrams()
        {
            RefreshMST();

            foreach (HoloGraphControl control in _holograms)
            {
                control.RefreshDeleteButton(CanRemovePlatform(control));
            }
        }

        private HoloGraphControl CreateHoloGraph(PlatformController controller)
        {
            QuickLogger.Debug($"Creating HoloGraph for Platform: {controller.GetUnitID()}",true);
            
            var holoGraph = Instantiate(ModelPrefab.HoloGramPrefab);
            
            var holoGraphControl = holoGraph.GetComponent<HoloGraphControl>();
            holoGraphControl.PlatFormController = controller;
            holoGraphControl.FindSlots();

            foreach (var item in holoGraphControl.Slots.Select((slot, index) => new { slot, index }))
            {
                item.slot.Target = controller.Ports[item.index].transform;
            }

            _holograms.Add(holoGraphControl);

            return holoGraphControl;
        }

        private HoloGraphControl AddMainHolograph()
        {
            var holoGraphControl = CreateHoloGraph(PlatformController);
            PlatformController.HoloGraphControl = holoGraphControl;
            holoGraphControl.transform.SetParent(HologramsGrid.transform,true);
            holoGraphControl.transform.localPosition = Vector3.zero;
            holoGraphControl.transform.localRotation = Quaternion.identity;
            holoGraphControl.transform.localScale = new Vector3(0.2f, 0.2f, 0.2f);

            return holoGraphControl;
        }

        public HoloGraphControl AddNewHolograph(HolographSlot slot, GameObject go)
        {
            QuickLogger.Debug($"Slot: {slot} | GameObject: {go}");
            var platformController = go.GetComponent<PlatformController>();
            var holoGraphControl = CreateHoloGraph(platformController);
            holoGraphControl.SetIcon(platformController.GetPlatformType());
            platformController.HoloGraphControl = holoGraphControl;
            holoGraphControl.transform.SetParent(slot.transform);
            holoGraphControl.transform.localPosition = new Vector3(123.92f, 0f, 0f);
            holoGraphControl.transform.SetParent(HologramsGrid.transform, true);
            holoGraphControl.transform.localScale = new Vector3(0.2f, 0.2f, 0.2f);
            holoGraphControl.transform.localRotation = Quaternion.identity;
            return holoGraphControl;
        }

        public HoloGraphControl AddNewHolograph(Transform slot, PlatformController platformController,Vector2Int position)
        {
            var holoGraphControl = CreateHoloGraph(platformController);
            holoGraphControl.gameObject.name = $"{position} {holoGraphControl.gameObject.name}";
            holoGraphControl.SetIcon(platformController.GetPlatformType());
            platformController.HoloGraphControl = holoGraphControl;
            holoGraphControl.transform.SetParent(slot);
            holoGraphControl.transform.localPosition = new Vector3(123.92f, 0f, 0f);
            holoGraphControl.transform.SetParent(HologramsGrid.transform, true);
            holoGraphControl.transform.localScale = new Vector3(0.2f, 0.2f, 0.2f);
            holoGraphControl.transform.localRotation = Quaternion.identity;
            Grid.Add(holoGraphControl, position);
            return holoGraphControl;
        }

        public void EnsureIsKinematic()
        {
            if (_rb == null) return;
            _rb.isKinematic = true;
        }

        private void AddConnection(string text, FcsDevice unit)
        {
            var controller = (ITelepowerPylonConnection)unit;
            _currentConnections.Add(text.ToLower(), controller);
        }

        public TelepowerPylonMode GetCurrentMode()
        {
            return TelepowerPylonMode.PUSH;
        }

        public Action<ITelepowerPylonConnection> OnDestroyCalledAction { get; set; }

        public void AddPullPylon(ITelepowerPylonConnection telepowerPylonConnection)
        {
            AddConnection(telepowerPylonConnection.UnitID,telepowerPylonConnection.GetDevice());
        }

        public IPowerInterface GetPowerRelay()
        {
            return _powerRelay;
        }

        public FcsDevice GetDevice()
        {
            return this;
        }

        public void DeleteFrequencyItemAndDisconnectRelay(string unitID)
        {
            DeleteFrequencyItem(unitID.ToLower());
        }

        private void DeleteFrequencyItem(string id)
        {
            QuickLogger.Debug($"Attempting to delete current connection {id}", true);
            if (_currentConnections.ContainsKey(id))
            {
                _currentConnections.Remove(id);
            }
            else
            {
                QuickLogger.Debug($"Failed to find connection in the list: {id}");
            }
        }

        public bool HasConnection(string unitKey)
        {
            foreach (KeyValuePair<string, ITelepowerPylonConnection> connection in _currentConnections)
            {
                if ((PowerRelay) connection.Value.GetPowerRelay() == _powerRelay)
                {
                    return true;
                }
            }

            return false;
        }

        public bool HasPowerRelayConnection(IPowerInterface getPowerRelay)
        {
            return false;
        }
    }

    internal enum PlatformKitModes
    {
        TurbinePlatform,
        EmptyPlatform
    }

    internal struct PowerInfo
    {
        internal float Power;
        internal float Total;
    }
}
