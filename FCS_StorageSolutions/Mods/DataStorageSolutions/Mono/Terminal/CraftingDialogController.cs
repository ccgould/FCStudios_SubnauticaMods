using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FCS_AlterraHub.Buildables;
using FCS_AlterraHub.Helpers;
using FCS_AlterraHub.Interfaces;
using FCS_AlterraHub.Model;
using FCS_AlterraHub.Mono;
using FCS_AlterraHub.Mono.Controllers;
using FCS_StorageSolutions.Configuration;
using FCSCommon.Helpers;
using FCSCommon.Utilities;
using SMLHelper.V2.Crafting;
using SMLHelper.V2.Handlers;
using UnityEngine;
using UnityEngine.UI;

namespace FCS_StorageSolutions.Mods.DataStorageSolutions.Mono.Terminal
{
    internal class CraftingDialogController : MonoBehaviour
    {
        private uGUI_Icon _icon;
        private TechType _techType;
        private GridHelperV2 _grid;
        private readonly List<IngredientItem> _ingredientItem = new List<IngredientItem>();
        private Text _techTypeName;
        private Text _description;
        private Text _requirements;
        private Toggle _recursiveToggle;
        private int _amount = 1;
        private Button _iconBTN;
        private DSSTerminalDisplayManager _mono;
        private readonly StringBuilder _sb = new StringBuilder();
        private CrafterSectionController _crafterSectionController;
        private Text _craftingAmount;
        private ItemSelector _itemSelector;

        internal void Initialize(DSSTerminalDisplayManager mono, CrafterSectionController crafterSectionController)
        {
            _mono = mono;
            _crafterSectionController = crafterSectionController;
            var closeBTN = GameObjectHelpers.FindGameObject(gameObject, "CloseBTN").GetComponent<Button>();
            closeBTN.onClick.AddListener((Hide));

            _itemSelector = GameObjectHelpers.FindGameObject(gameObject, "ItemSelection").AddComponent<ItemSelector>();
            _itemSelector.Initialize(mono.GetController(),this);


            _grid = GameObjectHelpers.FindGameObject(gameObject, "Grid").EnsureComponent<GridHelperV2>();
            _grid.OnLoadDisplay += OnLoadItemsGrid;
            _grid.Setup(12, gameObject, Color.gray, Color.white, null);

            foreach (Transform child in _grid.transform)
            {
                _ingredientItem.Add(child.gameObject.EnsureComponent<IngredientItem>());
            }


            var icon = GameObjectHelpers.FindGameObject(gameObject, "Icon");
            _iconBTN = icon.GetComponent<Button>();
            _iconBTN.onClick.AddListener((() =>
            {
                _itemSelector.Show();
            }));

            _icon = icon.AddComponent<uGUI_Icon>();
            _icon.sprite = SpriteManager.defaultSprite;

            _techTypeName = GameObjectHelpers.FindGameObject(gameObject, "ItemTitle").GetComponent<Text>();
            _techTypeName.text = string.Empty;

            _description = GameObjectHelpers.FindGameObject(gameObject, "Description").GetComponent<Text>();
            _description.text = string.Empty;

            _requirements = GameObjectHelpers.FindGameObject(gameObject, "Requirements").GetComponent<Text>();
            _requirements.text = string.Empty;

            _recursiveToggle = gameObject.GetComponentInChildren<Toggle>();
            _recursiveToggle.onValueChanged.AddListener((value =>
            {
                _craftingAmount.text = value ? "\u221E" : _amount.ToString();
            }));

            _craftingAmount = GameObjectHelpers.FindGameObject(gameObject, "CraftingAmount").GetComponent<Text>();

            var confirmBTN = GameObjectHelpers.FindGameObject(gameObject, "ConfirmBTN").GetComponent<Button>();
            confirmBTN.onClick.AddListener(() =>
            {
                //if (!CheckIfAllIngredientsAvailable())
                //{
                //    _mono.GetController().ShowMessage(AuxPatchers.AllItemsNotAvailableToCraft());
                //}

                if (_techType == TechType.None)
                {
                    _mono.GetController().ShowMessage(AuxPatchers.PleaseSelectAnItemToCraft());
                    return;
                }

                mono.GetController().Manager.AddCraftingOperation(new CraftingOperation(_techType, _amount, _recursiveToggle.isOn));
                _crafterSectionController.Refresh();
                Hide();
            });

            var addBTN = GameObjectHelpers.FindGameObject(gameObject, "AddBTN").GetComponent<Button>();

            addBTN.onClick.AddListener((() =>
            {
                if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
                {
                    _amount += 10;
                }
                else
                {
                    _amount++;
                }

                if (_amount >= 100)
                {
                    _amount = 100;
                }
                _craftingAmount.text = _amount.ToString();
            }));

            var subtractBTN = GameObjectHelpers.FindGameObject(gameObject, "MinusBTN").GetComponent<Button>();

            subtractBTN.onClick.AddListener((() =>
            {
                if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
                {
                    _amount -= 10;
                }
                else
                {
                    _amount--;
                }

                if (_amount <= 1)
                {
                    _amount = 1;
                }

                _craftingAmount.text = _amount.ToString();
            }));

            InvokeRepeating(nameof(UpdateRequirements), 0.5f, 0.5f);
        }

