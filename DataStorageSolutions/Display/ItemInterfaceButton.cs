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
        private TechType _techType;
        public BaseManager Manager { get; set; }

        private void Start()
        {
            _techType = (TechType) Tag;
            
            TextLineOne = string.Format(AuxPatchers.TakeFormatted(), Language.main.Get(_techType));

            uGUI_Icon trashIcon = InterfaceHelpers.FindGameObject(gameObject, "Icon").AddComponent<uGUI_Icon>();
            trashIcon.sprite = SpriteManager.Get(_techType);
        }

        public override void Update()
        {
            base.Update();

            _time += Time.deltaTime;

            if (_time >= interpolationPeriod)
            {
                _time = 0.0f;
                var itemsWithin = Manager.StorageManager.GetItemsWithin();
                if (itemsWithin.ContainsKey(_techType))
                {
                    TextComponent.text = itemsWithin[_techType].ToString();
                }
            }
        }
    }
}
