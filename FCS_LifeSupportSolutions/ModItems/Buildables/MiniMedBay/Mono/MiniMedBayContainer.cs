using FCS_AlterraHub.Core.Helpers;
using FCSCommon.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace FCS_LifeSupportSolutions.ModItems.Buildables.MiniMedBay.Mono;
internal class MiniMedBayContainer : MonoBehaviour
{

    private float _timeSpawnMedKit = -1f;
    public const int MaxContainerSlots = 6;
    internal bool IsContainerFull => _medKits >= MaxContainerSlots;
    private const float MedKitSpawnInterval = 600f;
    internal float Progress { get; set; }
    internal bool StartWithMedKit;
    private int _medKits;
    private MiniMedBayController _controller;
    public Action<int> OnKitAmountChanged;

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
        var size = TechData.GetItemSize(TechType.FirstAidKit);

        if (Inventory.main.HasRoomFor(size.x, size.y))
        {
            if (_medKits > 0)
            {
                _medKits--;
                PlayerInteractionHelper.GivePlayerItem(TechType.FirstAidKit);
            }
            else
            {
                QuickLogger.ModMessage(Language.main.Get("ALS_NoMedKitsToTake"));
            }
        }
        else
        {
            QuickLogger.ModMessage(Language.main.Get("InventoryFull"));
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
        OnKitAmountChanged?.Invoke(_medKits);
    }

    private void Awake()
    {
        _controller = gameObject.GetComponent<MiniMedBayController>();

        DayNightCycle main = DayNightCycle.main;

        if (_timeSpawnMedKit < 0.0 && main)
        {
            _timeSpawnMedKit = (float)(main.timePassed + (!StartWithMedKit ? MedKitSpawnInterval : 0.0));
        }
    }

    private void Update()
    {
        if (!_controller.IsOperational()) return;

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
