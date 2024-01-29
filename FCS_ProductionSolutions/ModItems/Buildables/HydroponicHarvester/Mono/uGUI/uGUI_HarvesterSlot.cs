using FCS_AlterraHub.API;
using FCS_AlterraHub.Core.Helpers;
using FCS_ProductionSolutions.ModItems.Buildables.HydroponicHarvester.Model;
using System;
using UnityEngine;
using UnityEngine.UI;
using static VehicleUpgradeConsoleInput;

namespace FCS_ProductionSolutions.ModItems.Buildables.HydroponicHarvester.Mono.uGUI;
internal class uGUI_HarvesterSlot : MonoBehaviour
{
    [SerializeField] private uGUI_Icon _icon;
    [SerializeField] private int id;
    [SerializeField] private Text _amount;
    [SerializeField] private Text _timeLeft;

    private PlantSlot _slot;
    private uGUI_HydroponicHarvester _ui;
    private HydroponicHarvesterController _controller;

    private void Awake()
    {
        _icon.sprite = SpriteManager.defaultSprite;
        _ui = gameObject.GetComponentInParent<uGUI_HydroponicHarvester>();
    }

    private void OnLongPress()
    {
        //_display.ShowMessage(AuxPatchers.ClearSlotLongPress(), FCSMessageButton.YESNO, (result =>
        //{
        //    if (result == FCSMessageResult.OKYES)
        //    {
        //        Slot.TryClear(true, true);
        //    }
        //}));
    }

    internal void SetIcon(TechType techType)
    {
        _icon.sprite = SpriteManager.Get(techType);
    }

    internal void UpdateCount()
    {
        _amount.text = $"{_slot?.GetCount() ?? 0}/{_slot?.GetMaxCapacity() ?? 50}";
    }

    internal void SetSlot(PlantSlot slot)
    {
        RefrestIcon(slot);
        _slot = slot;
        _slot.OnStatusUpDate += Refresh;
        UpdateCount();
    }

    private void RefrestIcon(PlantSlot slot)
    {
        if (slot.IsOccupied())
        {
            _icon.sprite = SpriteManager.Get(slot.GetPlantable()?.plantTechType ?? TechType.None);
        }
        else
        {
            _icon.sprite = SpriteManager.Get(TechType.None);
        }
    }

    internal void Purge()
    {
        if (_slot is null || _icon is null) return;
        _slot.OnStatusUpDate -= Refresh;
        _icon.sprite = SpriteManager.Get(TechType.None);
        UpdateCount();
        _controller = null;
        _slot = null;
    }

    private void Refresh()
    {
        if (_slot is null) return;
        RefrestIcon(_slot);
        UpdateCount();
    }

    public void GivePlayerSeed()
    {
        if (_slot is null)
        {
            FCSModsAPI.PublicAPI.ShowMessageInPDA(Language.main.Get("PS_SlotIsEmpty"));
            return;
        }

        if (!_slot.IsPlantFullyGrown())
        {
            FCSModsAPI.PublicAPI.ShowMessageInPDA(Language.main.Get("PS_PlantNotFullyGrown"));
            return;
        }


        if (_slot.GetReturnType() == _slot.GetSeedReturnType() || _slot.GetSeedReturnType() == TechType.None)
        {
            FCSModsAPI.PublicAPI.ShowMessageInPDA(Language.main.Get("PS_SameHarvesterOutput"));
            return;
        }

        if (!PlayerInteractionHelper.GivePlayerItem(_slot.GetSeedReturnType()))
        {
            FCSModsAPI.PublicAPI.ShowMessageInPDA(Language.main.Get("InventoryFull"));
        }
    }

    public void ClearSlot()
    {
        if(_slot is null || _slot.GetPlantable() is null) return;

        FCSModsAPI.PublicAPI.ShowMessageInPDA(Language.main.Get("PS_ClearSlotLongPress"),FCS_AlterraHub.ModItems.FCSPDA.Mono.uGUIComponents.FCSMessageButton.YESNO, (result) =>
        {
            if(result == FCS_AlterraHub.ModItems.FCSPDA.Mono.uGUIComponents.FCSMessageResult.OKYES)
            {
                GetController().ClearSlot(_slot.GetPlantable());
                Purge();
            }
        });
    }

    public void AddOrGetItem()
    {
        if(!_slot.IsOccupied())
        {
            GetController().Open(id);
            return;
        }

        if (_slot.GetReturnType() == TechType.None)
        {
            FCSModsAPI.PublicAPI.ShowMessageInPDA(Language.main.Get("PS_PlantNotFullyGrown"));
            return;
        }

        if(_slot.GetCount() <= 0)
        {
            FCSModsAPI.PublicAPI.ShowMessageInPDA(Language.main.Get("PS_NoItemsGenerated"));
            return;
        }

        if (_slot.GivePlayerItem())
        {
            UpdateCount();
        }
        else
        {
            FCSModsAPI.PublicAPI.ShowMessageInPDA(Language.main.Get("InventoryFull"));
        }
    }

    public HydroponicHarvesterController GetController()
    {
        if(_controller == null)
        {
            _controller = (HydroponicHarvesterController)_ui.GetController();
        }
        return _controller;
    }
}