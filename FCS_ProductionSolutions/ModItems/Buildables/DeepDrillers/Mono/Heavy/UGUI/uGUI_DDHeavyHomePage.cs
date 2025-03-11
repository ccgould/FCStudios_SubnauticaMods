using FCS_AlterraHub.Core.Helpers;
using FCS_AlterraHub.Core.Navigation;
using FCS_AlterraHub.Models.Abstract;
using FCS_AlterraHub.Models.Interfaces;
using FCS_AlterraHub.Models.Structs;
using FCS_AlterraHub.ModItems.FCSPDA.Interfaces;
using FCS_ProductionSolutions.ModItems.Buildables.DeepDrillers.Mono.Heavy.Struct;
using FCS_ProductionSolutions.ModItems.Buildables.DeepDrillers.Spawnables;
using FCSCommon.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;
using static HandReticle;
using static VehicleUpgradeConsoleInput;


namespace FCS_ProductionSolutions.ModItems.Buildables.DeepDrillers.Mono.Heavy.UGUI;
internal class uGUI_DDHeavyHomePage : Page, IuGUIAdditionalPage
{
    //[SerializeField] private Text drillCountLbl;
    //[SerializeField] private Text storageCountLbl;
    [SerializeField] private GameObject HologramsGrid;
    [SerializeField] private GameObject holoGramPrefab;
    [SerializeField] private Text? drillCounter;
    [SerializeField] private Text? storageCounter;

    private List<uGUI_DDHoloGraphControl> _holograms = new();

    private DeepDrillerOperatorController _sender;

    public override void Enter(object arg = null)
    {
        base.Enter(arg);

        QuickLogger.Debug("Enter Home", true);

        if (_sender is null)
        {
            _sender = arg as DeepDrillerOperatorController;
        }

        if (arg is not null || _sender is not null)
        {
            QuickLogger.Debug($"Operator controller is null {_sender is not null}", true);

            _sender.OnStorageCountUpdated += OnStorageCountUpdated;

            AddMainHolograph();

            foreach (var drill in _sender.GetConnectedDrills())
            {
                if (_sender.GetItemWithId(drill.Key, out GameObject result))
                {
                    var platformController = result.GetComponentInChildren<DDPlatformController>();
                    var holoGraphControl = CreateHoloGraph(platformController);
                    holoGraphControl.transform.SetParent(HologramsGrid.transform, true);
                    holoGraphControl.transform.localPosition = Vector3.zero;
                    holoGraphControl.transform.localRotation = Quaternion.identity;
                    holoGraphControl.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
                    holoGraphControl.gameObject.name = $"{drill.Value.HoloGraphPosition} {holoGraphControl.gameObject.name}";
                    _sender.Grid.Add(holoGraphControl, drill.Value.HoloGraphPosition);

                    QuickLogger.Debug($"Added Drill to slot {drill.Value.Slot}");
                }
                else
                {
                    QuickLogger.DebugError($"No matching drill found for id: {drill.Key}");

                }
            }


            MoveHologramsInPosition();
            OnStorageCountUpdated(_sender.GetDDContainerTotal());
            UpdateDrillCount();
        }
    }

    private void MoveHologramsInPosition()
    {
        foreach (var drill in _sender.GetConnectedDrills())
        {
            var parentHologram = findHoloWithID(drill.Value.ParentTurbineUnitID);
            var currentHologram = findHoloWithID(drill.Key);

            if(parentHologram is null || currentHologram is null)
            {
                QuickLogger.DebugError($"Failed to move hologram: Current is null: {currentHologram is null} | Parent is null {parentHologram is null}");
                return;
            }

            currentHologram.MoveIntoPosition(HologramsGrid.transform,parentHologram, drill.Value.Slot);
        }
    }

    private uGUI_DDHoloGraphControl findHoloWithID(string parentTurbineUnitID)
    {
        return _holograms.FirstOrDefault(x=>x.GetPrefabID() == parentTurbineUnitID);
    }

    private void AddMainHolograph()
    {
        QuickLogger.Debug("Adding holograph", true);
        var holo = CreateMainHolograph();
        holo.gameObject.name = $"{_sender.Grid.Center} {holo.gameObject.name}";
        _sender.Grid.Add(holo, _sender.Grid.Center);
    }

