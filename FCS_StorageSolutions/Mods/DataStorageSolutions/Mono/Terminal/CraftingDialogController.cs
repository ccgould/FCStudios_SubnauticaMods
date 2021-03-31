using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FCS_AlterraHub.Helpers;
using FCS_AlterraHub.Interfaces;
using FCS_AlterraHub.Model;
using FCS_AlterraHub.Mono;
using FCS_StorageSolutions.Configuration;
using FCSCommon.Helpers;
using FCSCommon.Utilities;
using UnityEngine;
using UnityEngine.UI;

namespace FCS_StorageSolutions.Mods.DataStorageSolutions.Mono.Terminal
{
    internal class CraftingDialogController : MonoBehaviour, IFCSDumpContainer
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
        private DumpContainerSimplified _dumpContainer;
        private DSSTerminalDisplayManager _mono;
        private readonly StringBuilder _sb = new StringBuilder();
        private CrafterSectionController _crafterSectionController;
        private Text _craftingAmount;

        internal void Initialize(DSSTerminalDisplayManager mono, CrafterSectionController crafterSectionController)
        {
            _mono = mono;
            _crafterSectionController = crafterSectionController;
            var closeBTN = GameObjectHelpers.FindGameObject(gameObject, "CloseBTN").GetComponent<Button>();
            closeBTN.onClick.AddListener((Hide));

            _grid = GameObjectHelpers.FindGameObject(gameObject, "Grid").EnsureComponent<GridHelperV2>();
            _grid.OnLoadDisplay += OnLoadItemsGrid;
            _grid.Setup(12, gameObject, Color.gray, Color.white, null);

            foreach (Transform child in _grid.transform)
            {
                _ingredientItem.Add(child.gameObject.EnsureComponent<IngredientItem>());
            }


            var icon = GameObjectHelpers.FindGameObject(gameObject, "Icon");
            _iconBTN = icon.GetComponent<Button>();
            _iconBTN.onClick.AddListener((() => {
                _dumpContainer.OpenStorage();
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
                if (_amount == 10)
                {
                    return;
                }
                _amount++;
                _craftingAmount.text = _amount.ToString();
            }));

            var subtractBTN = GameObjectHelpers.FindGameObject(gameObject, "MinusBTN").GetComponent<Button>();

            subtractBTN.onClick.AddListener((() =>
            {
                if (_amount == 0) return;
                _amount--;
                _craftingAmount.text = _amount.ToString();
            }));

            InvokeRepeating(nameof(UpdateRequirements),0.5f,0.5f);

            if (_dumpContainer != null) return;
            _dumpContainer = gameObject.EnsureComponent<DumpContainerSimplified>();
            _dumpContainer.Initialize(gameObject.transform, "Select item to craft.", this, 6, 8);
        }

        private bool CheckIfAllIngredientsAvailable()
        {
            return !_ingredientItem.Any(x => x.IsNotAvailable);
        }

        private void UpdateRequirements()
        {
            
            if(!_ingredientItem.Any()) return;

            _sb.Clear();

            foreach (IngredientItem item in _ingredientItem)
            {
                if(!item.IsNotAvailable) continue;
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

        internal void Set(TechType techType)
        {
            _icon.sprite = SpriteManager.Get(techType);
            _techType = techType;
            RefreshIngredients();
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
        
        public bool AddItemToContainer(InventoryItem item)
        {
            PlayerInteractionHelper.GivePlayerItem(item);
            
            _techType = item.item.GetTechType();

            
            _icon.sprite = SpriteManager.Get(_techType);
            _techTypeName.text = Language.main.Get(_techType);
            _description.text = Language.main.Get(TooltipFactory.techTypeTooltipStrings.Get(_techType));

            _grid.DrawPage();
            return true;
        }

        public bool IsAllowedToAdd(TechType techType, bool verbose)
        {
            return TechDataHelpers.ContainsValidCraftData(techType);
        }

        public bool IsAllowedToAdd(Pickupable pickupable, bool verbose)
        {
            return TechDataHelpers.ContainsValidCraftData(pickupable.GetTechType());
        }
    }
}