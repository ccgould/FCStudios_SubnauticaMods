using System;
using FCS_AlterraHub.Model;
using FCSCommon.Utilities;
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
            
            _primary = gameObject.transform.Find("Primary_HSV_Group").gameObject.EnsureComponent<HSVControl>();
            _secondary = gameObject.transform.Find("Secondary_HSV_Group").gameObject.EnsureComponent<HSVControl>();
            _emission = gameObject.transform.Find("Emission_HSV_Group").gameObject.EnsureComponent<HSVControl>();
            var doneBTN = gameObject.transform.Find("DoneBTN").GetComponent<Button>();

            doneBTN.onClick.AddListener((() =>
            {
                try
                {
                    _template.SetColors(new ColorTemplate
                    {
                        PrimaryColor = _primary.GetColor(),
                        SecondaryColor = _secondary.GetColor(),
                        EmissionColor = _emission.GetColor()
                    });
                    _sender.NotifyItemChanged(_template.Index, _template.GetTemplate());
                    Close();
                }
                catch (Exception e)
                {
                    QuickLogger.Error(e.StackTrace);
                    QuickLogger.Error(e.Message);
                    QuickLogger.Message("Attempting to fix in final block");
                }
                finally
                {
                    Close();
                }
            }));

            var cancelBTN = gameObject.transform.Find("CancelBTN").GetComponent<Button>();
            cancelBTN.onClick.AddListener(Close);

            _emission = gameObject.transform.Find("Emission_HSV_Group").GetComponent<HSVControl>();
        }

        public void Open(ColorPickerTemplateItemController template, uGUI_PaintToolColorPicker sender)
        {
            _isOpen = true;
            _template = template;
            var colors = template.GetTemplate();
            gameObject.SetActive(true);
            sender.gameObject.SetActive(false);
            _primary.SetColors(colors.PrimaryColor);
            _secondary.SetColors(colors.SecondaryColor);
            _emission.SetColors(colors.EmissionColor);
            _sender = sender;
            InterceptInput(true);
            LockMovement(true);
        }

        public void Close()
        {
            _primary?.SetColors(Color.white);
            _secondary?.SetColors(Color.white);
            _emission?.SetColors(Color.white);
            gameObject?.SetActive(false);
            _sender?.gameObject?.SetActive(true);
            if (_isOpen)
            {
                InterceptInput(false);
                LockMovement(false);
            }

            _isOpen = false;
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

        public bool IsOpen()
        {
            return _isOpen;
        }
    }
}
