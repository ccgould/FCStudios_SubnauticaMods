using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using FCS_AlterraHub.Enumerators;
using FCS_AlterraHub.Mono.FCSPDA.Mono.ScreenItems;
using FCSCommon.Helpers;
using UnityEngine;

namespace FCS_AlterraHub.Mono.AlterraHub
{
    internal class PanelHelper : MonoBehaviour
    {
        private bool _isInitialized;
        private GameObject _content;
        private readonly List<StoreItem> _storeItems = new List<StoreItem>();

        internal StoreCategory StoreCategory { get; set; }

        private void Initialize()
        {

            if (_isInitialized) return;

            _content = GameObjectHelpers.FindGameObject(gameObject, "Grid");

            if (_content != null)
            {
                foreach (Transform child in _content.transform)
                {
                    Destroy(child.gameObject);
                }
                _isInitialized = true;
            }


        }

        internal void AddContent(GameObject storeItem)
        {
            Initialize();
            if (!_isInitialized) return;
            storeItem.transform.SetParent(_content.transform, false);
            var storeItemComp = storeItem.GetComponent<StoreItem>();
            _storeItems.Add(storeItemComp);
            storeItem.SetActive(false);
        }
        
        internal void RefreshStoreItems()
        {
            foreach (StoreItem item in _storeItems)
            {
                item.CheckIsUnlocked();
            }
        }

        [Obsolete("This method can be used but is handled when the tab is clicked")]
        internal void ActivateStoreItem(TechType techType)
        {
            foreach (StoreItem item in _storeItems)
            {
                if (item.TechType == techType)
                {
                    item.IsVisibleForced = true;
                    item.gameObject.SetActive(true);
                    break;
                }
            }
        }
    }
}