using FCSAIPowerCellSocket.Mono;
using FCSCommon.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FCSAIPowerCellSocket.Model
{
    internal class AIPowerCellSocketPowerManager : MonoBehaviour
    {
        private PowerRelay _connectedRelay;
        private AIPowerCellSocketController _mono;
        public float chargeSpeed = 0.005f;


        private readonly HashSet<TechType> _compatibleTech = new HashSet<TechType>()
        {
            TechType.PowerCell,
            TechType.PrecursorIonPowerCell
        };
        internal Dictionary<TechType, float> PowercellTracker = new Dictionary<TechType, float>();
        private ChildObjectIdentifier _containerRoot;
        private ItemsContainer _batteryContainer;


        public float Charge { get; set; }


        internal void Initialize(AIPowerCellSocketController mono)
        {
            _mono = mono;
            StartCoroutine(UpdatePowerRelay());

            if (_containerRoot == null)
            {
                QuickLogger.Debug("Initializing Filter StorageRoot");
                var storageRoot = new GameObject("FilterStorageRoot");
                storageRoot.transform.SetParent(mono.transform, false);
                _containerRoot = storageRoot.AddComponent<ChildObjectIdentifier>();
            }

            _batteryContainer = new ItemsContainer(ContainerWidth, ContainerHeight, _containerRoot.transform,
                ARSSeaBreezeFCS32Buildable.StorageLabel(), null);

            _batteryContainer.isAllowedToAdd += IsAllowedToAdd;
            _batteryContainer.isAllowedToRemove += IsAllowedToRemove;

            _batteryContainer.onAddItem += mono.OnAddItemEvent;
            _batteryContainer.onRemoveItem += mono.OnRemoveItemEvent;


            //Add to the box colldier
            var handTarget = gameObject.AddComponent<GenericHandTarget>();
            handTarget.onHandHover = new HandTargetEvent();
            handTarget.onHandClick = new HandTargetEvent();
            handTarget.onHandHover.AddListener(OnPowerCellHandHover);
            handTarget.onHandClick.AddListener(OnPowerCellHandClick);
        }

        public int ContainerHeight { get; set; }

        public int ContainerWidth { get; set; }


        private void OnPowerCellHandHover(HandTargetEventData eventData)
        {
            HandReticle main = HandReticle.main;
            main.SetIcon(HandReticle.IconType.Hand);

            var secondText = "No Power Cell";
            //var powerCell = equipment.GetItemInSlot(slotOneText);
            //if (powerCell != null)
            //{
            //    var battery = powerCell.item.GetComponent<IBattery>();
            //    secondText = string.Format("Power {0}%", Mathf.RoundToInt((battery.charge / battery.capacity) * 100));
            //}

            main.SetInteractTextRaw("Power Cell", secondText);
        }

        private void OnPowerCellHandClick(HandTargetEventData eventData)
        {
            PDA pda = Player.main.GetPDA();
            if (!pda.isInUse)
            {
                //Inventory.main.SetUsedStorage(equipment, false);
                pda.Open(PDATab.Inventory, transform, null, 4f);
            }
        }

        private bool IsAllowedToAdd(Pickupable pickupable, bool verbose)
        {
            return pickupable != null && _compatibleTech.Contains(pickupable.GetTechType());
        }

        public float GetPower()
        {
            return 100f;
        }

        public float GetMaxPower()
        {
            return 4000f;
        }

        public bool ModifyPower(float amount, out float modified)
        {
            throw new NotImplementedException();
        }

        public bool HasInboundPower(IPowerInterface powerInterface)
        {
            return false;
        }

        public bool GetInboundHasSource(IPowerInterface powerInterface)
        {
            return false;
        }

        public bool CanDeconstruct(out string reason)
        {
            reason = String.Empty;
            return true;
        }

        private IEnumerator UpdatePowerRelay()
        {
            QuickLogger.Debug("In UpdatePowerRelay found at last!");

            var i = 1;

            while (_connectedRelay == null)
            {
                QuickLogger.Debug($"Checking For Relay... Attempt {i}");

                PowerRelay relay = PowerSource.FindRelay(this.transform);
                if (relay != null && relay != _connectedRelay)
                {
                    _connectedRelay = relay;
                    QuickLogger.Debug("PowerRelay found at last!");
                }
                else
                {
                    _connectedRelay = null;
                }

                i++;
                yield return new WaitForSeconds(0.5f);
            }
        }
    }
}
