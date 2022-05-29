using System;
using System.Collections.Generic;
using System.Linq;
using FCS_AlterraHub.Model;
using FCSCommon.Utilities;
using UnityEngine;
using UnityEngine.UI;

namespace FCS_HomeSolutions.Mods.PaintTool.Mono
{
    internal class uGUI_PaintToolColorPicker : MonoBehaviour
    {
        public static uGUI_PaintToolColorPicker Main;
        private Transform _grid;
        private ToggleGroup _toggleGroup;
        private Action<ColorTemplate,int> _callBack;
        private bool _isOpen;
        private GameObject _inputDummy;
        private GameObject inputDummy
        {
            get
            {
                if (this._inputDummy == null)
                {
                    this._inputDummy = new GameObject("InputDummy");
                    this._inputDummy.SetActive(false);
                }
                return this._inputDummy;
            }
        }
        private bool _cursorLockCached;
        private PaintToolController _controller;
        private HashSet<ColorPickerTemplateItemController> _colorItems = new ();
        private bool _isInitialized;

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

        private void Initialize()
        {
            if (_isInitialized) return;

            _grid = gameObject.transform.Find("Grid");

            for (int i = 0; i < _grid.childCount; i++)
            {
                var item = _grid.GetChild(i).gameObject.EnsureComponent<ColorPickerTemplateItemController>();
                item.Index = i;
                _colorItems.Add(item);
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
            cancelButton.onClick.AddListener((Close));

            var doneButton = gameObject.transform.Find("UseBTN").GetComponent<Button>();
            doneButton.onClick.AddListener((() =>
            {
                var selectedTemplate = GetSelectedTemplate();
                _callBack?.Invoke(selectedTemplate.GetTemplate(),selectedTemplate.Index);
                Close();
            }));
            _isInitialized = true;
        }

        internal void Close()
        {
            gameObject.SetActive(false);
            
            if (_isOpen)
            {
                InterceptInput(false);
                LockMovement(false);
            }

            _isOpen = false;
        }

        internal void Open(PaintToolController controller, Action<ColorTemplate,int> callBack)
        {
            Initialize();
            _isOpen = true;
            _controller = controller;
            LoadTemplates(controller.GetTemplates());
            gameObject.SetActive(true);
            SelectTemplate(controller.GetCurrentSelectedTemplateIndex());
            _callBack = callBack;
            InterceptInput(true);
            LockMovement(true);
        }

        private void SelectTemplate(int index)
        {
            QuickLogger.Debug($"Color Picker selecting: {index}",true);
            _colorItems.ElementAt(index).Select();
        }

        private void LoadTemplates(List<ColorTemplate> colorTemplates)
        {
            for (var index = 0; index < colorTemplates.Count; index++)
            {
                _colorItems.ElementAt(index).SetColors(colorTemplates[index]);
            }
        }

        private ColorPickerTemplateItemController GetSelectedTemplate()
        {
            return _toggleGroup.ActiveToggles().FirstOrDefault()?.gameObject.GetComponent<ColorPickerTemplateItemController>();
        }

        private void LockMovement(bool state)
        {
            FPSInputModule.current.lockMovement = state;
        }

        private void InterceptInput(bool state)
        {
            if (inputDummy.activeSelf == state)
            {
                return;
            }
            if (state)
            {
                InputHandlerStack.main.Push(inputDummy);
                _cursorLockCached = UWE.Utils.lockCursor;
                UWE.Utils.lockCursor = false;
                return;
            }
            UWE.Utils.lockCursor = _cursorLockCached;
            InputHandlerStack.main.Pop(inputDummy);
        }

        public void NotifyItemChanged(int templateIndex,ColorTemplate colorTemplate)
        {
            _controller.UpdateTemplates(templateIndex, colorTemplate);
        }

        public bool IsOpen()
        {
            var editorResult = uGUI_PaintToolColorPickerEditor.Main?.IsOpen() ?? false;
            return _isOpen || editorResult;
        }
    }
}
