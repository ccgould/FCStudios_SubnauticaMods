using System;
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
        
        public void Initialize(GameObject gameObject, string bodyMaterial)
        {
            _gameObject = gameObject;
            _bodyMaterial = bodyMaterial;
        }

        public void SetColorFromSave(Color color) 
        {
            ChangeColor(color);
        }

        public void SetMaskColorFromSave(Color color)
        {
            ChangeColorMask(color);
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

        public Color GetMaskColor()
        {
            var color = MaterialHelpers.GetBodyMaskColor(_gameObject, _bodyMaterial);
            if (color == null) return _defaultColor;
            return (Color)color;
        }

        public void ChangeColor(Color color)
        {
            MaterialHelpers.ChangeMaterialColor(_bodyMaterial, _gameObject, color);
            OnColorChanged?.Invoke(color);
        }

        public void ChangeColorMask(Color color)
        {
            MaterialHelpers.ChangeMaterialColor(_bodyMaterial, _gameObject, Color.white,color,Color.white);
            OnColorChanged?.Invoke(color);
        }
    }
}
