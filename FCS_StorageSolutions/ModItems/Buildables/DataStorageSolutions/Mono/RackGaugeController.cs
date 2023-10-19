using FCS_AlterraHub.Core.Helpers;
using FCS_StorageSolutions.ModItems.Buildables.DataStorageSolutions.Mono.Base;
using FCSCommon.Utilities;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace FCS_StorageSolutions.ModItems.Buildables.DataStorageSolutions.Mono;
public class RackGaugeController : MonoBehaviour, IPointerHoverHandler, IPointerExitHandler,IRackGaugeListener
{
    private RackBase baseRack;
    [SerializeField] private GameObject buttonsGroup;
    [SerializeField] private Image progressBar;
    [SerializeField] private Text progressLBL;
    [SerializeField] private Text slotNumber;
    private RackBase rackBase;
    private int index = -1;
    private DSSServerController server;



    public void OnEjectButtonPressed()
    {
        var isRemoved = rackBase.ClearSlotAndReturnServer(index);

        if(isRemoved)
        {
            PlayerInteractionHelper.GivePlayerItem(isRemoved);

            Eject();
        }
    }

    public void Eject()
    {
        server.UnSubscribe(this);
        server = null;
        UpdateValues();
        buttonsGroup?.SetActive(false);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        buttonsGroup?.SetActive(false);
    }

    public void OnPointerHover(PointerEventData eventData)
    {
        if(server is not null)
        {
            buttonsGroup?.SetActive(true);
        }
    }

    public void OnSettingsButtonPressed()
    {

    }

    internal void Initalize(int index,RackBase rackBase)
    {
        this.rackBase = rackBase;
        this.index = index;
        slotNumber.text = (index+1).ToString();
    }

    internal void SetServer(InventoryItem item)
    {
        this.server = item.item.GetComponentInChildren<DSSServerController>();
        server.Subscribe(this);
        UpdateValues();
    }

    public void UpdateValues()
    {
        if ((!gameObject.activeSelf)) return;
        QuickLogger.Debug($"=================== Updating Values {index} ===================");

        if(server is not null)
        {
            QuickLogger.Debug("Is not null", true);

            float percentage = (float)server.GetStorageTotal() / server.GetMaxStorage();

            progressBar.fillAmount = percentage;

            var val = percentage * 100;

            progressLBL.text = $"{Mathf.CeilToInt(val)}%";

            QuickLogger.Debug($"Percentage : {percentage}", true);
        }
        else
        {
            QuickLogger.Debug("Is null resetting values", true);
            progressBar.fillAmount = 0f;
            progressLBL.text = "N/A";
        }

        QuickLogger.Debug("=================== Updating Values End ===================", true);

    }

    internal bool IsEmpty()
    {
        return server == null;
    }

    internal DSSServerController GetServer()
    {
        return server;
    }

    private void OnDestroy()
    {
        if (server is not null)
        {
            server.UnSubscribe(this);
        }
    }
}
