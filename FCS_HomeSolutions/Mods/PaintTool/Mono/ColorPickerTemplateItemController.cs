using FCS_AlterraHub.Model;
using FCSCommon.Utilities;
using UnityEngine;
using UnityEngine.UI;

namespace FCS_HomeSolutions.Mods.PaintTool.Mono
{
    internal class ColorPickerTemplateItemController : MonoBehaviour
    {
        private Image _primaryCircle;
        private Image _secondaryCircle;
        private Image _emissionCircle;

        private ColorTemplate _colorTemplate = new();
        private bool _isInitialized;
        private Toggle _toggle;
        public int Index { get; set; }

        private void Initialize()
        {
            if (_isInitialized) return;
            _primaryCircle = gameObject.transform.Find("Primary").GetComponent<Image>();
            _secondaryCircle = gameObject.transform.Find("Secondary").GetComponent<Image>();
            _emissionCircle = gameObject.transform.Find("Emission").GetComponent<Image>();
            _toggle = gameObject.GetComponent<Toggle>();
            _isInitialized = true;
        }

        public void SetColors(ColorTemplate template)
        {
            Initialize();
            _colorTemplate = template;
            _primaryCircle.color = template.PrimaryColor;
            _secondaryCircle.color = template.SecondaryColor;
            _emissionCircle.color = template.EmissionColor;
        }

        public ColorTemplate GetTemplate()
        {
            return _colorTemplate;
        }

        public void Select()
        {
            QuickLogger.Debug($"Selecting index: {Index}", true);
            _toggle?.SetIsOnWithoutNotify(true);
        }
    }
}
