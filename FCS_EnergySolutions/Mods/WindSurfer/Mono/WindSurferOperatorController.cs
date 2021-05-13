using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using FCS_AlterraHub.Mono;
using FCS_AlterraHub.Mono.OreConsumer;
using FCS_AlterraHub.Registration;
using FCS_EnergySolutions.Buildable;
using FCS_EnergySolutions.Configuration;
using FCS_EnergySolutions.Mods.WindSurfer.Model;
using FCSCommon.Extensions;
using FCSCommon.Helpers;
using FCSCommon.Utilities;
using UnityEngine;
using UnityEngine.UI;

namespace FCS_EnergySolutions.Mods.WindSurfer.Mono
{
    internal class WindSurferOperatorController : FcsDevice, IFCSSave<SaveData>, IPlatform
    {
        private WindSurferOperatorDataEntry _savedData;
        private bool _fromSave;
        private DoorSensor _doorSensor;
        private MotorHandler _motor;
        private Rigidbody _rb;
        private int Capacity = 15;


        public Grid2<HoloGraphControl> Grid { get; private set; }
        public GameObject HologramsGrid;
        private PlatformController _platformController;

        private ScrollRect _scrollRect;
        private ScreenTrigger _screenTrigger;
        private Light[] _lights;
        private HashSet<Tuple<int, string,string, Vector2Int>> _connectedTurbines = new HashSet<Tuple<int, string,string, Vector2Int>>();
        private bool _saveTurbinesLoaded;
        private List<HoloGraphControl> _holograms = new List<HoloGraphControl>();
        public PlatformController PlatformController => _platformController ?? (_platformController = GetComponent<PlatformController>());
        public GameObject Turbines { get; set; }
        public override bool BypassRegisterCheck { get; } = true;

        private void FixedUpdate()
        {
            if(_rb == null) return;
            if (!_rb.isKinematic && (gameObject.transform.position.y > -0.05f && gameObject.transform.position.y < 0))
            {
                _rb.isKinematic = true;
            }
        }

        private void Start()
        { 
            FCSAlterraHubService.PublicAPI.RegisterDevice(this, Mod.WindSurferOperatorTabID, Mod.ModName);
            _rb = gameObject.GetComponent<Rigidbody>();

            InitialiseGrid();

            var holo = AddMainHolograph();
            holo.gameObject.name = $"{Grid.Center} {holo.gameObject.name}";
            Grid.Add(holo, Grid.Center);
        }

