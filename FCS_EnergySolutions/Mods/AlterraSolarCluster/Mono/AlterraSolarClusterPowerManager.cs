using FCS_AlterraHub.Helpers;
using FCS_AlterraHub.Mods.FCSPDA.Mono;
using FCS_EnergySolutions.Configuration;
using UnityEngine;

namespace FCS_EnergySolutions.Mods.AlterraSolarCluster.Mono
{
    internal class AlterraSolarClusterPowerManager : HandTarget, IHandTarget
    {
        private PowerSource _powerSource;
        private const float MaxDepth = 200f;
        private AnimationCurve _depthCurve;
        private AlterraSolarClusterController _mono;
        private float _powerCal;


        internal void Initialize(AlterraSolarClusterController mono)
        {
            _mono = mono;
            _powerSource = gameObject.GetComponent<PowerSource>();
            _depthCurve = new AnimationCurve();
            _depthCurve.AddKey(0f, 0f);
            _depthCurve.AddKey(0.4245796f, 0.5001081f);
            _depthCurve.AddKey(1f, 1f);
        }

        private float GetDepthScalar()
        {
            float time = Mathf.Clamp01((MaxDepth -
#if SUBNAUTICA
                                        Ocean.main.GetDepthOf(gameObject)
#else
                                        Ocean.GetDepthOf(gameObject)
#endif
                ) / MaxDepth);
            return _depthCurve.Evaluate(time);
        }

        private float GetSunScalar()
        {
            return DayNightCycle.main.GetLocalLightScalar();
        }

        private float GetRechargeScalar()
        {
            return GetDepthScalar() * GetSunScalar();
        }

        private void Update()
        {
            if (_mono != null && _mono.IsOperational && _powerSource != null)
            {
                _powerCal = (GetRechargeScalar() * DayNightCycle.main.deltaTime * 0.25f * 5f) * 13;
                _powerSource.power = Mathf.Clamp(_powerSource.power + _powerCal, 0f, _powerSource.maxPower);
            }
        }

        public void OnHandHover(GUIHand hand)
        {
            if (_mono != null && _mono.IsOperational)
            {
                var data = new[]
                {
                    AuxPatchers.SolarClusterHover(Mathf.RoundToInt(GetRechargeScalar() * 100f),
                        Mathf.RoundToInt(_powerSource.GetPower()), Mathf.RoundToInt(_powerSource.GetMaxPower()),
                        Mathf.RoundToInt((GetRechargeScalar() * 0.25f * 5f) * 13f)),
                    string.Empty
                };
                data.HandHoverPDAHelperEx(_mono.GetTechType());

                if (Input.GetKeyDown(FCS_AlterraHub.QPatch.Configuration.PDAInfoKeyCode))
                {
                    FCSPDAController.Main.OpenEncyclopedia(_mono.GetTechType());
                }
            }
        }

        public void OnHandClick(GUIHand hand)
        {
        }

        public float GetStoredPower()
        {
            return _powerSource.GetPower();
        }

        public void SetPower(float powerValue)
        {
            _powerSource.SetPower(powerValue);
        }

        public void CheckIfConnected()
        {
            if (_mono.Manager?.Habitat == null) return;
            if (!_mono.Manager.Habitat.powerRelay.inboundPowerSources.Contains(_powerSource))
            {
                _mono.Manager.Habitat.powerRelay.AddInboundPower(_powerSource);
            }
        }
    }
}