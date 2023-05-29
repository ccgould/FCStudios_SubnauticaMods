using UnityEngine;

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

    private void Update()
    {
        UpdatePulleyPosition();
    }

    private void UpdatePulleyPosition()
    {
        if(_controller.StartUpPercent > 0)
        {
            _ionCube.transform.localPosition = new Vector3(_ionCube.transform.localPosition.x, _startUpAnimationCurve.Evaluate(_controller.StartUpPercent), _ionCube.transform.localPosition.z);

        }

        if (_controller.CoolDownPercent > 0)
        {
            _ionCube.transform.localPosition = new Vector3(_ionCube.transform.localPosition.x, _coolDownAnimationCurve.Evaluate(_controller.CoolDownPercent), _ionCube.transform.localPosition.z);
        }
    }
}
