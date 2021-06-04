using System.Collections;
using FCSCommon.Helpers;
using FCSCommon.Objects;
using UnityEngine;

namespace FCS_AlterraHub.Mono.AlterraHubFabricatorBuilding.Mono
{
    [RequireComponent(typeof(WeldablePoint))]
    internal class ElectricalBox : HandTarget, IHandTarget
    {
        internal int Id { get; private set; }
        private LiveMixin _liveMixin;
        private AntennaController _controller;
        private GameObject _plate;
        public bool IsRepaired => _liveMixin?.health >= 100;

        internal void Initialize(AntennaController controller, int id)
        {
            Id = id;
            _controller = controller;
            gameObject.SetActive(false);
            _liveMixin = gameObject.AddComponent<LiveMixin>();
            _liveMixin.startHealthPercent = 0;
            _liveMixin.health = 0;
            _liveMixin.data = CustomLiveMixinData.Get();
            gameObject.SetActive(true);
            _plate = GameObjectHelpers.FindGameObject(gameObject, "door_mesh");
            StartCoroutine(CheckHealth());
        }

        private IEnumerator CheckHealth()
        {
            while (_liveMixin.health < 100)
            {
                yield return null;
            }

            Fix();
            yield break;
        }

        internal void Fix(bool forceFullHealth = false)
        {
            _controller.OnBoxFixed(Id);
            _plate.SetActive(true);
            if (forceFullHealth)
            {
                _liveMixin.initialHealth = 1;
                _liveMixin.health = 100;
            }
        }

        public void OnHandHover(GUIHand hand)
        {
            if (_liveMixin.health < _liveMixin.maxHealth)
            {
                HandReticle.main.SetInteractTextRaw("DamagedWires", "WeldToFix");
            }
        }

        public void OnHandClick(GUIHand hand)
        {
        }
    }
}