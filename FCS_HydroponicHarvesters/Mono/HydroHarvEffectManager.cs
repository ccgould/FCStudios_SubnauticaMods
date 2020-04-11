using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
        private List<GameObject> _bubbles = new List<GameObject>();
        private VFXController _vaporFX;
        private bool _fireVapor;
        private bool PowerAvaliable => _mono.PowerManager.HasPowerToConsume();
        private float _timeLeft = 1;
        internal void Initialize(HydroHarvController mono)
        {
            _mono = mono;
            _sprayTime = SprayTimeDefault;
            FindBubbles();
            FindVaporBlast();
        }

        private Transform FindObject(Transform transform, string value)
        {
            foreach (Transform child in transform)
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
            _vaporFX = FindObject(gameObject.transform, "MaterialEmitter")?.GetComponent<VFXController>();
            if (_vaporFX != null)
            {
                _vaporFX.emitters[1] = null;
                _vaporFX.gameObject.SetActive(true);
            }
        }

        private void ShowBubbles()
        {
            if (!PowerAvaliable) return;

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
           _vaporFX.Play(0);
            _fireVapor = true;
        }

        private void Update()
        {
            if (_mono == null || !_mono.IsConstructed || !_mono.IsInitialized) return;

            
            if (PowerAvaliable)
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
                       _vaporFX.Stop(0);
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
    }
}
