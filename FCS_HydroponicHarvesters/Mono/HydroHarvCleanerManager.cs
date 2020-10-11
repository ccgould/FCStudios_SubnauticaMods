using System;
using System.Collections.Generic;
using FCS_HydroponicHarvesters.Buildables;
using FCSCommon.Converters;
using FCSCommon.Utilities;
using FCSTechFabricator.Components;
using FCSTechFabricator.Interfaces;
using UnityEngine;

namespace FCS_HydroponicHarvesters.Mono
{
    internal class HydroHarvCleanerManager : MonoBehaviour, IFCSStorage
    {
        private HydroHarvController _mono;
        private const float ThirtyDays = 63000f;
        private float _unitSanitation = 63000f;
        private bool _isDirty;
        public int GetContainerFreeSpace { get; }
        public bool IsFull { get; }
        
        
        private void Update()
        {
            if(_mono == null || !_mono.IsInitialized || !_mono.IsConstructed || !_mono.PowerManager.HasPowerToConsume()) return;

            Desterilize();
        }

        private void Desterilize()
        {
            if (_isDirty) return;

            if (QPatch.Configuration.Config.GetsDirtyOverTime)
            {
                _unitSanitation -= DayNightCycle.main.deltaTime;
                if (_unitSanitation <= 0)
                {
                    QuickLogger.Debug("Unit is dirty");
                    _isDirty = true;
                    _unitSanitation = ThirtyDays;
                }
            }

            _mono.DisplayManager.UpdateTimeLeft(TimeConverters.SecondsToHMS(_unitSanitation));
        }

        private void Sanitize()
        {
            QuickLogger.Debug($"Sanitizing {_mono.PrefabId.Id}", true);
            if (_isDirty)
            {
                _isDirty = false;
                _unitSanitation = ThirtyDays;
            }
        }

        internal void Initialize(HydroHarvController mono)
        {
            _mono = mono;
        }
        
        public bool CanBeStored(int amount,TechType techType = TechType.None)
        {
            return false;
        }

        internal bool GetIsDirty()
        {
            return QPatch.Configuration.Config.GetsDirtyOverTime && _isDirty;
        }

        public bool AddItemToContainer(InventoryItem item)
        {
            Sanitize();
            Destroy(item.item);
            return true;
        }
        
        public bool IsAllowedToAdd(Pickupable pickupable, bool verbose)
        {
            QuickLogger.Debug(
                $"Current TechType: {pickupable.GetTechType()} || FlorKleen TechType: {QPatch.FloraKleen.TechType}");

            if (pickupable.GetTechType() == QPatch.FloraKleen.TechType)
            {
                if (_isDirty)
                {
                    return true;
                }

                QuickLogger.Message(HydroponicHarvestersBuildable.UnitIsntDirty(), true);
            }
            else
            {
                QuickLogger.Message(HydroponicHarvestersBuildable.NotAllowedItem(), true);
            }
            
            return false;
        }

        public bool IsAllowedToRemoveItems()
        {
            return false;
        }

        public Pickupable RemoveItemFromContainer(TechType techType, int amount)
        {
            throw new NotImplementedException();
        }

        public Dictionary<TechType, int> GetItemsWithin()
        {
            throw new NotImplementedException();
        }

        public Action<int, int> OnContainerUpdate { get; set; }
        public Action<FCSConnectableDevice, TechType> OnContainerAddItem { get; set; }
        public Action<FCSConnectableDevice, TechType> OnContainerRemoveItem { get; set; }

        public bool ContainsItem(TechType techType)
        {
            throw new NotImplementedException();
        }

        internal void Load(float savedDataUnitSanitation)
        {
            QuickLogger.Debug($"Setting UnitSanitation {_unitSanitation} to {savedDataUnitSanitation}");
            _unitSanitation = savedDataUnitSanitation;
            QuickLogger.Debug($"Set UnitSanitation {_unitSanitation}");
        }

        internal float GetUnitSanitation()
        {
            return _unitSanitation;
        }
    }
}
