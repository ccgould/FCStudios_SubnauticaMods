using FCS_AlterraHub.Core.Navigation;
using FCS_AlterraHub.Models.Structs;
using FCS_ProductionSolutions.ModItems.Buildables.DeepDrillers.Mono.Base.Interface;
using FCS_ProductionSolutions.ModItems.Buildables.DeepDrillers.Mono.Heavy.Enumerators;
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace FCS_ProductionSolutions.ModItems.Buildables.DeepDrillers.Mono.Heavy.UGUI;
public class uGUI_DDHoloGraphControl : MonoBehaviour
{
    [SerializeField] private List<DDHolographSlot> _slots = new(4);
    [SerializeField] private Image _drill;
    [SerializeField] private Image _operator;
    [SerializeField] private Button _deleteBTN;
    private GameObject _deleteBtnOBJ;
    [SerializeField] private TextMeshProUGUI _unitID;
    [SerializeField] private TextMeshProUGUI _oilInfo;
    public string parent { get; set; }
    public Vec2 postion { get; set; }


    private DDPlatformController platformController;


    private void Update()
    {
        //if (_deleteBTN != null && _deleteBtnOBJ != null && WindSurferOperatorController != null)
        //{
        //    _deleteBTN.interactable = DeepDrillerOperatorController.ScreenTrigger.selected;
        //}
    }

    internal void Set(DDPlatformController platFormController)
    {
        this.platformController = platFormController;

        //if (_drill == null)
        //{
        //    _drill = gameObject.FindChild("Drill");
        //    _drill.SetActive(false);
        //}

        //if (_operator == null)
        //{
        //    _operator = gameObject.FindChild("Operator");
        //}

        //if (_platform == null)
        //{
        //    _platform = gameObject.FindChild("Platform");
        //}


        //if (_unitID == null)
        //{
        //    _unitID = gameObject.FindChild("UnitID").GetComponent<Text>();
        //}

        //if (_powerInfo == null)
        //{
        //    _powerInfo = _drill.FindChild("PowerInfo").GetComponent<Text>();
        //}

        //if (_deleteBTN == null)
        //{
        //    _deleteBtnOBJ = gameObject.FindChild("DeleteBTN");
        //    _deleteBTN = _deleteBtnOBJ.GetComponent<Button>();
        //    _deleteBTN.onClick.AddListener((() =>
        //    {
        //        var result = WindSurferOperatorController.TryRemovePlatform(PlatFormController);
        //        if (result)
        //        {
        //            WindSurferOperatorController.RefreshHoloGrams();
        //        }

        //    }));
        //}

        //FindSlots();

        InvokeRepeating(nameof(UpdatePowerInfo), 1f, 1f);
    }

    private void UpdatePowerInfo()
    {
        if (_oilInfo != null)
        {
            _oilInfo.text = platformController?.GetMountedDevice().GetOilLevel() ?? "0/0";
        }

        if (_unitID != null)
        {
            UpdateUnitId();
        }
    }

    public void RefreshDeleteButton(bool value)
    {
        _deleteBtnOBJ.SetActive(value);
    }


    internal void SetIcon(HolographIconType type)
    {
        _operator.gameObject.SetActive(false);
        _drill.gameObject.SetActive(false);

        switch (type)
        {
            case HolographIconType.Operator:
                _operator.gameObject.SetActive(true);
                break;
            case HolographIconType.Drill:
                _drill.gameObject.SetActive(true);
                break;
        }
    }

    internal DDHolographSlot FindSlotWithId(int id)
    {
        return _slots.FirstOrDefault(x => x.GetSlotID() == id);
    }

    public void UpdateUnitId()
    {
        if (_unitID == null) return;
        _unitID.text = GetUnitID();
    }

    internal void MoveIntoPosition(Transform HologramsGrid, uGUI_DDHoloGraphControl parent,int parentSlot)
    {
        var slot = parent.FindSlotWithId(parentSlot);
        transform.SetParent(slot.transform);
        transform.localPosition = Vector3.zero;
        transform.localPosition = new Vector3(75f, 0f, 0f);
        transform.SetParent(HologramsGrid, true);
        transform.localScale = new Vector3(.5f, .5f, .5f);
        transform.localRotation = Quaternion.identity;
    }

    internal List<DDHolographSlot> GetSlots()
    {
        return _slots;
    }

    internal string GetUnitID()
    {
        return platformController?.GetMountedDevice()?.UnitID;
    }

    internal string GetPrefabID()
    {
        return platformController?.GetMountedDevice()?.GetPrefabID();
    }
}