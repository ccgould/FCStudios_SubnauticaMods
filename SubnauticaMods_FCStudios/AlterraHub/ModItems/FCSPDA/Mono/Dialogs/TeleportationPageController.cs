using System.Collections.Generic;
using FCS_AlterraHub.Core.Helpers;
using FCS_AlterraHub.Core.Services;
using FCS_AlterraHub.ModItems.FCSPDA.Enums;
using FCSCommon.Utilities;
using UnityEngine;
using UnityEngine.UI;

namespace FCS_AlterraHub.ModItems.FCSPDA.Mono.Dialogs;

internal class TeleportationPageController : MonoBehaviour
{
    private GameObject _grid;
    private List<ShipmentTracker> _trackedShipments = new();

    public void Initialize(FCSAlterraHubGUI controller)
    {
        var backButton = gameObject.FindChild("BackBTN").GetComponent<Button>();
        backButton.onClick.AddListener(() =>
        {
            controller.GoToPage(PDAPages.Home);
        });

        _grid = GameObjectHelpers.FindGameObject(gameObject, "Content");
    }

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
}