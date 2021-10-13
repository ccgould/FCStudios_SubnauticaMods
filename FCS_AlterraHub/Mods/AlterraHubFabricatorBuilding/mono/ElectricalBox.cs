using System.Collections;
using System.Security.Cryptography;
using FCS_AlterraHub.Helpers;
using FCS_AlterraHub.Model;
using FCSCommon.Helpers;
using UnityEngine;

namespace FCS_AlterraHub.Mods.AlterraHubFabricatorBuilding.Mono
{
    [RequireComponent(typeof(WeldablePoint))]
    internal class ElectricalBox : HandTarget, IHandTarget
    {
        internal int Id { get; private set; }
        private LiveMixin _liveMixin;
        private AntennaController _controller;
        private GameObject _plate;
        private GameObject _spark;
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
            AddSpark();
            StartCoroutine(CheckHealth());
        }

        private void AddSpark()
        {
            var sparkPnt = GameObjectHelpers.FindGameObject(gameObject, "xElecSource");
            StartCoroutine(SpawnHelper.SpawnUWEPrefab(UWEPrefabID.UnderwaterElecSourceMedium, sparkPnt.transform, OnEmissionAdded));
        }

        private void OnEmissionAdded(GameObject obj)
        {
            var fx = obj.GetComponentsInChildren<ParticleSystem>();

            foreach (ParticleSystem particleSystem in fx)
            {
                var main = particleSystem.main;
                main.scalingMode = ParticleSystemScalingMode.Hierarchy;
            }
            obj.transform.localScale = new Vector3(0.4f, 0.4f, 0.4f);
            _spark = obj;

            if (IsRepaired)
            {
                obj.SetActive(false);
            }
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
            _spark?.SetActive(false);
            if (forceFullHealth)
            {
#if SUBNAUTICA
                _liveMixin.initialHealth = 1;
#endif
                _liveMixin.health = 100;
            }
        }

        public void OnHandHover(GUIHand hand)
        {
            if (_liveMixin.health < _liveMixin.maxHealth)
            {
#if SUBNAUTICA
                HandReticle.main.SetInteractTextRaw(Language.main.Get("DamagedWires"), Language.main.Get("WeldToFix"));
#else
                HandReticle.main.SetText(HandReticle.TextType.Hand, $"{Language.main.Get("DamagedWires")}\n{Language.main.Get("WeldToFix")}", false);
#endif
            }
        }

        public void OnHandClick(GUIHand hand)
        {
        }
    }
}