using UnityEngine;

namespace FCS_ProductionSolutions.MatterAnalyzer.Mono
{
    internal class ScannerController : MonoBehaviour
    {
        private AnimationCurve _animationCurve;
        private float _percentage;

        private void Start()
        {
            _animationCurve = new AnimationCurve(new Keyframe(0, 1.313115f), new Keyframe(1, 0.845f));
        }

        private void Update()
        {
            var location = _animationCurve.Evaluate(_percentage);
            gameObject.transform.localPosition = new Vector3(gameObject.transform.localPosition.x,location, gameObject.transform.localPosition.z);
        }

        internal void SetPercentage(float percentage)
        {
            _percentage = percentage;
        }
    }
}