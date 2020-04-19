using System.Collections.Generic;
using FCSCommon.Utilities;
using FCSTechFabricator.Enums;
using UnityEngine;

namespace FCS_HydroponicHarvesters.Mono
{
    internal class HydroHarvEffectManager : MonoBehaviour
    {
        private HydroHarvController _mono;
        private const float SprayTimeDefault = 180f;
        private float _sprayTime;
        private readonly List<GameObject> _bubbles = new List<GameObject>();
        private VFXController _vaporFx;
        private bool _fireVapor;
        private bool PowerAvailable => _mono.PowerManager.HasPowerToConsume();
        private float _timeLeft = 1;

        private static Transform FindObject(Transform tr, string value)
        {
            foreach (Transform child in tr)
            {
                if (child.name.StartsWith(value))
                {
                    return child;
                }

                FindObject(child, value);
            }

            return null;
        }

        private void FindBubbles()
        {
            foreach (Transform transform in gameObject.transform)
            {
                if (transform.name.Trim().ToLower().StartsWith("xlateralbubbles"))
                {
                    _bubbles.Add(transform.gameObject);
                }
            }
        }

        private void FindVaporBlast()
        {
            _vaporFx = FindObject(gameObject.transform, "MaterialEmitter")?.GetComponent<VFXController>();
            if (_vaporFx != null)
            {
                _vaporFx.emitters[1] = null;
                _vaporFx.gameObject.SetActive(true);
            }
        }

        private void ShowBubbles()
        {
            if (!PowerAvailable) return;

            foreach (GameObject bubble in _bubbles)
            {
                if (!bubble.activeSelf)
                {
                    QuickLogger.Debug("Showing Bubbles");
                    bubble.SetActive(true);
                }
            }
        }

        private void HideBubbles()
        {
            foreach (GameObject bubble in _bubbles)
            {
                if (bubble.activeSelf)
                {
                    QuickLogger.Debug("Hiding Bubbles");
                    bubble.SetActive(false);
                }
            }
        }

        private void FireSpray()
        {
            QuickLogger.Debug($"Firing {_mono?.PrefabId?.Id} Spray",true);
           _vaporFx.Play(0);
            _fireVapor = true;
        }

        private void Update()
        {
            if (_mono == null || !_mono.IsConstructed || !_mono.IsInitialized) return;

            
            if (PowerAvailable)
            {
                //Handle Spray Firing
                _sprayTime -= DayNightCycle.main.deltaTime;
                if (_sprayTime < 0)
                {
                    if (_mono.HydroHarvGrowBed.GetBedType() == FCSEnvironment.Air)
                    {
                        FireSpray();
                        _sprayTime = SprayTimeDefault;
                    }
                }

                if (_fireVapor)
                {
                    _timeLeft -= DayNightCycle.main.deltaTime;
                    if (_timeLeft < 0)
                    {
                       _vaporFx.Stop(0);
                        _fireVapor = false;
                        _timeLeft = 1f;
                    }
                }

                //Handle Bubbles
                if (_mono.HydroHarvGrowBed.GetBedType() == FCSEnvironment.Water)
                {
                    ShowBubbles();
                }
                else
                {
                    HideBubbles();
                }
            }
            else
            {
                //HandleBubbles
                HideBubbles();
            }
        }

        internal void Initialize(HydroHarvController mono)
        {
            _mono = mono;
            _sprayTime = SprayTimeDefault;
            FindBubbles();
            FindVaporBlast();
        }
    }
}
