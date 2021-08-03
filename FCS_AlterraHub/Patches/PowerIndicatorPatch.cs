using System;
using System.Collections.Generic;
using System.Linq;
using FCS_AlterraHub.Buildables;
using FCS_AlterraHub.Helpers;
using FCSCommon.Utilities;
using HarmonyLib;
using Oculus.Newtonsoft.Json.Serialization;
using UnityEngine;
using UnityEngine.UI;

namespace FCS_AlterraHub.Patches
{
    [HarmonyPatch(typeof(uGUI_PowerIndicator))]
    [HarmonyPatch("Initialize")]
    class uGUI_PowerIndicator_Initialize_Patch
    {
        private static FCSHUD tracker;

        private static void Postfix(uGUI_PowerIndicator __instance)
        {

            if (IndicatorInstance == null)
            {
                if (Inventory.main == null)
                {
                    return;
                }
                var prefab = GameObject.Instantiate(AlterraHub.MissionMessageBoxPrefab);
                var hudTransform = GameObject.Find("ScreenCanvas").transform.Find("HUD").Find("Content");
                prefab.transform.SetParent(hudTransform, false);
                prefab.transform.localPosition = new Vector3(1400.00f, 340.00f, 0f);
                prefab.transform.localEulerAngles = new Vector3(0f, 0f, 0f);
                prefab.transform.localScale = Vector3.one;
                prefab.transform.SetSiblingIndex(0);
                MissionHUD = prefab.AddComponent<MissionHUD>();

                var imageSelectorPrefab = GameObject.Instantiate(AlterraHub.ImageSelectorHUDPrefab);

                imageSelectorPrefab.transform.SetParent(hudTransform, false);
                imageSelectorPrefab.transform.localScale = new Vector3(2f,2f,2f);
                imageSelectorPrefab.transform.SetSiblingIndex(0);
                var imageSelectorHud = imageSelectorPrefab.AddComponent<ImageSelectorHUD>();
                imageSelectorHud.Hide();

                IndicatorInstance = __instance;
            }
        }


        public static uGUI_PowerIndicator IndicatorInstance { get; set; }
        public static MissionHUD MissionHUD { get; set; }
    }

    public class ImageSelectorHUD : MonoBehaviour
    {
        private System.Action<Texture2D, Atlas.Sprite> _callback;
        private ToggleGroup _toggleGroup;
        private Dictionary<Texture2D, Atlas.Sprite> _images;
        public static ImageSelectorHUD Main;
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
        private bool _isOpen;
        private List<ImageSelectorItem_uGUI> _currentButtons = new ();

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

            _toggleGroup = gameObject.GetComponentInChildren<ToggleGroup>();

            GameObjectHelpers.FindGameObject(gameObject, "CloseBTN").GetComponent<Button>().onClick.AddListener(Hide);

            GameObjectHelpers.FindGameObject(gameObject, "DoneBTN").GetComponent<Button>().onClick.AddListener((() =>
            {

                Toggle selectedToggle = _toggleGroup.ActiveToggles().FirstOrDefault();
                if (selectedToggle != null)
                {
                    Debug.Log(selectedToggle, selectedToggle);
                    var selectedItem = selectedToggle.gameObject.GetComponent<ImageSelectorItem_uGUI>();
                    if (selectedItem != null)
                    {
                        var sprite = selectedItem.GetSprite();
                        var tex2D = selectedItem.GetTexture2D();
                        _callback?.Invoke(tex2D, sprite);
                    }
                }
                else
                {
                    QuickLogger.DebugError("No items selected",true);
                }

                Hide();
            }));
        }

        public void Hide()
        {
            gameObject.SetActive(false);
            if(_isOpen)
            {
                InterceptInput(false);
                LockMovement(false);
            }

            _isOpen = false;
        }

        public void Show(Dictionary<Texture2D, Atlas.Sprite> images, Texture2D selectedImage, System.Action<Texture2D, Atlas.Sprite> onDoneClicked)
        {
            _isOpen = true;
            _images = images;
            _callback = onDoneClicked;
            OnLoadDisplay();
            InterceptInput(true);
            LockMovement(true);
            SelectWithTexture2D(selectedImage);
            gameObject.SetActive(true);
        }

        private void OnLoadDisplay()
        {
            _currentButtons.Clear();

            foreach (Transform item in _toggleGroup.gameObject.transform)
            {
                Destroy(item.gameObject);
            }

            for (int i = 0; i < _images.Count; i++)
            {
                GameObject buttonPrefab = GameObject.Instantiate(AlterraHub.ImageSelectorItemPrefab);
                buttonPrefab.transform.SetParent(_toggleGroup.gameObject.transform, false);

                buttonPrefab.GetComponent<Toggle>().group = _toggleGroup;

                var uGUI = buttonPrefab.AddComponent<ImageSelectorItem_uGUI>();
                var currentElement = _images.ElementAt(i);
                uGUI.Initialize(currentElement.Key,currentElement.Value);
                _currentButtons.Add(uGUI);
            }
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

        public void SelectWithTexture2D(Texture2D texture2D)
        {
            if(texture2D == null) return;

            if (_currentButtons != null && _currentButtons.Any())
            {
                foreach (ImageSelectorItem_uGUI button in _currentButtons)
                {
                    if (button.IsEqual(texture2D))
                    {
                        button.Select();
                    }
                }
            }
        }
    }

    internal class ImageSelectorItem_uGUI : MonoBehaviour
    {
        private uGUI_Icon _icon;
        private Texture2D _texture;
        private Atlas.Sprite _sprite;
        private Toggle _toggle;

        public void Initialize(Texture2D texture, Atlas.Sprite sprite)
        {
            _toggle = gameObject.GetComponent<Toggle>();
            _icon = GameObjectHelpers.FindGameObject(gameObject, "Background").AddComponent<uGUI_Icon>();
            _icon.sprite = sprite;
            _texture = texture;
            _sprite = sprite;
        }

        public Texture2D GetTexture2D()
        {
            return _texture;
        }

        public Atlas.Sprite GetSprite()
        {
            return _sprite;
        }

        public bool IsEqual(Texture2D texture2D)
        {
            return texture2D == _texture;
        }

        public void Select()
        {
            _toggle.isOn = true;
        }
    }
}
