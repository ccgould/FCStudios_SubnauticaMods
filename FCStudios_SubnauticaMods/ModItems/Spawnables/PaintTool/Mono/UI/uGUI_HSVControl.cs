using UnityEngine;
using UnityEngine.UI;

namespace FCS_AlterraHub.ModItems.Spawnables.PaintTool.Mono.UI;
internal class uGUI_HSVControl : MonoBehaviour
{

    private float _h;
    private float _s;
    private float _v;

    [SerializeField]private Slider _hSlider;
    [SerializeField]private Slider _sSlider;
    [SerializeField]private Slider _vSlider;
    [SerializeField]private Image _sBarOverlay;
    [SerializeField]private Image _vBarOverlay;
    [SerializeField]private Image _colorOutput;
    [SerializeField]private uGUI_InputField _hInputField;
    [SerializeField]private uGUI_InputField _sInputField;
    [SerializeField]private uGUI_InputField _vInputField;

    void Awake()
    {
        _hSlider.onValueChanged.AddListener((amount =>
        {
            _hInputField.SetTextWithoutNotify(Mathf.RoundToInt(amount * 360).ToString());
            _h = amount;
            UpdateColor();
        }));

        _sSlider.onValueChanged.AddListener((amount =>
        {
            _sInputField.SetTextWithoutNotify(Mathf.RoundToInt(amount * 100).ToString());
            _s = amount;
            UpdateColor();
        }));

        _vSlider.onValueChanged.AddListener((amount =>
        {
            _vInputField.SetTextWithoutNotify(Mathf.RoundToInt(amount * 100).ToString());
            _v = amount;
            UpdateColor();
        }));

        _hInputField.onValueChanged.AddListener((amount =>
        {
            float convertedAmount = ValidateAmount(_hInputField,amount, 360f);
            _h = convertedAmount / 360f;
            _hSlider.SetValueWithoutNotify(_h);
            UpdateColor();
        }));

        _sInputField.onValueChanged.AddListener((amount =>
        {
            float convertedAmount = ValidateAmount(_sInputField,amount, 100f);
            _s = convertedAmount / 100f;
            _sSlider.SetValueWithoutNotify(_s);
            UpdateColor();
        }));

        _vInputField.onValueChanged.AddListener((amount => 
        {
            float convertedAmount = ValidateAmount(_vInputField,amount, 100f);
            _v = convertedAmount / 100f;
            _vSlider.SetValueWithoutNotify(_v);
            UpdateColor();
        }));
    }

    private float ValidateAmount(uGUI_InputField inputField, string amount, float max)
    {
        float convertedAmount = 0f;

        if (float.TryParse(amount, out float value))
        {
            convertedAmount = value;

            if (convertedAmount > max)
            {
                inputField.SetTextWithoutNotify(max.ToString());
                convertedAmount = max;
            }

            if (convertedAmount < 0)
            {
                inputField.SetTextWithoutNotify("0");
                convertedAmount = 0;
            }
        }
        else
        {
            inputField.SetTextWithoutNotify("0");
        }
        return convertedAmount;
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
