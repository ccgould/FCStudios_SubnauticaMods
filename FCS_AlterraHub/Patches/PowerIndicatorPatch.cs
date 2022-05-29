using System.Collections.Generic;
using System.Linq;
using FCS_AlterraHub.Buildables;
using FCS_AlterraHub.Enumerators;
using FCS_AlterraHub.Helpers;
using FCS_AlterraHub.Model.GUI;
using FCS_AlterraHub.Mods.OreConsumer.Mono;
using FCSCommon.Utilities;
using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;
#if SUBNAUTICA
using Sprite = Atlas.Sprite;
#endif

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
                //var prefab = Object.Instantiate(AlterraHub.MissionMessageBoxPrefab);
                var hudTransform = GameObject.Find("ScreenCanvas").transform.Find("HUD").Find("Content");
                //prefab.transform.SetParent(hudTransform, false);
                //prefab.transform.localPosition = new Vector3(1400.00f, 340.00f, 0f);
                //prefab.transform.localEulerAngles = new Vector3(0f, 0f, 0f);
                //prefab.transform.localScale = Vector3.one;
                //prefab.transform.SetSiblingIndex(0);
                //MissionHUD = prefab.AddComponent<MissionHUD>();

                var oreConsumerPrefab = Object.Instantiate(AlterraHub.GetPrefab("uGUI_OreConsumer"));

                oreConsumerPrefab.transform.SetParent(hudTransform, false);
                oreConsumerPrefab.transform.SetSiblingIndex(0);
                var oreConsumerHud = oreConsumerPrefab.AddComponent<OreConsumerHUD>();
                oreConsumerHud.Hide();

                var imageSelectorPrefab = Object.Instantiate(AlterraHub.ImageSelectorHUDPrefab);

                imageSelectorPrefab.transform.SetParent(hudTransform, false);
                imageSelectorPrefab.transform.localScale = new Vector3(2f,2f,2f);
                imageSelectorPrefab.transform.SetSiblingIndex(0);
                var imageSelectorHud = imageSelectorPrefab.AddComponent<ImageSelectorHUD>();
                imageSelectorHud.Hide();

                IndicatorInstance = __instance;
            }
        }


        public static uGUI_PowerIndicator IndicatorInstance { get; set; }
       // public static MissionHUD MissionHUD { get; set; }
    }

    public class ImageSelectorHUD : uGUI_InputGroup, uGUI_IButtonReceiver
    {
        private System.Action<Texture2D, Sprite> _callback;
        private ToggleGroup _toggleGroup;
        private Dictionary<Texture2D, Sprite> _images;
        public static ImageSelectorHUD Main;

        private List<ImageSelectorItem_uGUI> _currentButtons = new();
        
        public override void Awake()
        {
            base.Awake();
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

        private void Start()
        {
            gameObject.SetActive(false);
        }

        public override void Update()
        {
            base.Update();
            if (focused && GameInput.GetButtonDown(GameInput.Button.PDA))
            {
                Hide();
            }
        }

        public void Hide()
        {
            Deselect();
        }

        public void Show(Dictionary<Texture2D, Sprite> images, Texture2D selectedImage, System.Action<Texture2D, Sprite> onDoneClicked)
        {
            if (Time.timeSinceLevelLoad < 1f)
            {
                return;
            }
            if (!gameObject.activeInHierarchy)
            {
                gameObject.SetActive(true);
                Select();
            }

            _images = images;
            _callback = onDoneClicked;
            OnLoadDisplay();
            SelectWithTexture2D(selectedImage);
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
                GameObject buttonPrefab = Instantiate(AlterraHub.ImageSelectorItemPrefab);
                buttonPrefab.transform.SetParent(_toggleGroup.gameObject.transform, false);

                buttonPrefab.GetComponent<Toggle>().group = _toggleGroup;

                var uGUI = buttonPrefab.AddComponent<ImageSelectorItem_uGUI>();
                var currentElement = _images.ElementAt(i);
                uGUI.Initialize(currentElement.Key,currentElement.Value);
                _currentButtons.Add(uGUI);
            }
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

        public bool OnButtonDown(GameInput.Button button)
        {
            if (button == GameInput.Button.UICancel || button == GameInput.Button.PDA)
            {
                Deselect();
                return true;
            }
            return false;
        }

        public override void OnDeselect()
        {
            base.OnDeselect();
            gameObject.SetActive(false);
        }

        public override void OnReselect(bool lockMovement)
        {
            base.OnReselect(true);
        }
    }

    public class OreConsumerHUD : uGUI_InputGroup, uGUI_IButtonReceiver
    {
        private GameObject _grid;
        public static OreConsumerHUD Main;
        private bool _isOpen;
        private readonly List<uGUI_FCSDisplayItem> _currentItems = new();
        private OreConsumerController _sender;
        private Slider _powerSlider;
        private Text _powerUsageLabel;
        private Text _processingItemLabel;
        private Text _oreValueLabel;
        private Text _time;
        private Text _speedValueLabel;

        public override void Update()
        {
            base.Update();
            if (focused && GameInput.GetButtonDown(GameInput.Button.PDA))
            {
                Hide();
            }
            UpdateScreen();
        }

        public override void Awake()
        {
            base.Awake();

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

            _speedValueLabel = GameObjectHelpers.FindGameObject(gameObject, "SpeedValueLabel").GetComponent<Text>();
            _oreValueLabel = GameObjectHelpers.FindGameObject(gameObject, "OreValueLabel").GetComponent<Text>();

            _powerSlider = gameObject.GetComponentInChildren<Slider>();
            _powerSlider.onValueChanged.AddListener((value =>
            {
                var asInt = Mathf.RoundToInt(value);
                var speedMode = (SpeedModes) asInt;
                _sender?.ChangeSpeedMultiplier(speedMode);
                ChangeSpeedLabelText(speedMode.ToString());
            }));

            _powerUsageLabel = GameObjectHelpers.FindGameObject(gameObject, "PowerUsageLabel").GetComponent<Text>();
            _processingItemLabel = GameObjectHelpers.FindGameObject(gameObject, "ItemProcessingLabel").GetComponent<Text>();
            _time = GameObjectHelpers.FindGameObject(gameObject, "Time").GetComponent<Text>();

            _grid = gameObject.GetComponentInChildren<GridLayoutGroup>()?.gameObject;

            if (_grid != null)
            {
                foreach (Transform item in _grid.transform)
                {
                    var displayItem = item.gameObject.AddComponent<uGUI_FCSDisplayItem>();
                    displayItem.Initialize(TechType.None);
                    _currentItems.Add(displayItem);
                }
            }

            GameObjectHelpers.FindGameObject(gameObject, "CloseBTN").GetComponent<Button>().onClick.AddListener(Hide);

            GameObjectHelpers.FindGameObject(gameObject, "OpenBTN").GetComponent<Button>().onClick.AddListener((() =>
            {
                _sender.OpenStorage();
                Hide();
            }));

        }

        private void Start()
        {
            gameObject.SetActive(false);
        }

        private void ChangeSpeedLabelText(string value)
        {
            _speedValueLabel.text = value;
        }
        
        private void UpdateScreen()
        {
            if (_sender == null || !_isOpen) return;

            if (_powerUsageLabel != null)
            {
                _powerUsageLabel.text = $"{Language.main.Get("POWER")}: {_sender.GetPowerUsage()}";
            }

            if (_processingItemLabel != null)
            {
                _processingItemLabel.text = $"{AlterraHub.ProcessingItem()}: {_sender.GetProcessingItemString()}";
            }

            if (_oreValueLabel != null)
            {
                _oreValueLabel.text = $"{AlterraHub.OreValue()}: {_sender.GetOreValue()}";
            }

            if (_time != null)
            {
                _time.text = $"{AlterraHub.TimeLeft()}: {_sender.GetTimeLeftString()}s";
            }
        }

        private void OnLoadDisplay()
        {
            ClearItems();
            var oreQueue = _sender.GetOreQueue();
            for (int i = 0; i < oreQueue.Count; i++)
            {
                _currentItems[i].Set(oreQueue.ElementAt(i));
            }
        }

        private void ClearItems()
        {
            foreach (uGUI_FCSDisplayItem currentItem in _currentItems)
            {
                currentItem.Clear();
            }
        }

        internal void Show(OreConsumerController sender)
        {
            if (Time.timeSinceLevelLoad < 1f)
            {
                return;
            }
            if (!gameObject.activeInHierarchy)
            {
                gameObject.SetActive(true);
                Select();
            }

            _sender = sender;
            OnLoadDisplay();
            _sender.OnProcessingCompleted += OnLoadDisplay;

            _powerSlider.SetValueWithoutNotify(sender.GetSpeedMultiplier());
            ChangeSpeedLabelText(((SpeedModes)sender.GetSpeedMultiplier()).ToString());
            _isOpen = true;
        }

        public void Hide()
        {
            Deselect();
        }

        public bool OnButtonDown(GameInput.Button button)
        {
            if (button == GameInput.Button.UICancel || button == GameInput.Button.PDA)
            {
                Deselect();
                return true;
            }
            return false;
        }

        public override void OnDeselect()
        {
            base.OnDeselect();
            gameObject.SetActive(false);
            ClearItems();
            _isOpen = false;
            if (_sender != null)
            {
                _sender.OnProcessingCompleted -= OnLoadDisplay;
                _sender = null;
            }
        }

        public override void OnReselect(bool lockMovement)
        {
            base.OnReselect(true);
        }
    }

    internal class ImageSelectorItem_uGUI : MonoBehaviour
    {
        private uGUI_Icon _icon;
        private Texture2D _texture;
        private Sprite _sprite;
        private Toggle _toggle;

        public void Initialize(Texture2D texture, Sprite sprite)
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

        public Sprite GetSprite()
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
