using System;
using FCS_AlterraHomeSolutions.Mono.PaintTool;
using FCSCommon.Helpers;
using FCSCommon.Utilities;
using UnityEngine;

namespace FCS_AlterraHub.Mono
{
    public class ColorManager : MonoBehaviour
    {
        private string _bodyMaterial;
        private GameObject _gameObject;
        private readonly Color _defaultColor = new Color(0.7529412f, 0.7529412f, 0.7529412f, 1f);
        public Action<Color> OnColorChanged;
        private string _bodySecondary;

        public void Initialize(GameObject gameObject, string bodyMaterial, string secondaryMaterial = "")
        {
            _gameObject = gameObject;
            _bodyMaterial = bodyMaterial;
            _bodySecondary = secondaryMaterial;
        }

        public Color GetColor()
        {
            var color = MaterialHelpers.GetBodyColor(_gameObject, _bodyMaterial);
            if (color == null)
            {
                QuickLogger.Error("Color returned null on Get Color setting default color");
                return _defaultColor;
            }
            return (Color)color;
        }

        public void ChangeColor(Color color,ColorTargetMode mode = ColorTargetMode.Primary)
        {
            switch (mode)
            {
                case ColorTargetMode.Primary:
                    MaterialHelpers.ChangeMaterialColor(_bodyMaterial, _gameObject, color);
                    break;
                case ColorTargetMode.Secondary:
                    if(!string.IsNullOrWhiteSpace(_bodySecondary))
                        MaterialHelpers.ChangeMaterialColor(_bodySecondary, _gameObject, color);
                    break;
                case ColorTargetMode.Both:
                    MaterialHelpers.ChangeMaterialColor(_bodyMaterial, _gameObject, color);
                    if (!string.IsNullOrWhiteSpace(_bodySecondary))
                        MaterialHelpers.ChangeMaterialColor(_bodySecondary, _gameObject, color);
                    break;
            }

            OnColorChanged?.Invoke(color);
        }
    }
}
