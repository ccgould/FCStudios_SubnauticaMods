using FCS_AlterraHub.Core.Helpers;
using System;
using UnityEngine;

namespace FCS_ProductionSolutions.ModItems.Buildables.IonCubeGenerator.Mono
{
    internal class CubeGeneratorPulleyController : MonoBehaviour
    {
        private GameObject _cube;
        private AnimationCurve _startUpAnimationCurve;
        private IonCubeGeneratorController _controller;
        private bool _isInitialize;
        private AnimationCurve _coolDownAnimationCurve;

        private void Awake()
        {
            _cube = GameObjectHelpers.FindGameObject(gameObject, "IonCube");
            _startUpAnimationCurve = new AnimationCurve();
            _startUpAnimationCurve.AddKey(0, 0.773f);
            _startUpAnimationCurve.AddKey(1, 1.710066f);

            _coolDownAnimationCurve = new AnimationCurve();
            _coolDownAnimationCurve.AddKey(0, 1.710066f);
            _coolDownAnimationCurve.AddKey(1, 0.773f);
        }

        private void Update()
        {
            if (!_isInitialize) return;
            UpdatePulleyPosition();
        }

        private void UpdatePulleyPosition()
        {
            if(_controller.StartUpPercent > 0)
            {
                _cube.transform.localPosition = new Vector3(_cube.transform.localPosition.x, _startUpAnimationCurve.Evaluate(_controller.StartUpPercent), _cube.transform.localPosition.z);

            }

            if (_controller.CoolDownPercent > 0)
            {
                _cube.transform.localPosition = new Vector3(_cube.transform.localPosition.x, _coolDownAnimationCurve.Evaluate(_controller.CoolDownPercent), _cube.transform.localPosition.z);
            }
        }

        internal void Initialize(IonCubeGeneratorController controller)
        {
            _controller = controller;

            _isInitialize = true;
        }
    }
}
