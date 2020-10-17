using System.Security.Cryptography;
using FCS_AlterraHub.Enumerators;
using FCSCommon.Helpers;
using UnityEngine;

namespace FCS_AlterraHub.Mono.AlterraHub
{
    internal class PanelHelper : MonoBehaviour
    {
        private bool _isInitialized;
        private GameObject _content;

        internal StoreCategory StoreCategory { get; set; }

        private void Initialize()
        {

            if (_isInitialized) return;

            _content = GameObjectHelpers.FindGameObject(gameObject, "StoreGrid");

            foreach (Transform child in _content.transform)
            {
                Destroy(child.gameObject);
            }

            _isInitialized = true;
        }

        internal void AddContent(GameObject storeItem)
        {
            Initialize();
            storeItem.transform.SetParent(_content.transform, false);
        }
    }
}