        private bool CheckIfAllIngredientsAvailable()
        {
            return !_ingredientItem.Any(x => x.IsNotAvailable);
        }

        private void UpdateRequirements()
        {

            if (!_ingredientItem.Any()) return;

            _sb.Clear();

            foreach (IngredientItem item in _ingredientItem)
            {
                if (!item.IsNotAvailable) continue;
                _sb.Append($"{item.GetTechType()} x{item.GetAmount()}");
                _sb.Append(Environment.NewLine);
            }

            _requirements.text = _sb.ToString();
        }

        private void OnLoadItemsGrid(DisplayData data)
        {
            try
            {

                var grouped = TechDataHelpers.GetIngredients(_techType);

                if (data.EndPosition > grouped.Count)
                {
                    data.EndPosition = grouped.Count;
                }

                ResetIngredientItems();

                int w = 0;

                for (int i = data.StartPosition; i < data.EndPosition; i++)
                {
                    _ingredientItem[w++].Set(grouped.ElementAt(i), _mono.GetController().Manager);
                }

                _grid.UpdaterPaginator(grouped.Count);

            }
            catch (Exception e)
            {
                QuickLogger.Error("Error Caught");
                QuickLogger.Error($"Error Message: {e.Message}");
                QuickLogger.Error($"Error StackTrace: {e.StackTrace}");
            }
        }

        internal void Show()
        {
            gameObject.SetActive(true);
        }
        
        private void RefreshIngredients()
        {
            _grid.DrawPage();
        }

        internal void Hide()
        {
            gameObject.SetActive(false);
            Clear();
        }

        private void Clear()
        {
            _techType = TechType.None;
            _icon.sprite = SpriteManager.defaultSprite;
            _description.text = string.Empty;
            _techTypeName.text = string.Empty;
            _recursiveToggle.isOn = false;
            _requirements.text = string.Empty;
            _sb.Clear();

            ResetIngredientItems();
        }

        private void ResetIngredientItems()
        {
            foreach (IngredientItem ingredientItem in _ingredientItem)
            {
                ingredientItem.Reset();
            }
        }

        public bool SetItem(TechType item)
        {
            _techType = item;
            _icon.sprite = SpriteManager.Get(_techType);
            _techTypeName.text = Language.main.Get(_techType);
            _description.text = Language.main.Get(TooltipFactory.techTypeTooltipStrings.Get(_techType));

            _grid.DrawPage();
            return true;
        }
    }

    internal class ItemSelector : MonoBehaviour,IFCSDisplay
    {
        private List<CraftableItem> _craftableToggles = new();
        private GridHelperV2 _itemGrid;
        private TechType _selectedCraftable;
        private string _currentSearchString;
        private PaginatorController _paginatorController;
        private DSSTerminalController _mono;
        private CraftingDialogController _craftingDialogController;
        private const float _maxInteraction =3f;

        internal void Initialize(DSSTerminalController mono, CraftingDialogController craftingDialogController)
        {
            _mono = mono;
            _craftingDialogController = craftingDialogController;
            foreach (Transform craftableItem in GameObjectHelpers.FindGameObject(gameObject, "Grid").transform)
            {
                var craftableToggle = craftableItem.gameObject.EnsureComponent<CraftableItem>();
                craftableToggle.Initialize(mono);
                craftableToggle.OnButtonClick += OnToggleClick;
                _craftableToggles.Add(craftableToggle);
            }

            _itemGrid = mono.gameObject.AddComponent<GridHelperV2>();
            _itemGrid.OnLoadDisplay += OnLoadItemsGrid;
            _itemGrid.Setup(21, gameObject, Color.gray, Color.white, null);

            _paginatorController = GameObjectHelpers.FindGameObject(gameObject, "Paginator").AddComponent<PaginatorController>();
            _paginatorController.Initialize(this);

            #region Search
            var inputField = InterfaceHelpers.FindGameObject(gameObject, "InputField");
            var text = InterfaceHelpers.FindGameObject(inputField, "Placeholder")?.GetComponent<Text>();
            text.text = AlterraHub.SearchForItemsMessage();

            var searchField = inputField.AddComponent<SearchField>();
            searchField.OnSearchValueChanged += UpdateSearch;
            #endregion

            var doneBTN = GameObjectHelpers.FindGameObject(gameObject, "DoneBTN").GetComponent<Button>();
            var doneFBTN = doneBTN.gameObject.AddComponent<FCSButton>();
            doneFBTN.MaxInteractionRange = _maxInteraction;
            doneBTN.onClick.AddListener((() =>
            {
                _craftingDialogController.SetItem(_selectedCraftable);
                Hide();
            }));
        }

        private void Reset()
        {
            _selectedCraftable = TechType.None;
            foreach (CraftableItem craftableItem in _craftableToggles)
            {
                craftableItem.SetState(false);
            }
        }

        private void OnToggleClick(TechType techType, bool state)
        {
            _selectedCraftable = state ? techType : TechType.None;
        }

