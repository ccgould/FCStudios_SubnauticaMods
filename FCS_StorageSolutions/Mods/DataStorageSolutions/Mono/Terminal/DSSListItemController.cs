using System.Collections.Generic;
using FCS_AlterraHub.Mono;
using FCSCommon.Components;
using FCSCommon.Utilities;
using UnityEngine;
using UnityEngine.UI;

namespace FCS_StorageSolutions.Mods.DataStorageSolutions.Mono.Terminal
{
    internal class DSSListItemController : InterfaceButton
    {
        private Text _title;
        private readonly List<Image> _icons = new List<Image>();
        private BaseManager _manager;

        internal void Initialize(BaseManager manager)
        {
            BtnName = "BaseBTN";
            Tag = manager;
            STARTING_COLOR = new Color(0.1254902f, 0.5607843f, 0.6588235f, 1);
            HOVER_COLOR = new Color(0.181f, 0.652f, 0.708f, 1);
            _manager = manager;
            _title = gameObject.GetComponentInChildren<Text>();
            foreach (Transform child in transform)
            {
                _icons.Add(child.GetComponent<Image>());
            }

            InvokeRepeating(nameof(UpdateState), 1f, 1f);

        }

        private void UpdateState()
        {
            if (_manager == null) return;

            gameObject.SetActive(_manager.IsVisible);

            if (_title != null)
            {
                _title.text = _manager.GetBaseName();
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
                _icons[i]?.gameObject.SetActive(i == index);
            }
        }
        
        internal BaseManager GetBaseManager()
        {
            return _manager;
        }

        internal void Purge()
        {
            Destroy(gameObject);
        }
    }
}