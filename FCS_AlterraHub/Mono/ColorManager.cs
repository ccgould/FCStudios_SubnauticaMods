using System;
using FCS_AlterraHomeSolutions.Mono.PaintTool;
using FCS_AlterraHub.Objects;
using FCSCommon.Helpers;
using FCSCommon.Utilities;
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
                    break;

                case ColorTargetMode.Secondary:
                    result = !string.IsNullOrWhiteSpace(_bodySecondary) && MaterialHelpers.ChangeMaterialColor(_bodySecondary, _gameObject, color);
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

                    break;
            }

            _currentColor = color;
            OnColorChanged?.Invoke(color);
            return result;
        }
    }
}