        private void OnLoadItemsGrid(DisplayData data)
        {
            try
            {
                if (_mono == null || _itemGrid == null || _paginatorController == null) return;
                WorldHelpers.GetCraftables();
                var grouped = WorldHelpers.Craftables;
                
                if (!string.IsNullOrEmpty(_currentSearchString?.Trim()))
                {
                    grouped = grouped.Where(p => TechTypeExtensions.Get(Language.main, p).ToLower().Contains(_currentSearchString.Trim().ToLower())).ToList();
                }

                if (data.EndPosition > grouped.Count)
                {
                    data.EndPosition = grouped.Count;
                }

                for (int i = 0; i < data.MaxPerPage; i++)
                {
                    _craftableToggles[i].Reset();
                }

                int w = 0;

                for (int i = data.StartPosition; i < data.EndPosition; i++)
                {
                    _craftableToggles[w++].Set(grouped[i], _selectedCraftable == grouped[i]);
                }

                _itemGrid.UpdaterPaginator(grouped.Count);
                _paginatorController.ResetCount(_itemGrid.GetMaxPages());
            }
            catch (Exception e)
            {
                QuickLogger.Error("Error Caught");
                QuickLogger.Error($"Error Message: {e.Message}");
                QuickLogger.Error($"Error StackTrace: {e.StackTrace}");
            }
        }

        private void UpdateSearch(string newSearch)
        {
            _currentSearchString = newSearch;
            _itemGrid.DrawPage();
        }

        public void Refresh()
        {
            _itemGrid?.DrawPage();
        }

        public void GoToPage(int index)
        {
            _itemGrid.DrawPage(index);
        }

        public void GoToPage(int index, PaginatorController sender)
        {
            
        }

        internal void Show()
        {
            gameObject.SetActive(true);
            _itemGrid.DrawPage();
        }

        internal void Hide()
        {
            gameObject.SetActive(false);
            Reset();
        }
    }

    internal class CraftableItem : MonoBehaviour
    {
        private uGUI_Icon _icon;
        private Toggle _button;
        private TechType _techType;
        private FCSToolTip _toolTip;
        private StringBuilder _sb = new StringBuilder();
        private StringBuilder _sb2 = new StringBuilder();
        private Dictionary<TechType, int> _ingredients = new Dictionary<TechType, int>();
        private DSSTerminalController _mono;

        private const float _maxInteraction = 3f;

        public Action<TechType, bool> OnButtonClick { get; set; }

        internal void Initialize(DSSTerminalController mono)
        {
            _mono = mono;
            _icon = GameObjectHelpers.FindGameObject(gameObject, "Icon").AddComponent<uGUI_Icon>();
            _button = gameObject.GetComponentInChildren<Toggle>();
            _button.onValueChanged.AddListener((value => { OnButtonClick?.Invoke(_techType, value); }));
            var fcsbutton = _button.gameObject.AddComponent<FCSButton>();
            fcsbutton.MaxInteractionRange = _maxInteraction;

            _toolTip = _button.gameObject.AddComponent<FCSToolTip>();
            _toolTip.RequestPermission += () => WorldHelpers.CheckIfInRange(gameObject, Player.main.gameObject, _maxInteraction);
            _toolTip.ToolTipStringDelegate += ToolTipStringDelegate;
        }

        private string ToolTipStringDelegate()
        {
            _sb.Clear();
            _sb.AppendFormat("\n<size=20><color=#FFA500FF>{0}</color></size>", $"{Language.main.Get(_techType)}");
            _sb.AppendFormat("\n<size=20><color=#ffffffff>{0}:</color> {1}</size>", "Ingredients", $"{BuildIngredients()}");
            return _sb.ToString();
        }

        private string BuildIngredients()
        {
            _sb2.Clear();
            foreach (KeyValuePair<TechType, int> ingredient in _ingredients)
            {
                var addSpace = _ingredients.Count > 1 ? "," : string.Empty;
                var hasIngredient = _mono.Manager.GetItemCount(ingredient.Key) >= ingredient.Value;
                var color = hasIngredient ? "00ff00ff" : "ff0000ff";
                _sb2.AppendFormat("\n<size=20><color=#{0}>{1} x{2}{3}</color></size>", color, Language.main.Get(ingredient.Key), ingredient.Value, addSpace);
            }

            return _sb2.ToString();
        }

        internal void Set(TechType techType, bool state)
        {
            _techType = techType;
            foreach (Ingredient ingredient in CraftDataHandler.GetTechData(techType).Ingredients)
            {
                _ingredients.Add(ingredient.techType, ingredient.amount);
            }
            //_toolTip.TechType = techType;
            _icon.sprite = SpriteManager.Get(techType);
            _button.SetIsOnWithoutNotify(state);
            gameObject.SetActive(true);
        }

        internal bool GetState()
        {
            return _button.isOn;
        }

        internal void SetState(bool state)
        {
            _button.isOn = state;
        }

        public void Reset()
        {
            _ingredients.Clear();
            _button.SetIsOnWithoutNotify(false);
            _icon.sprite = SpriteManager.Get(TechType.None);
            gameObject.SetActive(false);
        }


    }
}