using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FCS_AlterraHub.Helpers;
using FCS_AlterraHub.Interfaces;
using FCS_AlterraHub.Mono;
using FCS_AlterraHub.Mono.Controllers;
using FCS_StorageSolutions.Configuration;
using FCS_StorageSolutions.Mods.DataStorageSolutions.Mono.Terminal.Enumerators;
using FCSCommon.Helpers;
using FCSCommon.Utilities;
using UnityEngine;
using UnityEngine.UI;

namespace FCS_StorageSolutions.Mods.DataStorageSolutions.Mono.Terminal
{
    internal class CraftingPageController : MonoBehaviour, ITerminalPage, IFCSDisplay
    {
        private DSSTerminalDisplayManager _mono;
        private GridHelperV2 _grid;
        private List<CrafterListItem> _trackedCrafterListItem = new List<CrafterListItem>();
        private PaginatorController _paginatorController;
        private CrafterSectionController _crafterSection;

        public void Initialize(DSSTerminalDisplayManager mono)
        {
            _mono = mono;

            var craftersSideBarGrid = GameObjectHelpers.FindGameObject(gameObject, "CraftersSideBarGrid");

            foreach (Transform child in craftersSideBarGrid.transform)
            {
                _trackedCrafterListItem.Add(child.gameObject.EnsureComponent<CrafterListItem>());
            }

            _paginatorController = GameObjectHelpers.FindGameObject(gameObject, "Paginator").AddComponent<PaginatorController>();
            _paginatorController.Initialize(this);


            var backBTN = GameObjectHelpers.FindGameObject(gameObject, "BackBTN").GetComponent<Button>();
            backBTN.onClick.AddListener((() =>
            {
                Hide();
                _mono.GoToTerminalPage(TerminalPages.Configuration);
            }));

            _grid = craftersSideBarGrid.EnsureComponent<GridHelperV2>();
            _grid.OnLoadDisplay += OnLoadItemsGrid;
            _grid.Setup(8, gameObject, Color.gray, Color.white, null, "CraftersSideBarGrid");
            
            _crafterSection = GameObjectHelpers.FindGameObject(gameObject, "CrafterSection").AddComponent<CrafterSectionController>();
            _crafterSection.Initialize(mono);
        }

        public void Show()
        {
            gameObject.SetActive(true);
            _grid.DrawPage();
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }

        private void OnLoadItemsGrid(DisplayData data)
        {
            try
            {
                if (_mono == null) return;

                var grouped = _mono.GetController().Manager.GetDevices("ACU").ToList();
                
                if (data.EndPosition > grouped.Count)
                {
                    data.EndPosition = grouped.Count;
                }

                for (int i = 0; i < data.MaxPerPage; i++)
                {
                    _trackedCrafterListItem[i].Reset();
                }
                
                int w = 0;
                
                for (int i = data.StartPosition; i < data.EndPosition; i++)
                {
                    _trackedCrafterListItem[w++].Set(grouped.ElementAt(i).UnitID);
                }

                _grid.UpdaterPaginator(grouped.Count);
                _paginatorController.ResetCount(_grid.GetMaxPages());

            }
            catch (Exception e)
            {
                QuickLogger.Error("Error Caught");
                QuickLogger.Error($"Error Message: {e.Message}");
                QuickLogger.Error($"Error StackTrace: {e.StackTrace}");
            }
        }
        
        private class CrafterListItem : MonoBehaviour
        {
            private bool _isInitialized;
            private Text _title;

            private void Initialize()
            {
                if (_isInitialized) return;
                _title = gameObject.GetComponentInChildren<Text>();
                _isInitialized = true;
            }

            internal void Set(string name)
            {
                Initialize();
                _title.text = name;
                gameObject.SetActive(true);
            }

            internal void Reset()
            {
                gameObject.SetActive(false);
            }
        }

        public void GoToPage(int index)
        {
            _grid.DrawPage(index);
        }

        public void GoToPage(int index, PaginatorController sender)
        {
            _grid.DrawPage(index);
        }
    }

    internal class CrafterSectionController : MonoBehaviour,IFCSDisplay
    {
        private DSSTerminalDisplayManager _mono;
        private GridHelperV2 _grid;
        private List<CraftingOperationItem> _craftingOperationItem = new List<CraftingOperationItem>();
        private PaginatorController _paginatorController;
        private CraftingDialogController _craftingDialog;

        public void Initialize(DSSTerminalDisplayManager mono)
        {
            _mono = mono;

            _paginatorController = GameObjectHelpers.FindGameObject(gameObject, "Paginator").AddComponent<PaginatorController>();
            _paginatorController.Initialize(this);
           
            _grid = GameObjectHelpers.FindGameObject(gameObject, "Grid").EnsureComponent<GridHelperV2>();
            
            foreach (Transform child in _grid.transform)
            {
                _craftingOperationItem.Add(child.gameObject.EnsureComponent<CraftingOperationItem>());
            }

            _grid.OnLoadDisplay += OnLoadItemsGrid;
            _grid.Setup(28, gameObject, Color.gray, Color.white, null);

            _craftingDialog = GameObjectHelpers.FindGameObject(mono.gameObject, "CraftingDialog").AddComponent<CraftingDialogController>();
            _craftingDialog.Initialize(mono,this);

            var addCraftingBTN = GameObjectHelpers.FindGameObject(gameObject, "AddOperationBTN").GetComponent<Button>();
            
            addCraftingBTN.onClick.AddListener((() =>
            {
                _craftingDialog.Show();
            }));

        }

