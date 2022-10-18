using System.Collections.Generic;
using FCS_AlterraHub.Buildables;
using FCS_AlterraHub.Helpers;
using FCS_AlterraHub.Mods.FCSPDA.Enums;
using FCS_AlterraHub.Mods.FCSPDA.Mono;
using FCS_AlterraHub.Mono;
using FCSCommon.Utilities;
using UnityEngine;
using UnityEngine.UI;

namespace FCS_AlterraHub.Managers.FCSAlterraHub;

internal class TeleportationPageController : MonoBehaviour
{
    private GameObject _grid;
    private List<ShipmentTracker> _trackedShipments = new();

    public void Initialize(FCSPDAController controller)
    {
        var backButton = gameObject.FindChild("BackBTN").GetComponent<Button>();
        backButton.onClick.AddListener((() =>
        {
            controller.GoToPage(PDAPages.Home);
        }));

        _grid = GameObjectHelpers.FindGameObject(gameObject, "Content");
    }

    internal void Refresh()
    {
        QuickLogger.Debug("Refreshing Teleportation list",true);
        foreach (Transform child in _grid.transform)
        {
            Destroy(child.gameObject);
        }

        foreach (BaseManager baseManager in BaseManager.Managers)
        {
            if (baseManager.IsValidForTeleport())
            {
                //Instantiate Item
                var item = GameObject.Instantiate(AlterraHub.PDATeleportItemPrefab);
                var baseItem = item.AddComponent<BaseTeleportItem>();
                baseItem.Initialize(baseManager);
                //Move in scrollview
                item.transform.SetParent(_grid.transform, false);
            }
        }
    }
}