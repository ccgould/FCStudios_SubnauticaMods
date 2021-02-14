using FCSCommon.Helpers;
using UnityEngine;
using UnityEngine.UI;

namespace FCS_AlterraHub.Mono.FCSPDA.Mono
{
    internal class DSSBaseItem : InterfaceButton
    {
        private GameObject _hubIcon;
        private GameObject _cyclopsIcon;
        private Text _baseName;

        private void Initialize()
        {
            _hubIcon = GameObjectHelpers.FindGameObject(gameObject, "Habitat");
            _cyclopsIcon = GameObjectHelpers.FindGameObject(gameObject, "Cyclops");

            if (_baseName == null)
            {
                _baseName = gameObject.FindChild("Text").EnsureComponent<Text>();
            }
        }

        internal void Set(BaseManager baseManager)
        {
            Initialize();
            Tag = baseManager;
            _baseName.text = baseManager.GetBaseName();

            if (baseManager.Habitat.isCyclops)
            {
                _cyclopsIcon.SetActive(true);
                _hubIcon.SetActive(false);
            }
            else
            {
                _cyclopsIcon.SetActive(false);
                _hubIcon.SetActive(true);
            }
            Show();
        }

        internal void Reset()
        {
            Initialize();
            _baseName.text = "";
            _cyclopsIcon.SetActive(false);
            _hubIcon.SetActive(true);
            Tag = null;
            Hide();
        }

        internal void Hide()
        {
            gameObject.SetActive(false);
        }

        internal void Show()
        {
            gameObject.SetActive(true);
        }
    }
}