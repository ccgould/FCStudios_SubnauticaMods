using FCS_AlterraHub.Registration;
using FCSCommon.Helpers;
using UnityEngine;
using UnityEngine.UI;

namespace FCS_AlterraHub.Mono.FCSPDA.Mono
{
    internal class BaseOperationItemController : MonoBehaviour
    {
        private BaseTransferOperation _operation;

        public void Initialize(BaseTransferOperation operation,BaseManager manager)
        {
            _operation = operation;

            GameObjectHelpers.FindGameObject(gameObject, "DeviceIcon").AddComponent<uGUI_Icon>().sprite = SpriteManager.Get(FCSAlterraHubService.PublicAPI.GetDeviceTechType(operation.DeviceId));
            GameObjectHelpers.FindGameObject(gameObject, "TransferIcon").AddComponent<uGUI_Icon>().sprite = SpriteManager.Get(operation.TransferItem);
            GameObjectHelpers.FindGameObject(gameObject, "AmountSlot").GetComponent<Text>().text = operation.Amount.ToString();
            GameObjectHelpers.FindGameObject(gameObject, "DeviceName").GetComponent<Text>().text = operation.DeviceId;
            GameObjectHelpers.FindGameObject(gameObject, "IsPullOperation").GetComponent<Toggle>().isOn = operation.IsPullOperation;
            GameObjectHelpers.FindGameObject(gameObject,"DeleteButton").GetComponent<Button>().onClick.AddListener(() =>
            {
                manager.RemoveBaseTransferItem(_operation);
                Destroy(gameObject);
            });
        }
    }
}