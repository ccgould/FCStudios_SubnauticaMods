using FCS_AlterraHub.API;
using FCS_AlterraHub.Core.Helpers;
using FCS_AlterraHub.Core.Services;
using FCS_AlterraHub.ModItems.FCSPDA.Enums;
using FCS_AlterraHub.ModItems.FCSPDA.Mono.Model;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace FCS_AlterraHub.ModItems.FCSPDA.Mono
{
    internal class DevicePageController : Page
    {
        private Transform _content;
        private Toggle _showAllToggle;
        private Dictionary<string, DeviceCatergory> _groups = new();

        internal void Initialize(FCSAlterraHubGUI gui)
        {
            var backButton = gameObject.FindChild("BackBTN")?.GetComponent<Button>();
            _content = GameObjectHelpers.FindGameObject(gameObject.FindChild("Body"),"Content").transform;
            _showAllToggle = GameObjectHelpers.FindGameObject(gameObject, "ShowAll").GetComponent<Toggle>();
            _showAllToggle.onValueChanged.AddListener((b) => 
            {
                Enter(null);
            });
            if (backButton != null)
            {
                backButton.onClick.AddListener((() =>
                {
                    gui.GoToPage(PDAPages.None);
                }));
            }
        }

        internal bool GetShowAllState()
        {
            return _showAllToggle.isOn;
        }

        public override void Enter(object arg)
        {
            base.Enter();

            for (int i = _groups.Count - 1; i >= 0; i--)
            {
                var item = _groups.ElementAt(i);
                Destroy(item.Value.gameObject);
                _groups.Remove(item.Key);
            }

            //Generate a list of devices
            var devices = HabitatService.main.GetDevicesInCurrentBase();

            foreach (var device in devices)
            {
                var category = Instantiate(FCSAssetBundlesService.PublicAPI.GetLocalPrefab("DeviceCategory"));
                var deviceCat = category.EnsureComponent<DeviceCatergory>();
                deviceCat.Initialize(this,device);
                deviceCat.gameObject.transform.SetParent(_content, false);
                _groups.Add(device.Key, deviceCat);
            }
        }
    }
}