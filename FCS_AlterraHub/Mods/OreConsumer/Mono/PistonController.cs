using System;
using System.Collections;
using UnityEngine;

namespace FCS_AlterraHub.Mods.OreConsumer.Mono
{
    public class PistonBuildUpMotion : MonoBehaviour
    {
        private float _percentage;
        private float _countDownSeconds = 0.2f;    
        private float _timeMultiplier;

        // Position Storage Variable
        private Vector3 originalLocalPosition;
        [SerializeField]
        private bool _isRunning = true;

        private bool _isReleasing;
        public Action OnPressureReleased { get; set; }

        // Use this for initialization
        private void Start()
        {
            // Store the starting position of the object relative to its parent.
            originalLocalPosition = transform.localPosition;
            _timeMultiplier = 1 / _countDownSeconds;
        }

        public void SetState(bool isRunning)
        {
            _isRunning = isRunning;
        }

        // Update is called once per frame
        private void Update()
        {
            if (!_isRunning) return;

            if (_isReleasing)
            {
                _percentage -= Time.deltaTime * _timeMultiplier; //Count

                if (_percentage < 0)
                {
                    _percentage = 0f;
                }

                // Calculate offset
                // Even though Transform.up is relative, it is in world space
                // That means that you can't apply it to localPosition
                //
                // Vector3.up could be used along with localPosition, but that would ignore the object's rotation

                Vector3 globalOffset = transform.up * _percentage;

                // Reset the position to the original
                transform.localPosition = originalLocalPosition;

                // Apply offset
                transform.position += globalOffset;

                if (!(_percentage <= 0)) return;
                OnPressureReleased?.Invoke();
                _isReleasing = false;
                //StartCoroutine(PlayerSteamSFX());
            }
            else
            {
                // Calculate offset
                // Even though Transform.up is relative, it is in world space
                // That means that you can't apply it to localPosition
                //
                // Vector3.up could be used along with localPosition, but that would ignore the object's rotation

                Vector3 globalOffset = transform.up * _percentage;

                // Reset the position to the original
                transform.localPosition = originalLocalPosition;

                // Apply offset
                transform.position += globalOffset;
            }


        }

        private IEnumerator PlayerSteamSFX()
        {
            //_extinguisherSound.Play();
            yield return new WaitForSeconds(1);
            //_extinguisherSound.Stop();
        }

        public void SetPercentage(float amount)
        {
            _percentage = amount;

            if (amount >= 1)
            {
                StartCoroutine(ReleasePressure());
            }
        }

        private IEnumerator ReleasePressure()
        {
            _isRunning = false;
            yield return new WaitForSeconds(2);
            _isRunning = true;
            _isReleasing = true;
            yield break;
        }
    }
}