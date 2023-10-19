using FCS_AlterraHub.Core.Components;
using FCS_StorageSolutions.Models;
using FCS_StorageSolutions.Services;
using FCSCommon.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


namespace FCS_StorageSolutions.ModItems.Buildables.DataStorageSolutions.Mono.C40Terminal.Mono;
internal class uGUI_TerminalBaseList : MonoBehaviour
{
    private GridHelper _gridHelper;
    private PaginatorController _paginatorController;
    [SerializeField] private List<uGUI_TerminalBaseListItem> listItems;
    [SerializeField] private DSSTerminalController terminalController;

    private void Awake()
    {
        _gridHelper = gameObject.GetComponent<GridHelper>();
        _gridHelper.OnLoadDisplay += GridHelper_OnLoadDisplay;
        _paginatorController = gameObject.GetComponentInChildren<PaginatorController>();
    }

    private void RefreshGrid()
    {
        _gridHelper.DrawPage();
    }

    private void GridHelper_OnLoadDisplay(DisplayData data)
    {
        try
        {

            if (DSSService.main is not null)
            {
                var grouped = DSSService.main.GetManagers();

                if (grouped is null) return;

                if (data.EndPosition > grouped.Count)
                {
                    data.EndPosition = grouped.Count;
                }

                for (int i = 0; i < data.MaxPerPage; i++)
                {
                    listItems[i].Reset();
                }

                int w = 0;

                for (int i = data.StartPosition; i < data.EndPosition; i++)
                {
                    listItems[w++].Set(grouped.ElementAt(i), terminalController);
                }

                _gridHelper?.UpdaterPaginator(grouped.Count);

                if (_gridHelper is not null)
                {
                    _paginatorController?.ResetCount(_gridHelper.GetMaxPages());

                }
            }
        }
        catch (Exception e)
        {
            QuickLogger.Error("Error Caught");
            QuickLogger.Error($"Error Message: {e.Message}");
            QuickLogger.Error($"Error StackTrace: {e.StackTrace}");
        }
    }


    public void ToggleVisibility()
    {
        gameObject.SetActive(!isActiveAndEnabled);
        
        if( gameObject.activeInHierarchy )
        {
            RefreshGrid();
        }
    }
}
