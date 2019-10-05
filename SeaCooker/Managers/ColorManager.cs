using AE.SeaCooker.Mono;
using FCSCommon.Helpers;
using UnityEngine;

namespace AE.SeaCooker.Managers
{
    internal class ColorManager
    {
        private Color _currentColor;
        private SeaCookerController _mono;
        private string _material;

        internal void Initialize(SeaCookerController mono, string colorMaterial)
        {
            _mono = mono;
            _material = colorMaterial;
        }
        public void SetCurrentBodyColor(Color color)
        {
            _currentColor = color;
            MaterialHelpers.ChangeMaterialColor(_material, _mono.gameObject, color);
        }

        internal Color GetColor()
        {
            return _currentColor;
        }
    }
}
