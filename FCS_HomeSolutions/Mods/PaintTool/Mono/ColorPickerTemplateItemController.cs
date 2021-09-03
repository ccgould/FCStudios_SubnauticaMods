using FCS_HomeSolutions.Mods.PaintTool.Models;
using UnityEngine;
using UnityEngine.UI;

namespace FCS_HomeSolutions.Mods.PaintTool.Mono
{
    internal class ColorPickerTemplateItemController : MonoBehaviour
    {
        private Image _primaryCircle;
        private Image _secondaryCircle;
        private Image _emissionCircle;

        private ColorTemplate _colorTemplate = new ColorTemplate();

        private void Awake()
        {
            _primaryCircle = gameObject.transform.Find("Primary").GetComponent<Image>();
            _secondaryCircle = gameObject.transform.Find("Secondary").GetComponent<Image>();
            _emissionCircle = gameObject.transform.Find("Emission").GetComponent<Image>();
        }

        public void SetColors(ColorTemplate template)
        {
            _colorTemplate = template;
            _primaryCircle.color = template.PrimaryColor;
            _secondaryCircle.color = template.SecondaryColor;
            _emissionCircle.color = template.EmissionColor;
        }

        public ColorTemplate GetTemplate()
        {
            return _colorTemplate;
        }
    }
}
