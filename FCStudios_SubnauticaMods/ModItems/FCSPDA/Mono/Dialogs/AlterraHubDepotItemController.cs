using FCS_AlterraHub.Models.Mono;
using FCSCommon.Utilities;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace FCS_AlterraHub.Mods.FCSPDA.Mono.Dialogs;

internal class AlterraHubDepotItemController : MonoBehaviour
{
    internal PortManager Destination { get; set; }
    internal bool IsChecked => _toggle.isOn;

    [SerializeField] private Toggle _toggle;
    [SerializeField] private Text text;
    [SerializeField] private ToggleGroup _toggleGroup;

    internal bool Initialize(PortManager depot, ToggleGroup toggleGroup, Transform list)
    {
        try
        {
            if (depot?.Manager == null || toggleGroup == null || list == null) return false;


            Destination = depot;
            text.text = $"Name: {depot.Manager.GetBaseName()}\nStatus: N/A";
            _toggleGroup = toggleGroup;
            _toggle = gameObject.GetComponentInChildren<Toggle>();
            _toggle.group = toggleGroup;
            //if (depot.IsFull)
            //{
            //    _toggle.enabled = false;
            //    _toggle.isOn = false;
            //}
            gameObject.transform.localScale = Vector3.one;
            gameObject.transform.SetParent(list, false);
            return true;
        }
        catch (Exception e)
        {
            QuickLogger.Error(e.Message);
            QuickLogger.Error(e.StackTrace);
            return false;
        }
    }

    public void UnRegisterAndDestroy()
    {
        _toggleGroup.UnregisterToggle(_toggle);
        Destroy(gameObject);
    }

    internal void Refresh()
    {
        text.text = $"Name: {Destination.Manager.GetBaseName()}\nStatus: N/A";

    }
}