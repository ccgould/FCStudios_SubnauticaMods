using System.Collections.Generic;
using FCSCommon.Helpers;
using FCSCommon.Utilities;
using FCSTechFabricator.Enums;
using UnityEngine;

namespace FCS_HydroponicHarvesters.Mono
{
    internal class HydroHarvEffectManager : MonoBehaviour
    {
        private HydroHarvController _mono;
        private readonly List<GameObject> _bubbles = new List<GameObject>();
        private GameObject _vaporFx;
        private bool PowerAvailable => _mono.PowerManager.HasPowerToConsume();
        
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
            _vaporFx = GameObjectHelpers.FindGameObject(gameObject, "PressurisedSteam_FCS");
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

        private void ToggleVapor(bool state)
        {
            if (state != _vaporFx?.activeSelf)
            {
                _vaporFx?.SetActive(state);
            }

        }

        private void Update()
        {
            if (_mono == null || !_mono.IsConstructed || !_mono.IsInitialized) return;

            
            if (PowerAvailable)
            {
                //Handle Spray Firing
                ToggleVapor(_mono.HydroHarvGrowBed.GetBedType() == FCSEnvironment.Air);

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
            FindBubbles();
            FindVaporBlast();
        }
    }
}
