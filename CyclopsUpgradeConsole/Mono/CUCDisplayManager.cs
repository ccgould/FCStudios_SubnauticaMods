using System;
using System.Collections.Generic;
using System.Linq;
using CyclopsUpgradeConsole.Struct;
using FCSCommon.Helpers;
using FCSCommon.Utilities;
using MoreCyclopsUpgrades.API.Buildables;
using UnityEngine;

namespace CyclopsUpgradeConsole.Mono
{
    internal class CUCDisplayManager : MonoBehaviour
    {
        private readonly List<UpgradeTrans> _slots = new List<UpgradeTrans>();

        internal void Setup()
        {
            try
            {
                for (int i = 0; i < 6; i++)
                {
                    var slot = GameObjectHelpers.FindGameObject(gameObject, $"Slot_{i + 1}");
                    var icon = GameObjectHelpers.FindGameObject(slot, "Icon")?.AddComponent<uGUI_Icon>();
                    icon?.gameObject.SetActive(false);
                    var trans = new UpgradeTrans {Slot = $"Module{i + 1}", Icon = icon };
                    _slots.Add(trans);
                }
            }
            catch (Exception e)
            {
                QuickLogger.Error($"Message: {e.Message} | StackTrace: {e.StackTrace}");
            }
        }
        internal void SetIcon(UpgradeSlot slot)
        {
            var button = _slots.Single(x => x.Slot == slot.slotName);
            button.Icon.sprite = SpriteManager.Get(slot.GetTechTypeInSlot());
            button.Icon.gameObject.SetActive(true);
        }

        internal void RemoveIcon(UpgradeSlot slot)
        {
            var button = _slots.Single(x => x.Slot == slot.slotName);
            button.Icon.gameObject.SetActive(false);
        }
    }
}