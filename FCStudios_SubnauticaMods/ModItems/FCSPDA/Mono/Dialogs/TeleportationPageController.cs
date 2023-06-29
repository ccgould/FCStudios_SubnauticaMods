using System.Collections.Generic;
using FCS_AlterraHub.Core.Navigation;
using FCSCommon.Utilities;
using UnityEngine;

namespace FCS_AlterraHub.ModItems.FCSPDA.Mono.Dialogs;

internal class TeleportationPageController : Page
{
    [SerializeField]
    private GameObject _grid;
    private List<ShipmentTracker> _trackedShipments = new();

    internal void Refresh()
    {
        QuickLogger.Debug("Refreshing Teleportation list", true);
        foreach (Transform child in _grid.transform)
        {
            Destroy(child.gameObject);
        }

        //TODO change into a observer Pattern
        //foreach (BaseManager baseManager in BaseManager.Managers)
        //{
        //    if (baseManager.IsValidForTeleport())
        //    {
        //        //Instantiate Item
        //        var item = GameObject.Instantiate(ModPrefabService.GetPrefab("PDATeleportItem"));
        //        var baseItem = item.AddComponent<BaseTeleportItem>();
        //        baseItem.Initialize(baseManager);
        //        //Move in scrollview
        //        item.transform.SetParent(_grid.transform, false);
        //    }
        //}
    }

    public override void Enter(object arg = null)
    {
        base.Enter();
        Refresh();
    }
}