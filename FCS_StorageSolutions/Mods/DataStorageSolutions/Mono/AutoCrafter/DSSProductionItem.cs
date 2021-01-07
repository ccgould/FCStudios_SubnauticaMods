using FCS_AlterraHub.Enumerators;
using FCS_AlterraHub.Mono;
using FCSCommon.Helpers;
using FCS_StorageSolutions.Configuration;
using UnityEngine;
using UnityEngine.UI;

namespace FCS_StorageSolutions.Mods.DataStorageSolutions.Mono.AutoCrafter
{
    internal class DSSProductionItem : MonoBehaviour
    {
        private uGUI_Icon _icon;
        private Text _text;
        private bool _isInitialized;
        private Text _amount;
        private Text _position;
        private DSSAutoCrafterDisplay _controller;
        private CraftingItem _craftingItem;
        private GameObject _closeBTN;
        private GameObject _repeatBTNObj;
        private FCSToggleButton _repeatBTN;
        private InterfaceButton _amountBTN;


        private void Update()
        {
            if (_craftingItem == null || _amountBTN  == null || _closeBTN == null) return;

            _closeBTN.SetActive(!_controller.GetController().CraftManager.IsRunning());
            _amountBTN.Disabled = _controller.GetController().CraftManager.IsRunning();
        }

        private void Initialize()
        {

            if (_isInitialized) return;

            _icon = gameObject.FindChild("Icon").EnsureComponent<uGUI_Icon>();
            _text = gameObject.FindChild("ItemName").EnsureComponent<Text>();
            _position = gameObject.FindChild("Text").EnsureComponent<Text>();
            _amount = GameObjectHelpers.FindGameObject(gameObject, "Amount").EnsureComponent<Text>();
            _amountBTN = _amount.gameObject.AddComponent<InterfaceButton>();
            _amountBTN.STARTING_COLOR = new Color(0.3647059f, 0.8588236f, 1f,1f);
            _amountBTN.HOVER_COLOR = Color.white;
            _amountBTN.OnButtonClick += (s,o) => { _controller.OpenNumberPad(_craftingItem); };
            _amountBTN.ButtonMode = InterfaceButtonMode.TextColor;
            _amountBTN.TextComponent = _amount;
            _amountBTN.TextLineOne = AuxPatchers.EnterAmount();

            _closeBTN = GameObjectHelpers.FindGameObject(gameObject, "CloseBTN");
            InterfaceHelpers.CreateButton(_closeBTN, "DeleteBTN",InterfaceButtonMode.Background,
                OnButtonClick, Color.white, new Color(0, 1, 1), 2.5f);

            _repeatBTNObj = GameObjectHelpers.FindGameObject(gameObject, "RepeatBTN");
            _repeatBTN = _repeatBTNObj.AddComponent<FCSToggleButton>();
            _repeatBTN.OnButtonClick += OnButtonClick;
            _repeatBTN.BtnName = "RepeatBTN";

            InterfaceHelpers.CreateButton(GameObjectHelpers.FindGameObject(gameObject, "ArrowUp"), "ArrowUp", InterfaceButtonMode.Background,
                OnButtonClick, Color.white, new Color(0, 1, 1), 2.5f);

            InterfaceHelpers.CreateButton(GameObjectHelpers.FindGameObject(gameObject, "ArrowDown"), "ArrowDown", InterfaceButtonMode.Background,
                OnButtonClick, Color.white, new Color(0, 1, 1), 2.5f);

            InvokeRepeating(nameof(UpdateCount),.1f,.1f);

            _isInitialized = true;
        }

        private void UpdateCount()
        {
            _amount.text = $"x{_craftingItem?.Amount??0}";
        }

        private void OnButtonClick(string btnName, object Tag)
        {
            switch (btnName)
            {
                case "DeleteBTN":
                    Delete();
                    break;
                case "ArrowUp":
                    _controller.Move(_craftingItem, -1);
                    break;
                case "ArrowDown":
                    _controller.Move(_craftingItem, 1);
                    break;
                case "RepeatBTN":
                    _craftingItem.IsRecurring = _repeatBTN.IsSelected;
                    _amount.text = "x1";
                    _craftingItem.Amount = 1;
                    break;
            }

            UpdateIndex();
        }

        internal void UpdateIndex()
        {
            if(_position == null) return;
            _position.text = (_controller?.GetProductIndex(_craftingItem)+1).ToString();
        }

        private void Delete()
        {
            _controller.RemoveCraftingItem(_craftingItem);
            Reset();
        }

        internal void Set(CraftingItem craftingItem, DSSAutoCrafterDisplay controller)
        {
            Initialize();
            _craftingItem = craftingItem;
            _controller = controller;
            _icon.sprite = SpriteManager.Get(craftingItem.TechType);
            _text.text = Language.main.Get(craftingItem.TechType);
            if (craftingItem.IsRecurring)
            {
                _repeatBTN.Select();
            }
            Show();
            UpdateIndex();
        }

        private void SetIsRecurring(bool isRecurring)
        {
            _craftingItem.IsRecurring = isRecurring;
        }

        internal void Reset()
        {
            Initialize();
            _icon.sprite = SpriteManager.Get(TechType.None);
            _text.text = string.Empty;
            _repeatBTN.DeSelect();
            Hide();
        }

        internal void Hide()
        {
            gameObject.SetActive(false);
        }

        internal void Show()
        {
            gameObject.SetActive(true);
        }
    }
}