using System;
using System.Collections.Generic;
using System.Linq;
using FCS_HomeSolutions.Mods.PaintTool.Models;
using UnityEngine;
using UnityEngine.UI;

namespace FCS_HomeSolutions.Mods.PaintTool.Mono
{
    internal class uGUI_PaintToolColorPicker : MonoBehaviour
    {
        public static uGUI_PaintToolColorPicker Main;
        private Transform _grid;
        private ToggleGroup _toggleGroup;
        private Action<ColorTemplate> _callBack;

        private readonly List<ColorTemplate> _defaultTemplates = new()
        {
            new ColorTemplate
            {
                PrimaryColor = Color.white,
                SecondaryColor = Color.gray,
                EmissionColor = Color.cyan
            },
            new ColorTemplate
            {
                PrimaryColor = Color.red,
                SecondaryColor = Color.gray,
                EmissionColor = Color.white
            },
            new ColorTemplate
            {
                PrimaryColor = Color.magenta,
                SecondaryColor = Color.gray,
                EmissionColor = Color.white
            },
        };

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
        }

        void Start()
        {
            _grid = gameObject.transform.Find("Grid");
            for (int i = 0; i < _grid.childCount; i++)
            {
                if (i + 1 <= _defaultTemplates.Count)
                {
                    var templateItem = _grid.GetChild(i).gameObject.EnsureComponent<ColorPickerTemplateItemController>();
                    templateItem.SetColors(_defaultTemplates[i]);
                }
            }

            _toggleGroup = gameObject.GetComponentInChildren<ToggleGroup>();

            var updateButton = gameObject.transform.Find("UpdateTemplateBTN").GetComponent<Button>();
            updateButton.onClick.AddListener((() =>
            {
                var selectedTemplate = GetSelectedTemplate();
                if (selectedTemplate != null)
                {
                    uGUI_PaintToolColorPickerEditor.Main.Open(selectedTemplate, this);
                }
            }));


            var cancelButton = gameObject.transform.Find("CancelBTN").GetComponent<Button>();
            cancelButton.onClick.AddListener((() =>
            {
                Close();
            }));

            var doneButton = gameObject.transform.Find("UseBTN").GetComponent<Button>();
            doneButton.onClick.AddListener((() =>
            {
                var selectedTemplate = GetSelectedTemplate();
                _callBack?.Invoke(selectedTemplate.GetTemplate());
                Close();
            }));
        }

        internal void Close()
        {
            gameObject.SetActive(false);
        }

        private void Open(Action<ColorTemplate> callBack)
        {
            _callBack = callBack;
            gameObject.SetActive(true);
        }

        private ColorPickerTemplateItemController GetSelectedTemplate()
        {
            return _toggleGroup.ActiveToggles().FirstOrDefault()?.gameObject.GetComponent<ColorPickerTemplateItemController>();
        }
    }
}
