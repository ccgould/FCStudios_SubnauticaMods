using UnityEngine;
using UnityEngine.UI;

namespace FCS_HomeSolutions.Mods.PaintTool.Mono
{
    internal class HSVControl : MonoBehaviour
    {
        private Image _colorOutput;
        private float _h;
        private float _s;
        private float _v;
        private Slider _hSlider;
        private Slider _sSlider;
        private Slider _vSlider;
        private Image _sBarOverlay;
        private Image _vBarOverlay;

        void Awake()
        {
            _colorOutput = gameObject.transform.Find("ColorOutput").GetComponent<Image>();

            var hBarGameObject = gameObject.transform.Find("H_Bar");
            _hSlider = hBarGameObject.GetComponent<Slider>();
            var hCounter = hBarGameObject.GetComponentInChildren<Text>();
            _hSlider.onValueChanged.AddListener((amount =>
            {
                hCounter.text = Mathf.RoundToInt(amount * 360).ToString();
                _h = amount;
                UpdateColor();
            }));

            var sBarGameObject = gameObject.transform.Find("S_Bar");
            _sBarOverlay = sBarGameObject.Find("Overlay").GetComponent<Image>();
            _sSlider = sBarGameObject.GetComponent<Slider>();
            var sCounter = sBarGameObject.GetComponentInChildren<Text>();
            _sSlider.onValueChanged.AddListener((amount =>
            {
                sCounter.text = amount.ToString("F1");
                _s = amount;
                UpdateColor();
            }));

            var vBarGameObject = gameObject.transform.Find("V_Bar");
            _vBarOverlay = vBarGameObject.transform.Find("Background").GetComponent<Image>();
            _vSlider = vBarGameObject.GetComponent<Slider>();
            var vCounter = vBarGameObject.GetComponentInChildren<Text>();
            _vSlider.onValueChanged.AddListener((amount =>
            {
                vCounter.text = amount.ToString("F1");
                _v = amount;
                UpdateColor();
            }));
        }

        private void UpdateColor()
        {
            var rgb = Color.HSVToRGB(_h, _s, _v);
            _colorOutput.color = rgb;
            _sBarOverlay.color = Color.HSVToRGB(_h, 1, _v);
            _vBarOverlay.color = Color.HSVToRGB(_h, _s, 1);
        }

        public void SetColors(Color color)
        {
            Color.RGBToHSV(color, out var h, out var s, out var v);
            _hSlider.value = h;
            _sSlider.value = s;
            _vSlider.value = v;
        }

        public Color GetColor()
        {
            return _colorOutput.color;
        }
    }
}
