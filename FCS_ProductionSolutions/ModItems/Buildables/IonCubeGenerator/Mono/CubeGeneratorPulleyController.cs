using FCS_ProductionSolutions.ModItems.Buildables.IonCubeGenerator.Mono.StateMachine;
using UnityEngine;
using static FCS_ProductionSolutions.ModItems.Buildables.IonCubeGenerator.Mono.StateMachine.CubeGeneratorStateManager;

namespace FCS_ProductionSolutions.ModItems.Buildables.IonCubeGenerator.Mono;

internal class CubeGeneratorPulleyController : MonoBehaviour
{
    [SerializeField]
    private GameObject _ionCube;
    [SerializeField]
    private AnimationCurve _startUpAnimationCurve;
    [SerializeField]
    private IonCubeGeneratorController _controller;
    [SerializeField]
    private AnimationCurve _coolDownAnimationCurve;

    private CubeGeneratorStateManager _stateManager;

    private void Awake()
    {
        _stateManager = GetComponent<CubeGeneratorStateManager>();
    }

    private void Update()
    {
        UpdatePulleyPosition();
    }

    private void UpdatePulleyPosition()
    {
        if(_stateManager.IsStartingUp())
        {
            _ionCube.transform.localPosition = new Vector3(_ionCube.transform.localPosition.x, _startUpAnimationCurve.Evaluate(_stateManager.GetState(CubeGeneratorStates.StartUp).GetProgressNormalized()), _ionCube.transform.localPosition.z);

        }

        if (_stateManager.IsCoolingDown())
        {
            _ionCube.transform.localPosition = new Vector3(_ionCube.transform.localPosition.x, _coolDownAnimationCurve.Evaluate(_stateManager.GetState(CubeGeneratorStates.CoolDown).GetProgressNormalized()), _ionCube.transform.localPosition.z);
        }
    }
}
