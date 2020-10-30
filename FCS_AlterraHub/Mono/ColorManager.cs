using System;
using FCS_AlterraHomeSolutions.Mono.PaintTool;
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

        public void Initialize(GameObject gameObject, string bodyMaterial, string secondaryMaterial = "")
        {
            _gameObject = gameObject;
            _bodyMaterial = bodyMaterial;
            _bodySecondary = secondaryMaterial;
        }

        public Color GetColor()
        {
            return _currentColor;
        }

        public bool ChangeColor(Color color,ColorTargetMode mode = ColorTargetMode.Primary)
        {
            bool result = false;
            switch (mode)
            {
                case ColorTargetMode.Primary:
                    result =  MaterialHelpers.ChangeMaterialColor(_bodyMaterial, _gameObject, color);
                    _currentColor = color;
                    break;

                case ColorTargetMode.Secondary:
                    result = !string.IsNullOrWhiteSpace(_bodySecondary) && MaterialHelpers.ChangeMaterialColor(_bodySecondary, _gameObject, color);
                    _currentSecondaryColor = color;
                    break;
                    
                case ColorTargetMode.Both:
                    result =  MaterialHelpers.ChangeMaterialColor(_bodyMaterial, _gameObject, color);
                    
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
            }

            
            OnColorChanged?.Invoke(color);
            return result;
        }

        public Color GetSecondaryColor()
        {
            return _currentSecondaryColor;
        }
    }
}