    private void Purge()
    {
        for (int i = _holograms.Count - 1; i >= 0; i--)
        {
            _sender.Grid.Remove(_holograms[i]);
            Destroy(_holograms[i].gameObject);
        }

        _holograms.Clear();
        
    }

    private void OnStorageCountUpdated(int amount)
    {
        UpdateStorageCount(amount);
    }

    internal void UpdateStorageCount(int total)
    {
        if (storageCounter is not null && _sender is not null)
        {
            storageCounter.text = $"{total} / 200";
        }
    }

    private void UpdateDrillCount()
    {
        if (drillCounter is not null)
        {
            drillCounter.text = $"{_holograms.Count - 1} / 15";
        }
    }

    public override void Exit()
    {
        base.Exit();

        _sender.OnStorageCountUpdated -= OnStorageCountUpdated;
        Purge();
        _sender = null;
    }

    public  IEnumerator AddPlatform(DDHolographSlot slot, Vector2Int position, IOut<bool> result)
    {
        QuickLogger.Debug("=================================== Add Platform ===============================================");

        if (_sender.GetConnectedDrills().Count == _sender.BuildingCapacity())
        {
            //_messageBox.Show(AuxPatchers.WindSurferMaxReached(), FCSMessageButton.OK, null);
            result.Set(false);
            yield break;
        }

        QuickLogger.Debug("1");

        //!!!!!!! Re-enable !!!!!!!

        //if (!CheckForKit())
        //{
        //    //var kit = PlatFormKitMode == PlatformKitModes.TurbinePlatform
        //    //    ? Language.main.Get(_windSurferKitTechType)
        //    //    : Language.main.Get(_windSurferPlatformKitTechType);
        //    //_messageBox.Show($"No {kit} found in your inventory.", FCSMessageButton.OK, null);
        //    result.Set(false);
        //    yield break;
        //}

        //var kitPickupable = Inventory.main.container.RemoveItem(DeepDrillerSpawnable.PatchedTechType);

        //Destroy(kitPickupable.gameObject);

        if (_sender.Grid.Count() > _sender.BuildingCapacity() || _sender.Grid.ElementAt(position) != null)
        {
            result.Set(false);
            yield break;

        } // Grid.ElementAt(position) != null tells us if the "slot" is filled

        QuickLogger.Debug("2");

        QuickLogger.Debug($"Adding Drill to Port: {slot.Target.name}|{slot.GetSlotID()}");

        var go = CraftData.GetPrefabForTechTypeAsync(DeepDrillerHeavyDutySpawnable.PatchedTechType, false);

        yield return go;

        var drill = GameObject.Instantiate(go.GetResult());

        if (drill != null)
        {
            slot.GetPlatformController().AddNewPlatForm(slot.Target, _sender, drill);

            if (drill == null)
            {
                result.Set(false);
                yield break;
            }

            foreach (Collider builtCollider in drill.GetComponentsInChildren<Collider>() ?? new Collider[0])
            {
                foreach (Collider collider in gameObject.GetComponentsInChildren<Collider>() ?? new Collider[0])
                {
                    Physics.IgnoreCollision(collider, builtCollider);
                }
            }


            var drillController = drill.GetComponent<DDPlatformController>();

            var device = drill.GetComponentInParent<FCSDevice>();

            slot.GetPlatformController().ConnectionData = new ConnectedDrillData
            {
                Slot = slot.GetSlotID(),
                ParentTurbineUnitID = slot.Target.GetComponentInParent<FCSDevice>().GetPrefabID(),
                HoloGraphPosition = position,
                Position = new FCS_AlterraHub.Models.Structs.Vec3(drill.transform.localPosition),
                /*UnitID = device.GetPrefabID()*/
            };

            _sender.Grid.Add(AddNewHolograph(slot, drill, position), position);

            drill.name = $"{position} {drill.name}";
            _sender.GetConnectedDrills().Add(drillController.GetMountedDevice().GetPrefabID(), slot.GetPlatformController().ConnectionData);
            _sender.AddPlatformBase(drillController.GetMountedDevice().GetPrefabID(), drillController);
            result.Set(true);

            UpdateDrillCount();

            QuickLogger.Debug("=================================== Add Platform ===============================================");
            yield break;
        }

            // Fetch the prefab:
         //   CoroutineTask<GameObject> task = CraftData.GetPrefabForTechTypeAsync(DeepDrillerHeavyDutySpawnable.PatchedTechType);
        // Wait for the prefab task to complete:
       // yield return task;
        // Get the prefab:
      //  GameObject drill = task.GetResult();

        
    }

