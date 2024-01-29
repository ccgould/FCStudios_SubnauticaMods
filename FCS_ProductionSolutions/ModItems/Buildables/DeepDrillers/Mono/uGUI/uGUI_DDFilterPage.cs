using FCS_AlterraHub.Core.Components;
using FCS_AlterraHub.Core.Components.uGUIComponents;
using FCS_AlterraHub.Core.Navigation;
using FCS_ProductionSolutions.Configuration;
using FCS_ProductionSolutions.ModItems.Buildables.DeepDrillers.Mono.Base;
using FCSCommon.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace FCS_ProductionSolutions.ModItems.Buildables.DeepDrillers.Mono.uGUI;
internal class uGUI_DDFilterPage : Page
{
    [SerializeField] private Toggle _toggleFilterBTN;
    [SerializeField] private Toggle _toggleBlackListBTN;
    [SerializeField] private GridHelper _filterGrid;
    [SerializeField] private List<uGUI_StorageItem> _trackedFilterState;
    [SerializeField] private PaginatorController _paginatorController;
    [SerializeField] private Text _filterPageInformation;

    private DrillSystem _sender;

    public override void Awake()
    {
        base.Awake();

        _filterGrid.OnLoadDisplay += OnLoadFilterGrid;
        
        InvokeRepeating(nameof(UpdateInformation), 1, 0.3f);

        foreach (var item in _trackedFilterState)
        {
            var toggle = item.gameObject.GetComponent<Toggle>();
            toggle.onValueChanged.AddListener((v) =>
            {
                OnFilterButtonClick(v, item);
            });
        }
    }

    public void SetBlackListMode(bool value)
    {
        QuickLogger.Debug($"Deep Driller Blacklist Mode: {value}", true);


        _sender.GetOreGenerator().SetBlackListMode(value);
    }

    public void ToggleFilter(bool value)
    {
        QuickLogger.Debug($"Deep Driller Filter: {value}", true);

        _sender.GetOreGenerator().SetFocus(value);
    }

    private void UpdateInformation()
    {
        if (_filterPageInformation == null || _sender is null) return;
        _filterPageInformation.text = AuxPatchers.FilterPageInformation(_toggleFilterBTN.isOn, _toggleBlackListBTN.isOn, _sender.GetOreGenerator().FocusCount());
    }

    private void OnLoadFilterGrid(DisplayData data)
    {
        try
        {
            if(_sender is null || _sender.GetOreGenerator() is null) return;

            _sender.GetOreGenerator().GetAllowedOres((grouped) =>
            {
                if (data.EndPosition > grouped.Count)
                {
                    data.EndPosition = grouped.Count;
                }

                foreach (var displayItem in _trackedFilterState)
                {
                    displayItem.Reset();
                }

                int w = 0;

                for (int i = data.StartPosition; i < data.EndPosition; i++)
                {
                    _trackedFilterState[w++].Set(grouped.ElementAt(i));
                }


                _filterGrid?.UpdaterPaginator(grouped.Count);

                if (_filterGrid is not null)
                {
                    _paginatorController?.ResetCount(_filterGrid.GetMaxPages());

                }

                foreach (var displayItem in _trackedFilterState)
                {
                    var toggle = displayItem.gameObject.GetComponent<Toggle>();
                    toggle.SetIsOnWithoutNotify(_sender.GetOreGenerator().GetFocusedOres().Contains(displayItem.GetTechType()));
                }
            });
            
        }
        catch (Exception e)
        {
            QuickLogger.Error("Error Caught");
            QuickLogger.Error($"Error Message: {e.Message}");
            QuickLogger.Error($"Error StackTrace: {e.StackTrace}");
        }
    }

    private void OnFilterButtonClick(bool value, uGUI_StorageItem button)
    {
        QuickLogger.Debug($"Deep Driller Filter Item Clicked: {button.GetTechType()}",true);
        if (value)
        {
            _sender.GetOreGenerator().AddFocus(button.GetTechType());
        }
        else
        {
           _sender.GetOreGenerator().RemoveFocus(button.GetTechType());
        }
    }

    public override void Enter(object arg = null)
    {
        base.Enter(arg);

        if (arg is null) return;

        _sender = arg as DrillSystem;

        _toggleBlackListBTN.SetIsOnWithoutNotify(_sender.GetOreGenerator().GetInBlackListMode());
        _toggleFilterBTN.SetIsOnWithoutNotify(_sender.GetOreGenerator().GetIsFocused());

        _filterGrid.DrawPage();

        UpdateInformation();
    }

    public override void Exit()
    {
        base.Exit();
        _filterGrid?.DrawPage(0);
    }
}