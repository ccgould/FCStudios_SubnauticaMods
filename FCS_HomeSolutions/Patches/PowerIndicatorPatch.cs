using System;
using System.Linq;
using FCS_AlterraHub.Helpers;
using FCS_AlterraHub.Model;
using FCS_HomeSolutions.Buildables;
using FCS_HomeSolutions.Mods.Elevator.Mono;
using FCS_HomeSolutions.Mods.PaintTool.Mono;
using FCSCommon.Utilities;
using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;
using Action = System.Action;

namespace FCS_HomeSolutions.Patches
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

                var hudTransform = GameObject.Find("ScreenCanvas").transform.Find("HUD").Find("Content");

                var elevatorHudPrefab = GameObject.Instantiate(ModelPrefab.ElevatorUIPrefab);
                elevatorHudPrefab.transform.SetParent(hudTransform, false);
                elevatorHudPrefab.transform.localScale = new Vector3(2f, 2f, 2f);
                elevatorHudPrefab.transform.SetSiblingIndex(0);
                var elevatorHud = elevatorHudPrefab.AddComponent<ElevatorHUD>();
                elevatorHud.Hide();

                //PaintTool uGUI
                var uGUI_PaintToolColorPicker = GameObject.Instantiate(ModelPrefab.GetPrefab("uGUI_PaintToolColorTemplate",  false));
                uGUI_PaintToolColorPicker.transform.SetParent(hudTransform, false);
                uGUI_PaintToolColorPicker.transform.SetSiblingIndex(0);
                var uGUI_PaintToolColorPickerHud = uGUI_PaintToolColorPicker.AddComponent<uGUI_PaintToolColorPicker>();
                uGUI_PaintToolColorPickerHud.Close();


                //PaintTool Editor uGUI
                var uGUI_PaintToolColorEditor = GameObject.Instantiate(ModelPrefab.GetPrefab("uGUI_PaintToolColorPicker",  false));
                uGUI_PaintToolColorEditor.transform.SetParent(hudTransform, false);
                uGUI_PaintToolColorEditor.transform.SetSiblingIndex(0);
                var uGUI_PaintToolColorEditorHud = uGUI_PaintToolColorEditor.AddComponent<uGUI_PaintToolColorPickerEditor>();
                uGUI_PaintToolColorEditorHud.Close();

                IndicatorInstance = __instance;
            }
        }

        public static uGUI_PowerIndicator IndicatorInstance { get; set; }
    }
    
    public class ElevatorHUD : uGUI_InputGroup, uGUI_IButtonReceiver
    {
        public static ElevatorHUD Main;
        public int MAX_FLOOR_LEVELS = 20;
        private GameObject _grid;
        private FCSElevatorController _currentController;
        private Slider _slider;
        private Toggle _toggle;
        private FCSMessageBox _messageBox;
        private Text _speedAmount;
        internal Action onFloorAdded;
        internal Action onFloorRemoved;
        internal Action onFloorLevelChanged;
        private ToggleGroup _toggleGroup;

        public override void Update()
        {
            base.Update();
            if (focused && GameInput.GetButtonDown(GameInput.Button.PDA))
            {
                Hide();
            }
        }

        private void Start()
        {
            gameObject.SetActive(false);
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

            _grid = GameObjectHelpers.FindGameObject(gameObject, "Content");

            GameObjectHelpers.FindGameObject(gameObject, "CloseBTN").GetComponent<Button>().onClick.AddListener(Hide);
            GameObjectHelpers.FindGameObject(gameObject, "AddBTN").GetComponent<Button>().onClick.AddListener(AddNewItem);
            _speedAmount = GameObjectHelpers.FindGameObject(gameObject, "ElevatorSpeedAmount").GetComponent<Text>();

            if (_messageBox == null)
            {
                _messageBox = GameObjectHelpers.FindGameObject(gameObject, "MessageBox").AddComponent<FCSMessageBox>();
            }

            _toggle = GameObjectHelpers.FindGameObject(gameObject, "ShowRailingsToggle").GetComponent<Toggle>();
            _toggle.onValueChanged.AddListener((
            value =>
            {
                _currentController.ChangeRailingVisibility(value);
            }));
            _slider = GameObjectHelpers.FindGameObject(gameObject, "Slider").GetComponent<Slider>();
            _slider.onValueChanged.AddListener((
            value =>
            {
                _speedAmount.text = value.ToString("0.00");
                _currentController.ChangeSpeed(value);
            }));
        }

        public void Hide()
        {
            Deselect();
        }

        internal void Show(FCSElevatorController controller)
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

            _currentController = controller;
            OnLoadDisplay();
            _toggle.SetIsOnWithoutNotify(controller.IsRailingsVisible());
            _slider.SetValueWithoutNotify(controller.GetSpeedValue());
            _speedAmount.text = controller.GetSpeedValue().ToString("0.00");
        }

        private void OnLoadDisplay()
        {

            var data = _currentController.GetStoredFloorData().OrderByDescending(x => x.Value.Meters).ToList();

            var total = data.Count - 1;

            for (int i = 0; i < data.Count; i++)
            {
                data.ElementAt(i).Value.FloorIndex = total--;
            }

            foreach (Transform item in _grid.transform)
            {
                Destroy(item.gameObject);
            }

            for (int i = 0; i < data.Count; i++)
            {
                GameObject floorItemPrefab = GameObject.Instantiate(ModelPrefab.ElevatorFloorItemPrefab);
                floorItemPrefab.transform.SetParent(_grid.transform, false);

                var currentElement = data.ElementAt(i);
                var uGUI = floorItemPrefab.AddComponent<ElevatorFloorItem_uGUI>();
                uGUI.Initialize(_currentController, currentElement.Value.FloorIndex, currentElement.Value.Meters, currentElement.Value.FloorName, currentElement.Key);

            }

        }
        
        private void AddNewItem()
        {
            if (_currentController.GetFloorCount() > MAX_FLOOR_LEVELS)
            {
                ShowMessage($"Elevator floor limit is {MAX_FLOOR_LEVELS}");
                return;
            }

            _currentController.AddNewFloor(Guid.NewGuid().ToString(), "Custom Floor", _currentController.GetNewFloorLevel(), _currentController.GetStoredFloorData().Count);
            OnLoadDisplay();
            onFloorAdded?.Invoke();
        }

        internal void ShowMessage(string message)
        {
            _messageBox.Show(message, FCSMessageButton.OK, null);
        }

        internal void Refresh()
        {
            OnLoadDisplay();
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

    internal class ElevatorFloorItem_uGUI : MonoBehaviour
    {
        private InputField _floorField;
        private InputField _nameField;
        private string _id;
        private FCSElevatorController _controller;
        private float MAX_FLOOR_LEVEL_IN_METERS = 200f;
        private float _prevMeters;

        public void Initialize(FCSElevatorController controller, int index, float meters, string name, string id)
        {
            GameObjectHelpers.FindGameObject(gameObject, "ItemIndex").GetComponent<Text>().text = index.ToString("D2");

            _controller = controller;

            _id = id;

            _prevMeters = meters;

            _floorField = GameObjectHelpers.FindGameObject(gameObject, "FloorField").GetComponent<InputField>();
            _floorField.SetTextWithoutNotify(!id.Equals("base") ? meters.ToString() : "0");
            _floorField.onEndEdit.AddListener(OnEndFloorEdit);
            _floorField.enabled = !id.Equals("base");

            _floorField.onValueChanged.AddListener(OnValueChanged);

            _nameField = GameObjectHelpers.FindGameObject(gameObject, "NameField").GetComponent<InputField>();
            _nameField.SetTextWithoutNotify(name);
            _nameField.onEndEdit.AddListener(OnEndNameEdit);


            var deleteBtn = GameObjectHelpers.FindGameObject(gameObject, "DeleteBTN").GetComponent<Button>();
            deleteBtn.onClick.AddListener((() =>
            {
                if (controller.DeleteLevel(_id))
                {
                    ElevatorHUD.Main.Refresh();
                    ElevatorHUD.Main.onFloorRemoved?.Invoke();
                }
            }));

            var goToFloorButton = GameObjectHelpers.FindGameObject(gameObject, "GoToFloorBTN").GetComponent<Button>();
            goToFloorButton.onClick.AddListener((() =>
            {
                QuickLogger.Debug($"Attempting to go to floor with Id: {_id}", true);

                if (meters > MAX_FLOOR_LEVEL_IN_METERS)
                {
                    ElevatorHUD.Main.ShowMessage($"Cannot go to floor {name} due to it being over the maximum floor height of {MAX_FLOOR_LEVEL_IN_METERS}m. Please adjust floor level.");
                    return;
                }


                controller.GoToFloor(_id);
            }));
        }

        private void OnValueChanged(string value)
        {
            if (string.IsNullOrWhiteSpace(value) || value.Equals("0"))
            {
                _floorField.SetTextWithoutNotify(_prevMeters.ToString());
                return;
            }

            if (float.Parse(value) > MAX_FLOOR_LEVEL_IN_METERS)
            {
                _floorField.SetTextWithoutNotify(_prevMeters.ToString());
                ElevatorHUD.Main.ShowMessage($"This elevator cannot go higher than {MAX_FLOOR_LEVEL_IN_METERS}m.");
            }
        }

        private void OnEndFloorEdit(string value)
        {
            _controller.UpdateFloor(_id, _nameField.text, float.Parse(value));
            ElevatorHUD.Main.Refresh();
            ElevatorHUD.Main.onFloorLevelChanged?.Invoke();
        }

        private void OnEndNameEdit(string value)
        {
            _controller.UpdateFloor(_id, value, float.Parse(_floorField.text));
        }
    }
}