    private bool CheckForKit()
    {
        return PlayerInteractionHelper.HasItem(TechType.None);
    }

    private uGUI_DDHoloGraphControl CreateMainHolograph()
    {
        var platformController = _sender.GetComponent<DDPlatformController>();
        var holoGraphControl = CreateHoloGraph(platformController);
        holoGraphControl.transform.SetParent(HologramsGrid.transform, true);
        holoGraphControl.transform.localPosition = Vector3.zero;
        holoGraphControl.transform.localRotation = Quaternion.identity;
        holoGraphControl.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
        holoGraphControl.gameObject.name = $"{_sender.Grid.Center} {holoGraphControl.gameObject.name}";
        _sender.Grid.Add(holoGraphControl, _sender.Grid.Center);
        return holoGraphControl;
    }

    private uGUI_DDHoloGraphControl CreateHoloGraph(DDPlatformController controller)
    {
        var holoGraph = Instantiate(holoGramPrefab);

        var holoGraphControl = holoGraph.GetComponent<uGUI_DDHoloGraphControl>();
        holoGraphControl.Set(controller);

        foreach (var item in holoGraphControl.GetSlots().Select((slot, index) => new { slot, index }))
        {
            item.slot.Target = controller.Ports[item.index].transform;
            item.slot.Set(this,_sender, controller);
        }

        _holograms.Add(holoGraphControl);

        return holoGraphControl;
    }

    public uGUI_DDHoloGraphControl AddNewHolograph(DDHolographSlot slot, GameObject go,Vector2Int position)
    {
        QuickLogger.Debug($"Slot: {slot} | GameObject: {go}");
        var platformController = go.GetComponent<DDPlatformController>();
        var holoGraphControl = CreateHoloGraph(platformController);
        holoGraphControl.SetIcon(platformController.GetPlatformType());
        holoGraphControl.transform.SetParent(slot.transform);
        holoGraphControl.transform.localPosition = new Vector3(75f, 0f, 0f);
        holoGraphControl.transform.SetParent(HologramsGrid.transform, true);
        holoGraphControl.transform.localScale = new Vector3(.5f, .5f, .5f);
        holoGraphControl.transform.localRotation = Quaternion.identity;
        holoGraphControl.postion = new Vec2(position.x, position.y);
        holoGraphControl.gameObject.name = $"{position} {holoGraphControl.gameObject.name}";
        return holoGraphControl;
    }


    public uGUI_DDHoloGraphControl AddNewHolograph(Transform slot, DDPlatformController platformController, Vector2Int position)
    {
        var holoGraphControl = CreateHoloGraph(platformController);
        holoGraphControl.postion = new Vec2(position.x, position.y);
        holoGraphControl.gameObject.name = $"{position} {holoGraphControl.gameObject.name}";
        holoGraphControl.SetIcon(platformController.GetPlatformType());
        holoGraphControl.transform.SetParent(slot);
        holoGraphControl.transform.localPosition = new Vector3(123.92f, 0f, 0f);
        holoGraphControl.transform.SetParent(HologramsGrid.transform, true);
        holoGraphControl.transform.localScale = new Vector3(0.2f, 0.2f, 0.2f);
        holoGraphControl.transform.localRotation = Quaternion.identity;
        _sender.Grid.Add(holoGraphControl, position);
        return holoGraphControl;
    }

    public IFCSObject GetController()
    {
        return _sender;
    }

    private float currentScale = 2;
    private float maxScale = 4f;
    private float minScale = 1f;

    void Update()
    {
        //Zoom(Input.GetAxis("Mouse ScrollWheel"));
    }


    public void ZoomIn()
    {
        Zoom(0.1f);
    }

    public void ZoomOut()
    {
        Zoom(-0.1f);
    }

    void Zoom(float increment)
    {
        currentScale += increment;
        if (currentScale >= maxScale)
        {
            currentScale = maxScale;
        }
        else if (currentScale <= minScale)
        {
            currentScale = minScale;
        }
        HologramsGrid.GetComponent<RectTransform>().localScale = new Vector2(currentScale, currentScale);
    }
}
