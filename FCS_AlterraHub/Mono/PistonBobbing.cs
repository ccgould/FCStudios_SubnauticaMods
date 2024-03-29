﻿using UnityEngine;

namespace FCS_AlterraHub.Mono
{
    public class PistonBobbing : MonoBehaviour
    {
        // User Inputs
        public float amplitude = -0.1f;//0.5f;
        public float frequency = 3.39f;//1f;

        // Position Storage Variable
        private Vector3 originalLocalPosition;
        [SerializeField]
        private bool _isRunning;
        public bool Invert { get; set; }

        // Use this for initialization
        private void Start()
        {
            // Store the starting position of the object relative to its parent.
            originalLocalPosition = transform.localPosition;
        }

        public void SetState(bool isRunning)
        {
            _isRunning = isRunning;
        }

        // Update is called once per frame  
        private void Update()
        {
            if (!_isRunning) return;
            // Calculate offset
            // Even though Transform.up is relative, it is in world space
            // That means that you can't apply it to localPosition
            //
            // Vector3.up could be used along with localPosition, but that would ignore the object's rotation

            Vector3 globalOffset = transform.up * Mathf.Sin(Time.time * Mathf.PI * frequency) * amplitude;
            
            // Reset the position to the original
            transform.localPosition = originalLocalPosition;

            // Apply offset
            if (!Invert)
            {
                transform.position += globalOffset;
            }
            else
            {
                transform.position -= globalOffset;
            }
        }
    }
}
