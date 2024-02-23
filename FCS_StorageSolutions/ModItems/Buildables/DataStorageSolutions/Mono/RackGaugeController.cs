using FCSCommon.Utilities;
using UnityEngine;
using UnityEngine.UI;

namespace FCS_StorageSolutions.ModItems.Buildables.DataStorageSolutions.Mono;
public class RackGaugeController : MonoBehaviour
{
    [SerializeField] private Image progressBar;
    [SerializeField] private Text progressLBL;

    internal void UpdateValues(DSSServerController server)
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
}
