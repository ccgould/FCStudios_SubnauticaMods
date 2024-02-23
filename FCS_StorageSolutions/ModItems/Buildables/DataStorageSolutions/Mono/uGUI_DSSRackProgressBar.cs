using FCS_AlterraHub.Core.Services;
using FCS_AlterraHub.Models.Mono;
using FCS_StorageSolutions.ModItems.Buildables.DataStorageSolutions.Mono.Base;
using UnityEngine;
using UnityEngine.UI;

namespace FCS_StorageSolutions.ModItems.Buildables.DataStorageSolutions.Mono;
internal class uGUI_DSSRackProgressBar : MonoBehaviour
{
    [SerializeField] private FCSStorage storage;
    [SerializeField] private Text storageLbl;
    [SerializeField] private Image preLoaderBar;
    [SerializeField] private RackBase rackBase;


    private void Start()
    {
        rackBase.GetStorage().ItemsContainer.onAddItem += RackBase_ContainerChanged;
        rackBase.GetStorage().ItemsContainer.onRemoveItem += RackBase_ContainerChanged;
    }

    private void OnDestroy()
    {
        rackBase.GetStorage().ItemsContainer.onAddItem -= RackBase_ContainerChanged;
        rackBase.GetStorage().ItemsContainer.onRemoveItem -= RackBase_ContainerChanged;
    }


    internal void Refresh(InventoryItem item = null)
    {
        RackBase_ContainerChanged(item);
    }

    private void RackBase_ContainerChanged(InventoryItem item)
    {
        storageLbl.text = rackBase.GetStorageAmountFormat();

        float percentage = (float)rackBase.CalculateStorageTotal() / rackBase.CalculateMaxStorage();

        preLoaderBar.fillAmount = percentage;
    }

}
