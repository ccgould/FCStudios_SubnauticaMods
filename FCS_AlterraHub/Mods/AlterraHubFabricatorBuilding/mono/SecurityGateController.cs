using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using FCS_AlterraHub.Buildables;
using FCS_AlterraHub.Configuration;
using FCS_AlterraHub.Helpers;
using FCS_AlterraHub.Model;
using FCSCommon.Utilities;
using UnityEngine;

namespace FCS_AlterraHub.Mods.AlterraHubFabricatorBuilding.Mono
{
    [RequireComponent(typeof(WeldablePoint))]
    internal class SecurityGateController : HandTarget, IHandTarget
    {
        internal int Id { get; private set; }
        private LiveMixin _liveMixin;
        private List<SecurityDoorController> _doors = new();
        private List<GameObject> _sparks = new();
        public bool IsRepaired => _liveMixin?.health >= 100;

        internal void Initialize()
        {
            gameObject.SetActive(false);
            _liveMixin = gameObject.AddComponent<LiveMixin>();
            _liveMixin.startHealthPercent = 0;
            _liveMixin.health = 0;
            _liveMixin.data = CustomLiveMixinData.Get();
            gameObject.SetActive(true);


            var door1 = GameObjectHelpers.FindGameObject(gameObject, "Door01").AddComponent<SecurityDoorController>();
            door1.openRot = Quaternion.Euler(door1.transform.localEulerAngles.x, -90, door1.transform.localEulerAngles.y);
            door1.ManualHoverText1 = "Weld";
            door1.ManualHoverText2 = "WeldToFix";
            door1.OnDoorStateChanged += OnDoorStateChanged;
            _doors.Add(door1);

            var door2 = GameObjectHelpers.FindGameObject(gameObject, "Door02").AddComponent<SecurityDoorController>();
            door2.openRot = Quaternion.Euler(door2.transform.localEulerAngles.x, 90, door2.transform.localEulerAngles.y);
            door2.ManualHoverText1 = "Weld";
            door2.ManualHoverText2 = "WeldToFix";
            door2.OnDoorStateChanged += OnDoorStateChanged;
            _doors.Add(door2);

            AddSpark();
            StartCoroutine(CheckHealth());
        }

        private void OnDoorStateChanged(DoorController caller, bool doorOpen)
        {
            foreach (SecurityDoorController door in _doors)
            {
                if (caller == door) continue;
                if (doorOpen)
                {
                    door.Open();
                }
                else
                {
                    door.Close();
                }
            }
        }

        private void AddSpark()
        {
            var sparkPnt = GameObjectHelpers.FindGameObjects(gameObject, "xElecSource",SearchOption.StartsWith);
            foreach (GameObject sparkPntObj in sparkPnt)
            {
                StartCoroutine(SpawnHelper.SpawnUWEPrefab(UWEPrefabID.UnderwaterElecSourceMedium, sparkPntObj.transform, OnEmissionAdded));
            }
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
            _sparks.Add(obj);

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
            Unlock();
            yield break;
        }

        internal void Unlock(bool forceFullHealth = false)
        {
            foreach (SecurityDoorController door in _doors)
            {
                door.UnlockDoor();
            }

            foreach (GameObject spark in _sparks)
            {
                spark.SetActive(false);
            }

            if (forceFullHealth)
            {
                _liveMixin.ResetHealth();
            }

            MaterialHelpers.ChangeEmissionColor(AlterraHub.BaseDecalsEmissiveController, gameObject, Color.cyan);
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

        public float GetHealth()
        {
            return _liveMixin.health;
        }

        public void LoadSave()
        {
            QuickLogger.Debug("Loading Security Doors Save",true);
            if (Mod.GamePlaySettings.AlterraHubDepotDoors.SecurityDoors >= 100)
            {
                Unlock(true);
            }
            else
            {
                _liveMixin.AddHealth(Mod.GamePlaySettings.AlterraHubDepotDoors.SecurityDoors);
            }
        }

        public void Lock()
        {
            foreach (SecurityDoorController door in _doors)
            {
                door.LockDoor();
            }

            foreach (GameObject spark in _sparks)
            {
                spark.SetActive(true);
            }

#if SUBNAUTICA
            _liveMixin.initialHealth = 0;
#endif
            _liveMixin.health = 0;

            MaterialHelpers.ChangeEmissionColor(AlterraHub.BaseDecalsEmissiveController, gameObject, Color.red);
        }

        public bool IsUnlocked()
        {
            return _doors.All(x => x.IsUnlocked);
        }
    }

    internal class SecurityDoorController : DoorController
    {
        public override bool IsSwingDoor => true;
    }
}