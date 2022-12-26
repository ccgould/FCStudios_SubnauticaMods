using FCS_AlterraHub.Configuration;
using FCS_AlterraHub.Helpers;
using FCS_AlterraHub.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static GameAchievements;

namespace FCS_AlterraHub.Mods.AlterraHubPod.Mono
{
    [RequireComponent(typeof(WeldablePoint))]
    internal class PodSolarClusterController:HandTarget, IHandTarget
    {
        internal int Id { get; private set; }
        private LiveMixin _liveMixin;
        private GameObject _fixMesh;
        private GameObject _spark;
        private GameObject _damageMesh;
        private PowerSource _powerSource;
        private RegeneratePowerSource _regenerate;
        public bool IsRepaired => _liveMixin?.health >= 100;

        internal void Initialize(int id)
        {
            Id = id;
            _powerSource = gameObject.AddComponent<PowerSource>();
            _powerSource.maxPower = 37.5f;
            gameObject.SetActive(false);

            _regenerate = gameObject.AddComponent<RegeneratePowerSource>();
            _regenerate.powerSource = _powerSource;


            gameObject.SetActive(true);

            gameObject.SetActive(false);
            _liveMixin = gameObject.AddComponent<LiveMixin>();
            _liveMixin.startHealthPercent = 0;
            _liveMixin.health = 0;
            _liveMixin.data = CustomLiveMixinData.Get();
            gameObject.SetActive(true);
            _fixMesh = GameObjectHelpers.FindGameObject(gameObject, "fix_mesh");
            _damageMesh = GameObjectHelpers.FindGameObject(gameObject, "damage_mesh");
            AddSpark();
            InvokeRepeating(nameof(TryRegenerate),1f,1f);
            StartCoroutine(CheckHealth());
        }

        private void TryRegenerate()
        {
            _regenerate.enabled = _liveMixin.IsFullHealth();
        }

        internal PowerSource GetPowerSource()
        {
            return _powerSource;
        }

        private void AddSpark()
        {
            var sparkPnt = GameObjectHelpers.FindGameObject(gameObject, "xElecSource");
            StartCoroutine(SpawnHelper.SpawnUWEPrefab(UWEPrefabID.UnderwaterElecSourceMedium, sparkPnt.transform,
                OnEmissionAdded));
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
            Mod.GamePlaySettings.FixedPowerBoxes.Add(Id);
            _fixMesh.SetActive(true);
            _damageMesh.SetActive(false);
            _spark?.SetActive(false);
            if (forceFullHealth)
            {
                _liveMixin.defaultHealth = 1;
                _liveMixin.health = 100;
            }
        }

        public void OnHandHover(GUIHand hand)
        {
            if (_liveMixin.health < _liveMixin.maxHealth)
            {
                HandReticle.main.SetText(HandReticle.TextType.Hand, $"{Language.main.Get("DamagedWires")}\n{Language.main.Get("WeldToFix")}", false);

            }
        }

        public void OnHandClick(GUIHand hand)
        {
        }

        public void MakeDirty()
        {
            _fixMesh.SetActive(false);
            _damageMesh.SetActive(true);
            _spark?.SetActive(true);
            _liveMixin.defaultHealth = 0;
            _liveMixin.health = 0;
        }
    }
}