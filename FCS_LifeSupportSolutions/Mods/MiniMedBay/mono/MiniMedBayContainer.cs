using FCS_AlterraHub.Helpers;
using FCS_LifeSupportSolutions.Configuration;
using FCSCommon.Utilities;
using UnityEngine;

namespace FCS_LifeSupportSolutions.Mods.MiniMedBay.mono
{
    internal class MiniMedBayContainer : MonoBehaviour
    {

        private float _timeSpawnMedKit = -1f;
        public const int MaxContainerSlots = 6;
        internal bool IsContainerFull => _medKits >= MaxContainerSlots;
        private const float MedKitSpawnInterval = 600f;
        internal float Progress { get; set; }
        internal bool StartWithMedKit;
        private int _medKits;
        private MiniMedBayController _mono;

        internal int NumberOfFirstAids
        {
            get => _medKits;
            set
            {
                if (value < 0 || value > MaxContainerSlots)
                    return;

                if (value < _medKits)
                {
                    do
                    {
                        RemoveSingleKit();
                    } while (value < _medKits);
                }
                else if (value > _medKits)
                {
                    do
                    {
                        SpawnKit();
                    } while (value > _medKits);
                }
            }
        }

        internal void RemoveSingleKit()
        {
            var size = CraftData.GetItemSize(TechType.FirstAidKit);
            if (Inventory.main.HasRoomFor(size.x, size.y))
            {
                if (_medKits > 0)
                {
                    _medKits--;
                    PlayerInteractionHelper.GivePlayerItem(TechType.FirstAidKit);
                }
                else
                {
                    QuickLogger.ModMessage(AuxPatchers.NoMedKitsToTake());
                }
            }
            else
            {
                QuickLogger.ModMessage(AuxPatchers.NoInventorySpace());
            }

            NotifyDisplay();
        }

        private void SpawnKit()
        {
            _medKits++;
            _timeSpawnMedKit = DayNightCycle.main.timePassedAsFloat + MedKitSpawnInterval;
            NotifyDisplay();
        }

        private void NotifyDisplay()
        {
            _mono.DisplayManager.UpdateDispenserCount(_medKits);
        }

        internal void Initialize(MiniMedBayController mono)
        {
            _mono = mono;

            DayNightCycle main = DayNightCycle.main;

            if (_timeSpawnMedKit < 0.0 && main)
            {
                _timeSpawnMedKit = (float)(main.timePassed + (!StartWithMedKit ? MedKitSpawnInterval : 0.0));
            }
        }

        private void Update()
        {
            if (!_mono.IsOperational) return;

            if (IsContainerFull)
            {
                Progress = 0f;
                _timeSpawnMedKit = DayNightCycle.main.timePassedAsFloat + MedKitSpawnInterval;
                return;
            }

            DayNightCycle main = DayNightCycle.main;

            float a = _timeSpawnMedKit - MedKitSpawnInterval;
            Progress = Mathf.InverseLerp(a, a + MedKitSpawnInterval, DayNightCycle.main.timePassedAsFloat);

            if (main.timePassed > _timeSpawnMedKit)
            {
                NumberOfFirstAids++;
            }
        }
        
        private void RemoveItem()
        {
            QuickLogger.Debug("Resetting Time", true);
            _timeSpawnMedKit = DayNightCycle.main.timePassedAsFloat + MedKitSpawnInterval;
        }
        
        public bool GetIsEmpty()
        {
            return _medKits == 0;
        }

        internal float GetTimeToSpawn()
        {
            return _timeSpawnMedKit;
        }

        internal void SetTimeToSpawn(float value)
        {
            _timeSpawnMedKit = value;
        }
    }
}
