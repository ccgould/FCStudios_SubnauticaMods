using SMLHelper.V2.Json.ExtensionMethods;
using Steamworks;
using UnityEngine;

namespace FCS_EnergySolutions.PowerStorage.Mono
{

    internal class PowerSupply : MonoBehaviour, IPowerInterface
    {
        private PowerStorageController _mono;
        private bool _allowedToDisCharge;
        private PowerCellCharger _powerCharger => _mono?.PowercellCharger;

        internal void Initialize(PowerStorageController mono)
        {
            _mono = mono;
        }

        public float GetPower()
        {
            if (!_allowedToDisCharge) return 0;
            return _powerCharger?.GetTotal()??0;
        }

        public float GetMaxPower()
        {
            if (!_allowedToDisCharge) return 0;
            return _powerCharger?.GetCapacity() ?? 0;
        }

        internal string  GetPowerString()
        {
            if (_powerCharger?.GetTotal() == null) return "0/0";
            return $"{Mathf.RoundToInt(_powerCharger.GetTotal())}/{Mathf.RoundToInt(_powerCharger.GetCapacity())}";
        }

        public bool ModifyPower(float amount, out float consumed)
        {
            consumed = 0f;
            if (_powerCharger == null || !_allowedToDisCharge) return false;

            var currentPower = _powerCharger.GetTotal();
            var currentCapacity = _powerCharger.GetCapacity();

            float num = currentPower;
            bool result;
            
            if (amount >= 0f)
            {
                result = (amount <= currentCapacity - currentPower);
            }
            else
            {
                result = (currentPower >= -amount);
            }

            if (GameModeUtils.RequiresPower())
            {
                consumed = _powerCharger.GetTotal() + amount - num;
                _powerCharger.RemoveCharge(consumed);
            }
            else
            {
                consumed = amount;
            }
            return result;
        }

        public bool HasInboundPower(IPowerInterface powerInterface)
        {
            return false;
        }

        public bool GetInboundHasSource(IPowerInterface powerInterface)
        {
            return false;
        }

        private void OnDestroy()
        {
            _powerCharger?.RemovePowerSource(this);
        }

        public void SetAllowedToCharge(bool value)
        {
            _powerCharger.SetAllowedToCharge(value);

            _allowedToDisCharge = !value;
        }

        public void AddItem(InventoryItem item)
        {
            _powerCharger.AddItemToContainer(item);
        }

        public byte[] Save(ProtobufSerializer serializer)
        {
            return _powerCharger.Save(serializer);
        }

        public void Load(ProtobufSerializer serializer, byte[] savedDataData)
        {
            _powerCharger.Load(serializer, savedDataData);
        }

        public bool HasPowercells()
        {
            return _powerCharger.HasPowerCells();
        }
    }
}
