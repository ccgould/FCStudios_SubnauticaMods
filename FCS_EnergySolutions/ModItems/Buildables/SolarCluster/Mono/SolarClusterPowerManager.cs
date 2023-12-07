using System.Linq;
using UnityEngine;

namespace FCS_EnergySolutions.ModItems.Buildables.SolarCluster.Mono;
internal class SolarClusterPowerManager : MonoBehaviour
{
    [SerializeField] private PowerSource _powerSource;
    private const float MaxDepth = 200f;
    [SerializeField] private AnimationCurve _depthCurve;
    private SolarClusterController _mono;
    private float _powerCal;

    private void Awake()
    {
        _mono = GetComponent<SolarClusterController>();
    }

    private float GetDepthScalar()
    {
        float time = Mathf.Clamp01((MaxDepth - Ocean.GetDepthOf(gameObject)) / MaxDepth);
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
        if (_mono != null && _mono.IsOperational() && _powerSource != null)
        {
            _powerCal = (GetRechargeScalar() * DayNightCycle.main.deltaTime * 0.25f * 5f) * 13;
            _powerSource.power = Mathf.Clamp(_powerSource.power + _powerCal, 0f, _powerSource.maxPower);
        }
    }

    //public void OnHandHover(GUIHand hand)
    //{
    //    if (_mono != null && _mono.IsOperational())
    //    {
    //        var data = new[]
    //        {
    //                AuxPatchers.SolarClusterHover(Mathf.RoundToInt(GetRechargeScalar() * 100f),
    //                    Mathf.RoundToInt(_powerSource.GetPower()), Mathf.RoundToInt(_powerSource.GetMaxPower()),
    //                    Mathf.RoundToInt((GetRechargeScalar() * 0.20f/*0.25f old value */ * 5f) * 13f)),
    //                string.Empty
    //            };
    //        data.HandHoverPDAHelperEx(_mono.GetTechType());

    //        if (Input.GetKeyDown(FCS_AlterraHub.Main.Configuration.PDAInfoKeyCode))
    //        {
    //            //TODO V2 Fix
    //            //FCSPDAController.Main.OpenEncyclopedia(_mono.GetTechType());
    //        }
    //    }
    //}

    //public void OnHandClick(GUIHand hand)
    //{
    //}

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
        var habitat = _mono?.CachedHabitatManager?.GetSubRoot();

        if (habitat == null || habitat.powerRelay == null) return;


        if (!habitat.powerRelay.inboundPowerSources.Contains(_powerSource))
        {
            habitat.powerRelay.AddInboundPower(_powerSource);
        }
    }
}
