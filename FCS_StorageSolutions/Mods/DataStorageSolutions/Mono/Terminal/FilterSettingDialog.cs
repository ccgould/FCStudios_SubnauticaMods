using FCS_AlterraHub.Enumerators;
using FCS_AlterraHub.Mono;
using FCSCommon.Helpers;
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
                _mono.ChangeStorageFilter(StorageLocation.All);
                _label.text = "ALL";
            }

            if (button.Equals("ToggleButton_1"))
            {
                _mono.ChangeStorageFilter(StorageLocation.Servers);
                _label.text = "SERVERS";
            }

            if (button.Equals("ToggleButton_2"))
            {
                _mono.ChangeStorageFilter(StorageLocation.AlterraStorage);
                _label.text = "ALTERRA STORAGE";
            }

            if (button.Equals("ToggleButton_3"))
            {
                _mono.ChangeStorageFilter(StorageLocation.StorageLockers);
                _label.text = "STORAGE LOCKERS";
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