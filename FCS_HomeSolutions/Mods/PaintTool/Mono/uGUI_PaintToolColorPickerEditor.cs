using FCS_HomeSolutions.Mods.PaintTool.Models;
using UnityEngine;
using UnityEngine.UI;

namespace FCS_HomeSolutions.Mods.PaintTool.Mono
{
    internal class uGUI_PaintToolColorPickerEditor : MonoBehaviour
    {
        private uGUI_PaintToolColorPicker _sender;
        private HSVControl _primary;
        private HSVControl _secondary;
        private HSVControl _emission;
        private ColorPickerTemplateItemController _template;
        public static uGUI_PaintToolColorPickerEditor Main;


        // Start is called before the first frame update
        private void Awake()
        {
            if (Main == null)
            {
                Main = this;
                DontDestroyOnLoad(this);
            }

            else if (Main != null)
            {
                Destroy(gameObject);
                return;
            }


            _primary = gameObject.transform.Find("Primary_HSV_Group").GetComponent<HSVControl>();
            _secondary = gameObject.transform.Find("Secondary_HSV_Group").GetComponent<HSVControl>();
            _emission = gameObject.transform.Find("Emission_HSV_Group").GetComponent<HSVControl>();
            var doneBTN = gameObject.transform.Find("DoneBTN").GetComponent<Button>();
            doneBTN.onClick.AddListener((() =>
            {
                _template.SetColors(new ColorTemplate
                {
                    PrimaryColor = _primary.GetColor(),
                    SecondaryColor = _secondary.GetColor(),
                    EmissionColor = _emission.GetColor()
                });
                Close();
            }));

            var cancelBTN = gameObject.transform.Find("CancelBTN").GetComponent<Button>();
            cancelBTN.onClick.AddListener((() =>
            {
                Close();
            }));

            _emission = gameObject.transform.Find("Emission_HSV_Group").GetComponent<HSVControl>();
        }

        public void Open(ColorPickerTemplateItemController template, uGUI_PaintToolColorPicker sender)
        {
            _template = template;
            var colors = template.GetTemplate();
            gameObject.SetActive(true);
            sender.gameObject.SetActive(false);
            _primary.SetColors(colors.PrimaryColor);
            _secondary.SetColors(colors.SecondaryColor);
            _emission.SetColors(colors.EmissionColor);
            _sender = sender;
        }

        public void Close()
        {
            _primary.SetColors(Color.white);
            _secondary.SetColors(Color.white);
            _emission.SetColors(Color.white);
            gameObject.SetActive(false);
            _sender.gameObject.SetActive(true);
        }
    }
}
