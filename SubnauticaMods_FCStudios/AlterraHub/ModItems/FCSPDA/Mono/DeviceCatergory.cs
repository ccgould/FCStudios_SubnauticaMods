using FCS_AlterraHub.API;
using FCS_AlterraHub.Models.Abstract;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace FCS_AlterraHub.ModItems.FCSPDA.Mono
{
    internal class DeviceCatergory : MonoBehaviour
    {
        private Transform _content;

        internal void Initialize(KeyValuePair<string, List<FCSDevice>> devices)
        {
            var title = gameObject.FindChild("Title").GetComponentInChildren<TMP_Text>();
            title.text = devices.Key;

            _content = gameObject.FindChild("Device_Content").transform;

            foreach (var device in devices.Value)
            {
                if (!device.IsVisibleInPDA) continue;
                var item = Instantiate(FCSAssetBundlesService.PublicAPI.GetLocalPrefab("DeviceEntry"));
                var deviceEntry = item.EnsureComponent<DeviceEntry>();
                deviceEntry.Initialize(device);
                item.gameObject.transform.SetParent(_content, false);
            }
        }
    }
}