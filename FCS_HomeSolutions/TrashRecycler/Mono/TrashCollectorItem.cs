using System.Collections.Generic;
using FCS_AlterraHub.Mono;
using FCSCommon.Helpers;
using UnityEngine.UI;

namespace FCS_HomeSolutions.TrashRecycler.Mono
{
    internal class TrashCollectorItem : InterfaceButton
    {

        private Text _amount;
        private uGUI_Icon _icon;
        private bool _isInitialized;
        private float _timeLeft;
        private Recycler _recycler;
        private TechType _techType;
        private const float MAXTIME = 0.1f;

        public override void Update()
        {
            base.Update();
            if (_amount == null || _techType == TechType.None) return;
            _timeLeft -= DayNightCycle.main.deltaTime;
            if (_timeLeft <= 0)
            {
                Refresh();
                _timeLeft = MAXTIME;
            }
        }

        private void Refresh()
        {
            if (_techType == TechType.None) return;
            _amount.text = _recycler.GetCount(_techType).ToString();
        }

        private void Initialize()
        {
            if (_isInitialized) return;
            _icon = GameObjectHelpers.FindGameObject(gameObject, "Icon")?.AddComponent<uGUI_Icon>();
            _amount = gameObject.GetComponentInChildren<Text>();
            _isInitialized = true;
        }

        public void UpdateItem(KeyValuePair<TechType, int> elementAt, Recycler recycler)
        {
            _recycler = recycler;
            Tag = elementAt.Key;
            _techType = elementAt.Key;
            Initialize();
            _icon.sprite = SpriteManager.Get(elementAt.Key);
        }
    }
}