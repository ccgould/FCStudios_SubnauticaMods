using FCS_AlterraHub.Core.Helpers;
using FCS_EnergySolutions.ModItems.Buildables.SolarCluster.Patches;
using UnityEngine;

namespace FCS_EnergySolutions.ModItems.Buildables.SolarCluster.Mono;

internal class SolarClusterMovementManager : MonoBehaviour
{
    /// <summary>
    /// The transform to rotate around the X axis (elevate) when aiming.
    /// </summary>
    [SerializeField] private Transform _elevator;

    /// <summary>
    /// The transform to rotate around the Y axis (rotate) when aiming.
    /// </summary>
    [SerializeField] private Transform _rotator;

    private Transform _myTarget;

    private const float MaxPitch = 323f;
    private const float MinPitch = 90f;

    private float _targetPitch;

    private float _targetYaw;

    private float _currentPitch;

    private float _currentYaw;

    private SolarClusterController _mono;

    private void Awake()
    {
        _mono = gameObject.GetComponent<SolarClusterController>();
    }

    public void LookAt(Vector3 point)
    {
        Vector3 eulerAngles = Quaternion.LookRotation(transform.InverseTransformDirection(Vector3.Normalize(point - transform.position))).eulerAngles;
        float num = eulerAngles.x;
        if (num > MaxPitch)
        {
            num = Mathf.Clamp(num, MinPitch, MaxPitch);
        }
        _targetPitch = num;
        _targetYaw = eulerAngles.y;
    }

    private void Update()
    {
        if (_mono != null && _mono.IsOperational() && uSkyManager.main?.SunDir != null && Player_Patches.SunTarget != null)
        {
            if (_myTarget == null)
            {
                _myTarget = Player_Patches.SunTarget.transform;
            }


            if (_myTarget)
            {
                LookAt(_myTarget.transform.position);
            }

            _currentYaw = Mathf.LerpAngle(_currentYaw, _targetYaw, Time.deltaTime * 2f);
            _currentPitch = Mathf.LerpAngle(_currentPitch, _targetPitch, Time.deltaTime * 2f);
            _rotator.localEulerAngles = new Vector3(0f, _currentYaw, 0f);
            _elevator.localEulerAngles = new Vector3(_currentPitch, 0f, 0f);

        }
    }
}
