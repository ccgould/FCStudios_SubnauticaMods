using FCS_AlterraHub.Core.Helpers;
using FCSCommon.Utilities;
using UnityEngine.EventSystems;
using UnityEngine;
using UnityEngine.UI;
using FCS_StorageSolutions.ModItems.Buildables.DataStorageSolutions.Mono.Base;

namespace FCS_StorageSolutions.ModItems.Buildables.DataStorageSolutions.Mono;

public class uGUI_RackGaugeController : MonoBehaviour, IPointerHoverHandler, IPointerExitHandler
{
    private RackBase baseRack;
    [SerializeField] private GameObject buttonsGroup;
    [SerializeField] private Image progressBar;
    [SerializeField] private Text progressLBL;
    [SerializeField] private Text slotNumber;
    private RackBase rackBase;
    private int index = -1;
    private DSSServerController server;
    private RackSlotData rackSlotData;

    private void Awake()
    {
        InvokeRepeating(nameof(UpdateValues), .2f, .2f);
;   }

    public void OnEjectButtonPressed()
    {
        var isRemoved = rackBase.ClearSlotAndReturnServer(rackSlotData);

        if (isRemoved)
        {
            PlayerInteractionHelper.GivePlayerItem(isRemoved);

            Eject();
        }
    }

    public void Eject()
    {
        if (server != null)
        {
            //server.UnSubscribe(this);
            server = null;
        }

        //UpdateValues();
        buttonsGroup?.SetActive(false);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        buttonsGroup?.SetActive(false);
    }

    public void OnPointerHover(PointerEventData eventData)
    {
        if (server is not null)
        {
            buttonsGroup?.SetActive(true);
        }
    }

    public void OnSettingsButtonPressed()
    {

    }

    private void SetServer(DSSServerController item, RackBase rackBase)
    {
        //item?.Subscribe(this);
        server = item;
        rackSlotData = item?.GetCurrentSlot();
        this.rackBase = rackBase;
        //UpdateValues();
    }

    private void UpdateValues()
    {
        if (!gameObject.activeSelf) return;
        //QuickLogger.Debug($"=================== Updating Values {index} ===================");

        if (server is not null)
        {
            //QuickLogger.Debug("Is not null", true);

            float percentage = (float)server.GetStorageTotal() / server.GetMaxStorage();

            progressBar.fillAmount = percentage;

            var val = percentage * 100;

            progressLBL.text = $"{Mathf.CeilToInt(val)}%";

            //QuickLogger.Debug($"Percentage : {percentage}", true);
        }
        else
        {
            //QuickLogger.Debug("Is null resetting values", true);
            progressBar.fillAmount = 0f;
            progressLBL.text = "N/A";
        }

        //QuickLogger.Debug("=================== Updating Values End ===================", true);

    }

    internal bool IsEmpty()
    {
        return server == null;
    }

    internal DSSServerController GetServer()
    {
        return server;
    }

    private void OnDisable()
    {

        QuickLogger.Debug("OnDisbale RackGauge", true);
        if (server is not null)
        {
            //server.UnSubscribe(this);
        }
    }

    private void OnDestroy()
    {
        if (server is not null)
        {
            //server.UnSubscribe(this);
        }
    }

    internal int GetSlotId()
    {
        return index;
    }

    internal void SetSlot(RackSlotData rackSlotData, RackBase rackBase, int id)
    {
        index = id;
        slotNumber.text = (index + 1).ToString();
        SetServer(rackSlotData.GetServer(), rackBase);
    }
}
