using System;
using FCS_AlterraHomeSolutions.Mono.PaintTool;
using FCS_AlterraHub.Extensions;
using FCS_AlterraHub.Helpers;
using FCS_AlterraHub.Model;
using FCSCommon.Helpers;
using FCSCommon.Utilities;
using UnityEngine;

namespace FCS_AlterraHub.Mono
{
    public class ColorManager : MonoBehaviour
    {
        private string _bodyMaterial;
        private GameObject _gameObject;
        private string _bodySecondary;
        private Color _currentColor = Color.white;
        private Color _currentSecondaryColor = Color.white;
        private Color _currentLumColor = Color.white;
        private string _lumMaterial;
        private ColorTemplate _currentTemplate = new ColorTemplate
        {
            PrimaryColor = Color.white,
            SecondaryColor = Color.white,
            EmissionColor = Color.cyan
        };


        public void Initialize(GameObject gameObject, string bodyMaterial, string secondaryMaterial = "",
            string lumControllerMaterial = "")
        {
            _gameObject = gameObject;
            _bodyMaterial = bodyMaterial;
            _bodySecondary = secondaryMaterial;
            _lumMaterial = lumControllerMaterial;
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

        public bool ChangeColor(ColorTemplate template)
        {

            bool result;

            if (template == null) return false;

            try
            {
                _currentTemplate = template;
                MaterialHelpers.ChangeMaterialColor(_bodyMaterial, _gameObject, template.PrimaryColor);
                MaterialHelpers.ChangeMaterialColor(_bodySecondary, _gameObject, template.SecondaryColor);
                MaterialHelpers.ChangeEmissionColor(_lumMaterial, _gameObject, template.EmissionColor);
                result = true;
            }
            catch (Exception e)
            {
                QuickLogger.Error(e.StackTrace);
                QuickLogger.Error(e.Message);
                result = false;
            }
            return result;
        }


        public ColorTemplate GetTemplate()
        {
            return _currentTemplate;
        }

        public void LoadTemplate(ColorTemplateSave colorTemplate)
        {
            ChangeColor(colorTemplate.ToColorTemplate());
        }

        public ColorTemplateSave SaveTemplate()
        {
            return _currentTemplate.ToColorTemplate();
        }
    }
}
