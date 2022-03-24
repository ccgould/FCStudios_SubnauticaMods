using FCS_AlterraHub.Helpers;
using FCS_EnergySolutions.Configuration;
using UnityEngine;

namespace FCS_EnergySolutions.Mods.WindSurfer.Mono
{
    internal class WindSurferPowerController : HandTarget,IHandTarget
    {
        private float _time;
        private float MaxTurbineEnergyPerSecond = 2.66f;
        private float _energyPerSec;
        private PowerSource _powerSource;
        private bool _isInitialized;
        private WindSurferController _mono;
        internal bool ProducingPower
        {
            get
            {
                var value = _mono != null;
                return value;
            }
        }

        internal void Initialize(WindSurferController mono)
        {
            if (_isInitialized) return;
            _mono = mono;
            _powerSource = GetComponent<PowerSource>();
            if (_powerSource == null)
            {
                Debug.LogError("WindSurfer could not find PowerSource", this);
                return;
            }
            _isInitialized = true;
        }

        private void Update()
        {
            if (this.ProducingPower)
            {
                ProducePower();
            }
        }

        private void ProducePower()
        {
            if (WorldHelpers.CheckIfPaused())
            {
                return;
            }

            if (GameModeUtils.RequiresPower())
            {
                _time -= DayNightCycle.main.deltaTime;

                if (_time <= 0)
                {
                    _energyPerSec = MaxTurbineEnergyPerSecond;
                    _powerSource.AddEnergy(_energyPerSec, out float num2);
                    _time = 1f;
                }

            }
        }

        public float GetEnergyProducing()
        {
            return _energyPerSec;
        }

        public float GetStoredPower()
        {
            return _powerSource.power;
        }

        public float GetMaxPower()
        {
            return _powerSource.maxPower;
        }

        public void OnHandHover(GUIHand hand)
        {
            HandReticle main = HandReticle.main;

            var mainString = Language.main.GetFormat(AuxPatchers.WindSurferOnHover(), _mono.UnitID,
                Mathf.RoundToInt(this._powerSource.GetPower()), Mathf.RoundToInt(this._powerSource.GetMaxPower()),
                (_energyPerSec * 60).ToString("N1"));

            var subString = $"For more information press {FCS_AlterraHub.QPatch.Configuration.PDAInfoKeyCode}";

#if SUBNAUTICA
            main.SetInteractTextRaw(mainString, subString);
#else
            main.SetTextRaw(HandReticle.TextType.Use, mainString);
            main.SetTextRaw(HandReticle.TextType.UseSubscript, subString);
#endif
            main.SetIcon(HandReticle.IconType.Info);
        }

        public void OnHandClick(GUIHand hand)
        {
        }
    }
}