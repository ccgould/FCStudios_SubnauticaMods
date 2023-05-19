using FCS_AlterraHub.Core.Services;
using FCS_AlterraHub.ModItems.FCSPDA.Data.Models;
using FCS_AlterraHub.ModItems.FCSPDA.Enums;
using FCSCommon.Utilities;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace FCS_AlterraHub.ModItems.FCSPDA.Mono.Dialogs;

internal class EncyclopediaListItem : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField]
    private TMP_Text _label;
    private Button _button;
    private EncyclopediaData _encyclopediaData;
    private EncyclopediaMainTabController _controller;
    private EncyclopediaEntryData _entryData;

    internal void Initialize(EncyclopediaMainTabController encyclopediaMainTabController, EncyclopediaData value)
    {
        if(_label != null)
        {
            _label.text = value.Title;
        }
        _encyclopediaData = value;
        _controller = encyclopediaMainTabController;
    }

    internal void Initialize(EncyclopediaEntryData value)
    {
        if (_label != null)
        {
            _label.text = value.Title;
        }
        _entryData = value;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        //TODO decouple
        if (_controller == null) return;
        _controller.HoverTriggered(_encyclopediaData);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (_controller == null) return;
        _controller.Clear();
    }

    public EncyclopediaEntryData GetData()
    {
        return _entryData;
    }

    public void onClick()
    {
        QuickLogger.Debug("InOnClick");
        if (FCSPDAController.Main.GetGUI().GetCurrentPage() == PDAPages.Encyclopedia)
        {
            QuickLogger.Debug($"Passing Data: {_entryData.Path}");
            EncyclopediaService.GetEncyclopediaTabController().SetData(_entryData);
            QuickLogger.Debug("DataPassed");

        }
        else
        {
            QuickLogger.Debug("Current Page isnt Encycopedia. Going to Encyclopedia page and passing data.");

            FCSPDAController.Main.GetGUI().GoToPage(PDAPages.Encyclopedia, _encyclopediaData);

            QuickLogger.Debug("Passing Data Complete");

        }
    }
}