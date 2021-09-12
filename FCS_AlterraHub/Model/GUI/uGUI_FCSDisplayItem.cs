using FCS_AlterraHub.Helpers;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace FCS_AlterraHub.Model.GUI
{
    class uGUI_FCSDisplayItem :MonoBehaviour
    {
        private uGUI_Icon _icon;
        private Texture2D _texture;
        private Atlas.Sprite _sprite;
        private Toggle _toggle;
        private Button _button;
        private TechType _techType;

        public void Initialize(TechType techType,bool isToggle = false,bool isButton = false)
        {
            if (isToggle)
            {
                _toggle = gameObject.GetComponent<Toggle>();
            }

            if (isButton)
            {
                _button = gameObject.GetComponent<Button>();
            }

            _icon = GameObjectHelpers.FindGameObject(gameObject, "Icon").AddComponent<uGUI_Icon>();
            Set(techType);
        }

        public void Subscribe(UnityAction<bool> callback)
        {
            if (_toggle != null)
            {
                _toggle.onValueChanged.AddListener(callback);
            }
        }

        public void Subscribe(UnityAction callback)
        {
            if (_button != null)
            {
                _button.onClick.AddListener(callback);
            }
        }
        
        public void UnSubscribe(UnityAction<bool> callback)
        {
            if (_toggle != null)
            {
                _toggle.onValueChanged.RemoveListener(callback);
            }
        }

        public void UnSubscribe(UnityAction callback)
        {
            if (_button != null)
            {
                _button.onClick.RemoveListener(callback);
            }
        }

        public void Select()
        {
            if (_toggle != null)
            {
                _toggle.isOn = true;
            }

            if (_button != null)
            {
                _button.Select();
            }
        }

        public void Deselect()
        {
            if (_toggle != null)
            {
                _toggle.isOn = false;
            }
        }

        public void Clear()
        {
            _icon.sprite = SpriteManager.Get(TechType.None);
            _techType = TechType.None;
        }

        public TechType GetTechType()
        {
            return _techType;
        }

        public void Set(TechType techType)
        {
            _icon.sprite = SpriteManager.Get(techType);
            _techType = techType;
        }
    }
}
