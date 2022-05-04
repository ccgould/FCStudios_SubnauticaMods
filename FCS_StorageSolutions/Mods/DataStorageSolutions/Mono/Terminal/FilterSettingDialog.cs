using FCS_AlterraHub.Enumerators;
using FCS_AlterraHub.Helpers;
using FCS_AlterraHub.Mono;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace FCS_StorageSolutions.Mods.DataStorageSolutions.Mono.Terminal
{
    internal class FilterSettingDialog : InterfaceButton
    {
        private GameObject _dialog;

        private DSSTerminalDisplayManager _mono;
        private Text _label;

        internal void Initialize(GameObject root, DSSTerminalDisplayManager mono)
        {
            _mono = mono;
            _label = gameObject.GetComponentInChildren<Text>();
            _dialog = GameObjectHelpers.FindGameObject(root, "FilterSettingList");
            var grid = _dialog.transform.GetChild(0);
            var group = grid.gameObject.EnsureComponent<FCSToggleGroup>();
            group.SetColor(new Color(0.1254902f, 0.5607843f, 0.6588235f, 1f),default);
            group.SetMode(InterfaceButtonMode.Aplha);
            group.Initialize(); 
            group.OnToggleButtonAction += OnToggleButtonAction;
        }

        private void OnToggleButtonAction(string button)
        {
            if (button.Equals("ToggleButton_0"))
            {
                _mono.ChangeStorageFilter(StorageType.All);
                _label.text = "ALL";
            }

            if (button.Equals("ToggleButton_1"))
            {
                _mono.ChangeStorageFilter(StorageType.Servers);
                _label.text = "SERVERS";
            }

            if (button.Equals("ToggleButton_2"))
            {
                _mono.ChangeStorageFilter(StorageType.RemoteStorage);
                _label.text = "REMOTE STORAGE UNIT";
            }

            if (button.Equals("ToggleButton_3"))
            {
                _mono.ChangeStorageFilter(StorageType.StorageLockers);
                _label.text = "STORAGE LOCKERS";
            }

            if (button.Equals("ToggleButton_4"))
            {
                _mono.ChangeStorageFilter(StorageType.SeaBreeze);
                _label.text = "SEABREEZE";
            }

            if (button.Equals("ToggleButton_5"))
            {
                _mono.ChangeStorageFilter(StorageType.Harvester);
                _label.text = "HARVESTER";
            }

            if (button.Equals("ToggleButton_6"))
            {
                _mono.ChangeStorageFilter(StorageType.Replicator);
                _label.text = "REPLICATOR";
            }
        }

        internal void Hide()
        {
            _dialog.SetActive(false);
        }

        public override void OnPointerClick(PointerEventData eventData)
        {
            if (_dialog != null)
            {
                _dialog.SetActive(!_dialog.activeSelf);
            }
        }
    }
}