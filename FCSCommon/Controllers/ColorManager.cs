using FCSCommon.Helpers;
using UnityEngine;

namespace FCSCommon.Controllers
{
    public class ColorManager
    {
        private string _bodyMaterial;
        private GameObject _gameObject;

        public Color GetColor()
        {
            var color = MaterialHelpers.GetBodyColor(_gameObject, _bodyMaterial);
            if (color == null) return Color.white;
            return (Color)color;
        }

        public void ChangeColor(Color color)
        {
            MaterialHelpers.ChangeMaterialColor(_bodyMaterial, _gameObject, color);
        }

        public void Initialize(GameObject gameObject, string bodyMaterial)
        {
            _gameObject = gameObject;
            _bodyMaterial = bodyMaterial;
        }
    }
}
