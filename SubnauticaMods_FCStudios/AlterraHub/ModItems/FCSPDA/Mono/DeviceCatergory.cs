using FCS_AlterraHub.API;
using FCS_AlterraHub.Core.Helpers;
using FCS_AlterraHub.Core.Services;
using FCS_AlterraHub.Models.Abstract;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace FCS_AlterraHub.ModItems.FCSPDA.Mono
{
    internal class DeviceCatergory : MonoBehaviour
    {
        private Transform _content;
        private DevicePageController _controller;

        internal void Initialize(DevicePageController controller,KeyValuePair<string, List<FCSDevice>> devices)
        {
            _controller = controller;
            var title = gameObject.FindChild("Title").GetComponentInChildren<TMP_Text>();
            title.text = devices.Key;

            var dropDownToggle = GameObjectHelpers.FindGameObject(gameObject, "DropDownToggle").GetComponent<Toggle>();
            var infoBTN = GameObjectHelpers.FindGameObject(gameObject, "InfoBTN").GetComponent<Button>();
            infoBTN.onClick.AddListener(() => 
            {
                if (devices.Value.Count > 0)
                {
                    FCSPDAController.Main.GetGUI().OnInfoButtonClicked?.Invoke(devices.Value[0]?.GetTechType() ?? TechType.None);
                }
            });

            _content = gameObject.FindChild("Device_Content").transform;

            dropDownToggle.onValueChanged.AddListener((b)=> 
            {
                _content.gameObject.SetActive(b);
            });

            foreach (var device in devices.Value)
            {
                if (!device.IsVisibleInPDA && !_controller.GetShowAllState()) continue;
                var item = Instantiate(FCSAssetBundlesService.PublicAPI.GetLocalPrefab("DeviceEntry"));
                var deviceEntry = item.EnsureComponent<DeviceEntry>();
                deviceEntry.Initialize(device);
                item.gameObject.transform.SetParent(_content, false);
            }
        }
    }
}