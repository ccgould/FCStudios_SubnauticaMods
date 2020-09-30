using DataStorageSolutions.Buildables;
using DataStorageSolutions.Mono;
using FCSCommon.Components;
using FCSCommon.Helpers;
using UnityEngine;

namespace DataStorageSolutions.Display
{
    internal class ItemInterfaceButton : InterfaceButton
    {
        private float _time;
        public float interpolationPeriod = 0.1f;
        private object _tag;
        private uGUI_Icon _icon;
        private TechType _techType;

        public override object Tag
        {
            get => _tag;
            set
            {
                _tag = value;
                SetIcon((TechType)value);
            }
        }

        private void Start()
        {
            TextComponent.text = string.Empty;
        }

        private void SetIcon(TechType value)
        {
            _techType = value;
            TextLineOne = string.Format(AuxPatchers.TakeFormatted(), Language.main.Get(value));

            uGUI_Icon trashIcon = GetIcon();
            trashIcon.sprite = SpriteManager.Get(value);
        }

        private uGUI_Icon GetIcon()
        {
            if (_icon == null)
            {
                _icon = InterfaceHelpers.FindGameObject(gameObject, "Icon").AddComponent<uGUI_Icon>();
            }

            return _icon;
        }

        public BaseManager Manager { get; set; }
        
        public override void Update()
        {
            base.Update();

            _time += Time.deltaTime;

            if (_time >= interpolationPeriod)
            {
                _time = 0.0f;

                if (!gameObject.activeSelf) return;

                TextComponent.text = Manager.StorageManager.GetItemCount(_techType).ToString();
            }
        }
    }
}
