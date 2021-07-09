using System;
using FCS_AlterraHub.Buildables;
using FCS_AlterraHub.Helpers;
using FCS_EnergySolutions.Buildable;
using FCSCommon.Utilities;
using UnityEngine;

namespace FCS_EnergySolutions.Mods.PowerStorage.Mono
{
    internal class BatteryDummyController : HandTarget, IHandTarget
    {
        private string _slot;
        private PowerCellCharger _charger;
        private InventoryItem _inventoryItem;
        private Battery _battery;
        private readonly Color _colorEmpty = new Color(1f, 0f, 0f, 1f);
        private readonly Color _colorHalf = new Color(1f, 1f, 0f, 1f);
        private readonly Color _colorFull = new Color(0f, 1f, 0f, 1f);
        private Material _emission;

        public void Update()
        {
            UpdateVisuals();
        }

        public InventoryItem InventoryItem
        {
            get => _inventoryItem;
            set
            {
                _inventoryItem = value;
                _battery = value?.item.GetComponent<Battery>();
            }
        }

        internal void Initialize(string slot, PowerCellCharger charger)
        {
            _slot = slot;
            _charger = charger;
            var m_Material = GetComponent<Renderer>();
            foreach (var materials in m_Material.materials)
            {
                if (materials.name.StartsWith(AlterraHub.BaseLightsEmissiveController,StringComparison.OrdinalIgnoreCase))
                {
                    _emission = materials;
                    break;
                }
            }
        }

        public void OnHandHover(GUIHand hand)
        {
            HandReticle main = HandReticle.main;
            main.SetInteractText(Language.main.Get("CyclopsRemovePowerCell"), _battery.GetChargeValueText());
            main.SetIcon(HandReticle.IconType.Hand);
        }

        public void OnHandClick(GUIHand hand)
        {
            if (PlayerInteractionHelper.CanPlayerHold(InventoryItem.item.GetTechType()))
            {
                _charger.OnUnEquip(_slot, this);
                _battery = null;
                InventoryItem = null;
            }
            else
            {
                QuickLogger.ModMessage(AlterraHub.InventoryFull());
            }
        }

        public void IsVisible(bool value)
        {
            gameObject.SetActive(value);
        }

        public bool GetIsVisible()
        {
            return gameObject.activeSelf;
        }

        public void UpdateVisuals()
        {

            if (_emission == null || _battery == null) return;

            float n = _battery._charge / _battery._capacity;

            if (n >= 0f)
            {
                Color value = (n < 0.5f) ? Color.Lerp(_colorEmpty, _colorHalf, 2f * n) : Color.Lerp(_colorHalf, _colorFull, 2f * n - 1f);
                MaterialHelpers.ChangeEmissionColor(_emission, value);
                return;
            }
            MaterialHelpers.ChangeEmissionColor(_emission, _colorEmpty);
        }
    }
}