        public override void OnDestroy()
        {
            base.OnDestroy();
            Mod.OnLightsEnabledToggle -= OnLightsEnabledToggle;
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
            
            _lights = gameObject.GetComponentsInChildren<Light>();

            Mod.OnLightsEnabledToggle += OnLightsEnabledToggle;

            if (_doorSensor == null)
            {
                _doorSensor = gameObject.FindChild("DoorTrigger").EnsureComponent<DoorSensor>();
                _doorSensor.Initialize(GameObjectHelpers.FindGameObject(gameObject, "DoorController"));
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

            if (Turbines == null)
            {
                Turbines = GameObjectHelpers.FindGameObject(gameObject, "Turbines");
            }

            if (_screenTrigger == null)
            {
                _screenTrigger = GameObjectHelpers.FindGameObject(gameObject, "ScreenTrigger").AddComponent<ScreenTrigger>();
            }


            InvokeRepeating(nameof(CheckTeleportationComplete), 0.2f, 0.2f);

            IsInitialized = true;

            QuickLogger.Debug($"Initialized");
        }

        private void OnLightsEnabledToggle(bool value)
        {
            if (_lights != null)
            {
                foreach (Light light in _lights)
                {
                    light.gameObject.SetActive(value);
                }
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
            while (_savedData.CurrentConnections.Count != 0)
            {
                for (int i = _savedData.CurrentConnections.Count - 1; i >= 0; i--)
                {
                    var turbine = _savedData.CurrentConnections.ElementAt(i);
                    if (turbine.Item4.x == 9 && turbine.Item4.y == 9) continue;

                    var parent = FCSAlterraHubService.PublicAPI.FindDevice(turbine.Item2);
                    var device = FCSAlterraHubService.PublicAPI.FindDevice(turbine.Item3);


                    if (parent.Value != null && device.Value != null)
                    {
                        var parentPlatformController = ((IPlatform)parent.Value).PlatformController;
                        var turbinePlatformController = ((IPlatform)device.Value).PlatformController;

                        QuickLogger.Debug($"Adding From Save Turbine {turbinePlatformController.GetUnitID()} with connection to {parentPlatformController.GetUnitID()} on port {turbine.Item1}");

                        AddPlatformFromSave(turbine.Item1, parentPlatformController, turbinePlatformController, turbine.Item4);

                        if(_connectedTurbines.Contains(turbine))
                            _savedData.CurrentConnections.Remove(turbine);
                    }
                    else
                    {
                        QuickLogger.Error($"Failed to find device with ID: {turbine.Item3}");
                    }
                }

                yield return null;
            }

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

        public void Save(SaveData newSaveData, ProtobufSerializer serializer = null)
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
            Grid = new Grid2<HoloGraphControl>((Capacity + 1) * 2);
        }

        public void AddPlatform(HolographSlot slot, Vector2Int position)
        {
            if (Grid.Count() > Capacity || Grid.ElementAt(position) != null) { return; } // Grid.ElementAt(position) != null tells us if the "slot" is filled
            
            QuickLogger.Debug($"Adding Turbine to Port: {slot.Target.name}|{slot.ID}");
            
            var turbine = slot.PlatformController.AddNewPlatForm(slot.Target, slot.ID, CraftData.GetPrefabForTechType(Mod.WindSurferClassName.ToTechType()));
            if (turbine == null) { return; }
            
            var turbineController = turbine.GetComponent<WindSurferController>();
            
            slot.PlatformController.ConnectionData = new Tuple<int, string,string, Vector2Int>(slot.ID,slot.Target.GetComponentInParent<FcsDevice>().UnitID, turbineController.GetUnitID(), position);
            
            var holo = AddNewHolograph(slot, turbine);
            holo.gameObject.name = $"{position} {holo.gameObject.name}";
            
            turbine.name = $"{position} {turbine.name}";
            
            Grid.Add(holo, position);

            _connectedTurbines.Add(slot.PlatformController.ConnectionData);
        }

        public void AddPlatformFromSave(int slotID,PlatformController parentPlatform, PlatformController turbine, Vector2Int position)
        {
            if (Grid.Count() > Capacity || Grid.ElementAt(position) != null) { return; } // Grid.ElementAt(position) != null tells us if the "slot" is filled
            
            if (turbine == null) { return; }
            
            var turbineController = turbine.GetComponent<WindSurferController>();

            turbine.ConnectionData = new Tuple<int, string,string, Vector2Int>(slotID,parentPlatform.GetUnitID(), turbineController.GetUnitID(), position);

            MoveTurbineToPosition(parentPlatform.transform,turbine.transform);

            var port = FindHoloPort(parentPlatform.GetUnitID(), slotID);

            if (port == null)
            {
                QuickLogger.Error("Failed to find hologram port");
                return;
            }

            var holo = AddNewHolograph(port, turbine);
            holo.gameObject.name = $"{position} {holo.gameObject.name}";
            turbine.name = $"{position} {turbine.name}";
            
            Grid.Add(holo, position);
            
            _connectedTurbines.Add(turbine.ConnectionData);
        }

        private void MoveTurbineToPosition(Transform parentPlatformTransform, Transform turbineTransform)
        {
            EnsureIsKinematic();
            turbineTransform.SetParent(Turbines.transform, true);
            var turbineController = turbineTransform.GetComponent<WindSurferController>();
            turbineController.TryMoveToPosition();
            turbineController.PoleState(true);
        }

        //private void MoveTurbineToPort(Transform parentPlatformTransform, Transform turbineTransform)
        //{
        //    EnsureIsKinematic();
        //    turbineTransform.position = Vector3.zero;
        //    turbineTransform.SetParent(parentPlatformTransform);
        //    turbineTransform.localPosition = new Vector3(11.10f, 0f, 0f);
        //    turbineTransform.SetParent(Turbines.transform, true);
        //    turbineTransform.localRotation = Quaternion.identity;
        //    var turbineController = turbineTransform.GetComponent<WindSurferController>();
        //    turbineController.PoleState(true);
        //}

        private Transform FindHoloPort(string holoPortUnitId,int slotID)
        {
            foreach (HoloGraphControl holoGraphControl in _holograms)
            {
                if (holoGraphControl.PlatFormController.GetUnitID().Equals(holoPortUnitId))
                {
                    holoGraphControl.FindSlots();
                    var trans = holoGraphControl.Slots.FirstOrDefault(x=>x.ID==slotID)?.transform;
                    return trans;
                }
            }

            return null;
        }


        public void RemovePlatform(PlatformController controller)
        {
            var holoGraphControl = controller.HoloGraphControl;

            var canRemove = CanRemovePlatform(holoGraphControl);

            if (canRemove)
            {
                var turbineController = controller.gameObject.GetComponentInChildren<WindSurferController>();
                QuickLogger.Debug($"Removing Turbine: {turbineController.GetUnitID()}", true);
                _connectedTurbines.Remove(controller.ConnectionData);
                Grid.Remove(holoGraphControl);
                Destroy(controller.gameObject);
                Destroy(holoGraphControl.gameObject);
            }
        }

        internal bool CanRemovePlatform(HoloGraphControl holoGraphControl)
        {
            QuickLogger.Debug("1");
            Graph<HoloGraphControl> G = Grid.ToGraph();
            QuickLogger.Debug("2");
            QuickLogger.Debug($"Grid{G} | HoloGraphControl {holoGraphControl} | Get Vertix{G.GetVertex(holoGraphControl)}");
            var neighbours = G.GetVertex(holoGraphControl).Neighbours;
            QuickLogger.Debug("3");
            G.RemoveVertex(holoGraphControl);
            QuickLogger.Debug("4");
            bool canRemove = true;
            QuickLogger.Debug("5");
            foreach (var neighbour in neighbours.Select(n => n.To))
            {
                QuickLogger.Debug("5.1");
                if (!G.MST(neighbour).Contains(Grid.ElementAt(Grid.Center)))
                {
                    canRemove = false;
                    break;
                }
                QuickLogger.Debug("5.2");
            }
            QuickLogger.Debug("6"); 
            return canRemove;
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
            holoGraphControl.SetAsTurbine();
            platformController.HoloGraphControl = holoGraphControl;
            holoGraphControl.transform.SetParent(slot.transform);
            holoGraphControl.transform.localPosition = new Vector3(123.92f, 0f, 0f);
            holoGraphControl.transform.SetParent(HologramsGrid.transform, true);
            holoGraphControl.transform.localScale = new Vector3(0.2f, 0.2f, 0.2f);
            holoGraphControl.transform.localRotation = Quaternion.identity;
            return holoGraphControl;
        }

        public HoloGraphControl AddNewHolograph(Transform slot, PlatformController platformController)
        {
            var holoGraphControl = CreateHoloGraph(platformController);
            holoGraphControl.SetAsTurbine();
            platformController.HoloGraphControl = holoGraphControl;
            holoGraphControl.transform.SetParent(slot);
            holoGraphControl.transform.localPosition = new Vector3(123.92f, 0f, 0f);
            holoGraphControl.transform.SetParent(HologramsGrid.transform, true);
            holoGraphControl.transform.localScale = new Vector3(0.2f, 0.2f, 0.2f);
            holoGraphControl.transform.localRotation = Quaternion.identity;
            return holoGraphControl;
        }

        public void EnsureIsKinematic()
        {
            if (_rb == null) return;
            _rb.isKinematic = true;
        }
        
    }

    internal interface IPlatform
    {
        PlatformController PlatformController { get;}
    }

    public static class UIExtensions
    {
        // Shared array used to receive result of RectTransform.GetWorldCorners
        static Vector3[] corners = new Vector3[4];

        /// <summary>
        /// Transform the bounds of the current rect transform to the space of another transform.
        /// </summary>
        /// <param name="source">The rect to transform</param>
        /// <param name="target">The target space to transform to</param>
        /// <returns>The transformed bounds</returns>
        public static Bounds TransformBoundsTo(this RectTransform source, Transform target)
        {
            // Based on code in ScrollRect's internal GetBounds and InternalGetBounds methods
            var bounds = new Bounds();
            if (source != null)
            {
                source.GetWorldCorners(corners);

                var vMin = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
                var vMax = new Vector3(float.MinValue, float.MinValue, float.MinValue);

                var matrix = target.worldToLocalMatrix;
                for (int j = 0; j < 4; j++)
                {
                    Vector3 v = matrix.MultiplyPoint3x4(corners[j]);
                    vMin = Vector3.Min(v, vMin);
                    vMax = Vector3.Max(v, vMax);
                }

                bounds = new Bounds(vMin, Vector3.zero);
                bounds.Encapsulate(vMax);
            }
            return bounds;
        }

        /// <summary>
        /// Normalize a distance to be used in verticalNormalizedPosition or horizontalNormalizedPosition.
        /// </summary>
        /// <param name="axis">Scroll axis, 0 = horizontal, 1 = vertical</param>
        /// <param name="distance">The distance in the scroll rect's view's coordiante space</param>
        /// <returns>The normalized scoll distance</returns>
        public static float NormalizeScrollDistance(this ScrollRect scrollRect, int axis, float distance)
        {
            // Based on code in ScrollRect's internal SetNormalizedPosition method
            var viewport = scrollRect.viewport;
            var viewRect = viewport != null ? viewport : scrollRect.GetComponent<RectTransform>();
            var viewBounds = new Bounds(viewRect.rect.center, viewRect.rect.size);

            var content = scrollRect.content;
            var contentBounds = content != null ? content.TransformBoundsTo(viewRect) : new Bounds();

            var hiddenLength = contentBounds.size[axis] - viewBounds.size[axis];
            return distance / hiddenLength;
        }

        /// <summary>
        /// Scroll the target element to the vertical center of the scroll rect's viewport.
        /// Assumes the target element is part of the scroll rect's contents.
        /// </summary>
        /// <param name="scrollRect">Scroll rect to scroll</param>
        /// <param name="target">Element of the scroll rect's content to center vertically</param>
        public static void ScrollToCenter(this ScrollRect scrollRect, RectTransform target)
        {
            // The scroll rect's view's space is used to calculate scroll position
            var view = scrollRect.viewport != null ? scrollRect.viewport : scrollRect.GetComponent<RectTransform>();

            // Calcualte the scroll offset in the view's space
            var viewRect = view.rect;
            var elementBounds = target.TransformBoundsTo(view);
            var offset = viewRect.center.y - elementBounds.center.y;

            // Normalize and apply the calculated offset
            var scrollPos = scrollRect.verticalNormalizedPosition - scrollRect.NormalizeScrollDistance(1, offset);
            scrollRect.verticalNormalizedPosition = Mathf.Clamp(scrollPos, 0f, 1f);

            var offset2 = viewRect.center.x - elementBounds.center.x;
            var scrollPos2 = scrollRect.horizontalNormalizedPosition - scrollRect.NormalizeScrollDistance(0, offset2);
            scrollRect.horizontalNormalizedPosition = Mathf.Clamp(scrollPos2, 0, 1);
        }

        public static void ScrollToCenter(this ScrollRect scrollRect, RectTransform target, RectTransform.Axis axis = RectTransform.Axis.Vertical)
        {
            // The scroll rect's view's space is used to calculate scroll position
            var view = scrollRect.viewport ?? scrollRect.GetComponent<RectTransform>();

            // Calcualte the scroll offset in the view's space
            var viewRect = view.rect;
            var elementBounds = target.TransformBoundsTo(view);

            // Normalize and apply the calculated offset
            if (axis == RectTransform.Axis.Vertical)
            {
                var offset = viewRect.center.y - elementBounds.center.y;
                var scrollPos = scrollRect.verticalNormalizedPosition - scrollRect.NormalizeScrollDistance(1, offset);
                scrollRect.verticalNormalizedPosition = Mathf.Clamp(scrollPos, 0, 1);
            }
            else
            {
                var offset = viewRect.center.x - elementBounds.center.x;
                var scrollPos = scrollRect.horizontalNormalizedPosition - scrollRect.NormalizeScrollDistance(0, offset);
                scrollRect.horizontalNormalizedPosition = Mathf.Clamp(scrollPos, 0, 1);
            }
        }
    }
}
