using System;
using FCS_AlterraHomeSolutions.Mono.PaintTool;
using FCS_AlterraHub.Helpers;
using FCSCommon.Helpers;
using UnityEngine;

namespace FCS_AlterraHub.Mono
{
    public class ColorManager : MonoBehaviour
    {
        private string _bodyMaterial;
        private GameObject _gameObject;
        public Action<Color> OnColorChanged;
        private string _bodySecondary;
        private Color _currentColor = Color.white;
        private Color _currentSecondaryColor = Color.white;
        private Color _currentLumColor = Color.white;
        private string _lumMaterial;
        

        public void Initialize(GameObject gameObject, string bodyMaterial, string secondaryMaterial = "",
            string lumControllerMaterial = "")
        {
            _gameObject = gameObject;
            _bodyMaterial = bodyMaterial;
            _bodySecondary = secondaryMaterial;
            _lumMaterial = lumControllerMaterial;
        }

        public Color GetColor()
        {
            return _currentColor;
        }

        public Color GetColor(ColorTargetMode mode)
        {
            switch (mode)
            {
                case ColorTargetMode.Primary:
                case ColorTargetMode.Both:
                    return _currentColor;
                case ColorTargetMode.Secondary:
                    return _currentSecondaryColor;
                case ColorTargetMode.Emission:
                    return _currentLumColor;
            }
            return _currentColor;
        }

        public bool ChangeColor(Color color,ColorTargetMode mode = ColorTargetMode.Primary)
        {
            bool result = false;

            //if (GetColor(mode) == color) return result; //Disabled to allow paint to be used even when the object already has the color.
            switch (mode)
            {
                case ColorTargetMode.Primary:
                    result = MaterialHelpers.ChangeMaterialColor(_bodyMaterial, _gameObject, color);
                    _currentColor = color;
                    break;

                case ColorTargetMode.Secondary:
                    result = !string.IsNullOrWhiteSpace(_bodySecondary) && MaterialHelpers.ChangeMaterialColor(_bodySecondary, _gameObject, color);
                    _currentSecondaryColor = color;
                    break;

                case ColorTargetMode.Both:
                    result = MaterialHelpers.ChangeMaterialColor(_bodyMaterial, _gameObject, color);

                    if (!string.IsNullOrWhiteSpace(_bodySecondary))
                    {

                        var secResult = MaterialHelpers.ChangeMaterialColor(_bodySecondary, _gameObject, color);
                        if (secResult)
                        {
                            result = secResult;
                        }
                    }

                    _currentColor = color;
                    _currentSecondaryColor = color;

                    break;

                case ColorTargetMode.Emission:

                    if (!string.IsNullOrWhiteSpace(_lumMaterial) || _currentLumColor != color)
                    {
                        var emissionResult = MaterialHelpers.ChangeEmissionColor(_lumMaterial, _gameObject, color);
                        _currentLumColor = color;
                        OnColorChanged?.Invoke(color);
                        return true;
                    }
                    break;
            }

            OnColorChanged?.Invoke(color);
            return result;
        }

        public Color GetSecondaryColor()
        {
            return _currentSecondaryColor;
        }

        public Color GetLumColor()
        {
            return _currentLumColor;
        }
    }
}
