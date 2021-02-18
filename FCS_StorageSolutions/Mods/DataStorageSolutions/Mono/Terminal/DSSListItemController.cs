using System.Collections.Generic;
using FCS_AlterraHub.Mono;
using FCSCommon.Utilities;
using UnityEngine;
using UnityEngine.UI;

namespace FCS_StorageSolutions.Mods.DataStorageSolutions.Mono.Terminal
{
    internal class DSSListItemController : InterfaceButton
    {
        private Text _title;
        private readonly List<GameObject> _icons = new List<GameObject>();
        private BaseManager _manager;
        private bool _isInitialized;
        private Button _settingsBTN;
        private DSSTerminalDisplayManager _displayController;

        private void Initialize()
        {
            if (_isInitialized) return;
            BtnName = "BaseBTN";
            STARTING_COLOR = new Color(0.1254902f, 0.5607843f, 0.6588235f, 1);
            HOVER_COLOR = new Color(0.181f, 0.652f, 0.708f, 1);
            _title = gameObject.GetComponentInChildren<Text>();
            _settingsBTN = gameObject.GetComponentInChildren<Button>();
            _settingsBTN.onClick.AddListener(() =>
            {
                //Open the Transceiver Dialog
                _displayController.OpenItemTransceiverPage(this);

            });


            for (int i = 0; i < 5; i++)
            {
                _icons.Add(gameObject.transform.GetChild(i).gameObject);
            }

            InvokeRepeating(nameof(UpdateState),1,1);

            _isInitialized = true;
        }
        
        internal void Set(BaseManager manager, bool isCurrentBase, DSSTerminalDisplayManager displayController)
        {
            if (manager == null) return;

            Initialize();

            ChangeIcon(isCurrentBase ? DssListItemIcon.Current : manager.Habitat.isCyclops ? DssListItemIcon.Cyclops : DssListItemIcon.HUB);

            Tag = manager;
            _displayController = displayController;
            _manager = manager;
            gameObject.SetActive(true);
            UpdateState();
        }

        private void UpdateState()
        {
            if (_manager == null) return;

            if (_title != null)
            {
                _title.text = _manager.GetBaseName();
            }

            if (_settingsBTN != null)
            {
                _settingsBTN.interactable = _manager.HasTransceiverConnected();
            }
        }

        internal void ChangeIcon(DssListItemIcon icon)
        {
            if (_icons == null) return;
            int index = (int)icon;
            

            QuickLogger.Debug($"Icon Index: {index} | Count {_icons.Count}");

            for (int i = 0; i < _icons.Count; i++)
            {
                QuickLogger.Debug($"Icon Index Name: {_icons[i]?.gameObject.name}");
                _icons[i].SetActive(i == index);
            }
        }
        
        internal BaseManager GetBaseManager()
        {
            return _manager;
        }

        internal void Reset()
        {
            gameObject.SetActive(false);
        }
    }
}