        private void OnLoadItemsGrid(DisplayData data)
        {
            try
            {
                if (_mono == null) return;

                var grouped = _mono.GetController().Manager.GetBaseCraftingOperations();
                QuickLogger.Debug("1");
                if (data.EndPosition > grouped.Count)
                {
                    data.EndPosition = grouped.Count;
                }
                QuickLogger.Debug("2");
                for (int i = 0; i < data.MaxPerPage; i++)
                {
                    _craftingOperationItem[i].Reset();
                }
                QuickLogger.Debug("3");
                int w = 0;
                QuickLogger.Debug("4");
                for (int i = data.StartPosition; i < data.EndPosition; i++)
                {
                    QuickLogger.Debug($"Crafting OP: {w} | Current Index: {i}");
                    _craftingOperationItem[w++].Set(grouped.ElementAt(i));
                }

                _grid.UpdaterPaginator(grouped.Count);
                _paginatorController.ResetCount(_grid.GetMaxPages());

            }
            catch (Exception e)
            {
                QuickLogger.Error("Error Caught");
                QuickLogger.Error($"Error Message: {e.Message}");
                QuickLogger.Error($"Error StackTrace: {e.StackTrace}");
            }
        }

        public void GoToPage(int index)
        {
            _grid.DrawPage(index);
        }

        public void GoToPage(int index, PaginatorController sender)
        {
            _grid.DrawPage(index);
        }

        private class CraftingOperationItem : MonoBehaviour
        {
            private bool _isInitialized;
            private Text _title;
            private uGUI_Icon _icon;
            protected CraftingOperation _craftingOperation;

            private void Initialize()
            {
                if (_isInitialized) return;
                _title = gameObject.GetComponentInChildren<Text>();
                _icon = GameObjectHelpers.FindGameObject(gameObject, "Icon").AddComponent<uGUI_Icon>();
                _isInitialized = true;
            }


            //Continue to set up the crafting item icon amount and hover

            internal void Set(CraftingOperation craftingOperation)
            {
                Initialize();
                _craftingOperation = craftingOperation;
                _title.text = craftingOperation.Amount.ToString();
                _icon.sprite = SpriteManager.Get(craftingOperation.TechType);
                gameObject.SetActive(true);
            }

            internal void Reset()
            {
                gameObject.SetActive(false);
            }
        }

        internal void Refresh()
        {
            _grid.DrawPage();
        }
    }

    internal class CraftingDialogController : MonoBehaviour, IFCSDumpContainer
    {
        private uGUI_Icon _icon;
        private TechType _techType;
        private GridHelperV2 _grid;
        private List<IngredientItem> _ingredientItem = new List<IngredientItem>();
        private Text _techTypeName;
        private Text _description;
        private Text _requirements;
        private Toggle _recursiveToggle;
        private int _amount = 1;
        private Button _iconBTN;
        private DumpContainerSimplified _dumpContainer;
        private DSSTerminalDisplayManager _mono;
        private StringBuilder _sb = new StringBuilder();
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
                if (!CheckIfAllIngredientsAvailable())
                {
                    _mono.GetController().ShowMessage(AuxPatchers.AllItemsNotAvailableToCraft());
                }

                if (_techType == TechType.None)
                {
                    _mono.GetController().ShowMessage(AuxPatchers.PleaseSelectAnItemToCraft());
                    return;
                }

                mono.GetController().Manager.AddCraftingOperation(new CraftingOperation
                {
                    Amount = _amount,
                    IsRecursive = _recursiveToggle.isOn,
                    TechType = _techType
                });
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
                    _ingredientItem[w++].Set(grouped.ElementAt(i), _mono);
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

    internal class IngredientItem : MonoBehaviour
    {
        private bool _isInitialized;
        private Text _title;
        private uGUI_Icon _icon;
        private DSSTerminalDisplayManager _mono;
        private TechType _techType;
        private GameObject _isNotAvailable;
        private int _amount;
        internal bool IsNotAvailable { get; private set; }

        private void Initialize()
        {

            if (_isInitialized) return;
            _title = gameObject.GetComponentInChildren<Text>();
            _icon = GameObjectHelpers.FindGameObject(gameObject, "Icon").AddComponent<uGUI_Icon>();
            _isNotAvailable = GameObjectHelpers.FindGameObject(gameObject, "NotAvailable");
            _isInitialized = true;
            InvokeRepeating(nameof(CheckIfAvailable), 0.5f, 0.5f);
        }

        private void CheckIfAvailable()
        {

            if (_techType == TechType.None)
            {
                IsNotAvailable = false;
                return;
            }

            if (_mono?.GetController()?.Manager != null)
            {
                IsNotAvailable =  !_mono.GetController().Manager.HasItem(_techType);
                _isNotAvailable.SetActive(IsNotAvailable);
            }
        }

        //Continue to set up the crafting item icon amount and hover

        internal void Set(IIngredient ingredient, DSSTerminalDisplayManager mono)
        {
            Initialize();
            _mono = mono;
            _title.text = ingredient.amount.ToString();
            _icon.sprite = SpriteManager.Get(ingredient.techType);
            _techType = ingredient.techType;
            _amount = ingredient.amount;
            gameObject.SetActive(true);
        }

        internal void Reset()
        {
            Initialize();
            _techType = TechType.None;
            _amount = 0;
            _icon.sprite = SpriteManager.defaultSprite;
            _title.text = string.Empty;
            _isNotAvailable.SetActive(false);
            gameObject.SetActive(false);
        }

        public TechType GetTechType() 
        {
            return _techType;
        }

        public int GetAmount()
        {
            return _amount;
        }
    }
}