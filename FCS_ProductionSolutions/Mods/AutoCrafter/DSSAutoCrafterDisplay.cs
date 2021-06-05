using System;
using System.Collections.Generic;
using System.Text;
using FCS_AlterraHub.Abstract;
using FCS_AlterraHub.Helpers;
using FCS_AlterraHub.Model;
using FCS_AlterraHub.Mono;
using FCS_ProductionSolutions.Buildable;
using FCS_ProductionSolutions.Mods.AutoCrafter.Interfaces;
using FCSCommon.Helpers;
using FCSCommon.Utilities;
using UnityEngine;
using UnityEngine.UI;

namespace FCS_ProductionSolutions.Mods.AutoCrafter
{
    internal class DSSAutoCrafterDisplay : AIDisplay
    {
        private DSSAutoCrafterController _mono;
        private GameObject _canvas;
        private uGUI_Icon _currentCraftingItemIcon;
        private uGUI_Icon _targetItemIcon;
        private GridHelperV2 _ingredientsGrid;
        private Text _reqItemsList;
        public Action OnCancelBtnClick { get; set; }
        private Button _standbyBTN;
        private readonly StringBuilder _sb = new StringBuilder();
        private List<IngredientItem> _ingredientItems = new List<IngredientItem>();
        private ManualPageController _manualPageController;
        private HomePageController _homePageController;
        private Text _status;
        private Text _total;
        private FCSMessageBox _messageBox;
        private StandByPageController _standbyPageController;

        internal GameObject ManualPage { get; private set; }
        internal GameObject HomePage { get; private set; }
        internal GameObject AutomaticPage { get; private set; }
        public Action<string> OnStatusUpdate { get; set; }
        public GameObject StandByPage { get; set; }
        public Action<int, int> OnTotalUpdate { get; set; }
        public Action OnLoadComplete { get; set; }
        public Action<string> OnMessageReceived { get; set; }

        internal void Setup(DSSAutoCrafterController mono)
        {
            _mono = mono;

            if (FindAllComponents())
            {
                _manualPageController.Refresh();
            }
        }
        
        public override void OnButtonClick(string btnName, object tag)
        {

        }

        public override bool FindAllComponents()
        {
            try
            {
                ManualPage = GameObjectHelpers.FindGameObject(gameObject, "ManualPage");
                HomePage = GameObjectHelpers.FindGameObject(gameObject, "Main");
                AutomaticPage = GameObjectHelpers.FindGameObject(gameObject, "AutomaticPage");
                StandByPage = GameObjectHelpers.FindGameObject(gameObject, "StandBySettingsPage");

                _canvas = gameObject.GetComponentInChildren<Canvas>().gameObject;

                var ingredientsGrid = GameObjectHelpers.FindGameObject(gameObject, "Frame").FindChild("Grid");

                #region CurrentCraftingItemIcon

                _currentCraftingItemIcon = GameObjectHelpers.FindGameObject(gameObject, "CurrentCraftingItemIcon")
                    .FindChild("Icon").AddComponent<uGUI_Icon>();
                _currentCraftingItemIcon.sprite = SpriteManager.defaultSprite;

                #endregion

                #region CurrentItemIcon

                _targetItemIcon = GameObjectHelpers.FindGameObject(gameObject, "ItemIcon")
                    .FindChild("Icon").AddComponent<uGUI_Icon>();
                _targetItemIcon.sprite = SpriteManager.defaultSprite;
                #endregion

                _reqItemsList = GameObjectHelpers.FindGameObject(gameObject, "RequirementsNotMetInformation").GetComponent<Text>();

                foreach (Transform child in ingredientsGrid.transform)
                {
                    _ingredientItems.Add(child.gameObject.EnsureComponent<IngredientItem>());
                }

                #region LoadIngredients


                _ingredientsGrid = ingredientsGrid.AddComponent<GridHelperV2>();
                _ingredientsGrid.OnLoadDisplay += OnLoadIngredientsGrid;
                _ingredientsGrid.Setup(9, gameObject, Color.gray, Color.white, null);

                #endregion

                var cancelBTN = GameObjectHelpers.FindGameObject(gameObject, "CancelBTN").GetComponent<Button>();
                var cancelFBTN = cancelBTN.gameObject.AddComponent<FCSButton>();
                cancelFBTN.ShowMouseClick = true;
                cancelFBTN.TextLineOne = "Cancel";
                cancelFBTN.TextLineTwo = "Cancels crafting operation.";
                cancelBTN.onClick.AddListener(() =>
                {
                    OnCancelBtnClick?.Invoke();
                });


                var backBTN = GameObjectHelpers.FindGameObject(gameObject, "BackBTN").GetComponent<Button>();
                var backFBTN = backBTN.gameObject.AddComponent<FCSButton>();
                backFBTN.ShowMouseClick = true;
                backFBTN.TextLineOne = "Back";
                backBTN.onClick.AddListener(() =>
                {
                    GoToPage(AutoCrafterPages.Home);
                });

                var standbyToggle = GameObjectHelpers.FindGameObject(AutomaticPage, "StandByBTN");
                _standbyBTN = standbyToggle.GetComponent<Button>();
                var standByBtn = standbyToggle.AddComponent<FCSButton>();
                standByBtn.ShowMouseClick = true;
                standByBtn.TextLineOne = "StandBy";
                standByBtn.TextLineTwo = "Puts this crafter in a mode that allows it to help other crafters to craft missing required items.";
                _standbyBTN.onClick.AddListener(()=>
                {
                    if (_mono.HasConnectedCrafter() && _mono.CurrentCrafterMode != AutoCrafterMode.StandBy)
                    {
                        _mono.ShowMessage(AuxPatchers.CannotSetStandByHasConnections(GetConnectedCraftersIds()));
                        return;
                    }
                    GoToPage(AutoCrafterPages.StandBy);
                });

                _status = GameObjectHelpers.FindGameObject(AutomaticPage, "Status").GetComponent<Text>();
                _total = GameObjectHelpers.FindGameObject(AutomaticPage, "Total").GetComponent<Text>();

                _standbyPageController = StandByPage.AddComponent<StandByPageController>();
                _standbyPageController.Initialize(this);

                _homePageController = HomePage.AddComponent<HomePageController>();
                _homePageController.Initialize(this);

                _manualPageController = ManualPage.AddComponent<ManualPageController>();
                _manualPageController.Initialize(this);

                _messageBox = GameObjectHelpers.FindGameObject(gameObject, "MessageBox").AddComponent<FCSMessageBox>();

                OnMessageReceived += s =>
                {
                    _messageBox.Show(s,FCSMessageButton.OK,null);
                };

                OnStatusUpdate += status =>
                {
                    _status.text = status.ToUpper();
                };

                OnTotalUpdate += (amount, maxAmount) => { _total.text = $"{amount}/{maxAmount}"; };
            }
            catch (Exception e)
            {
                QuickLogger.Error(e.StackTrace);
                QuickLogger.Error(e.Message);
                return false;
            }

            return true;
        }

        private string GetConnectedCraftersIds()
        {
            var sb = new StringBuilder();
            foreach (var connection in _mono.GetConnections())
            {
                sb.Append($"({connection} )");
            }

            return sb.ToString();
        }

        internal void GoToPage(AutoCrafterPages page)
        {
            switch (page)
            {
                case AutoCrafterPages.Home:
                    _manualPageController.Hide();
                    _homePageController.Show();
                    _standbyPageController.Hide();
                    AutomaticPage.SetActive(false);
                    break;
                case AutoCrafterPages.Automatic:
                    _manualPageController.Hide();
                    _homePageController.Hide();
                    _standbyPageController.Hide();
                    AutomaticPage.SetActive(true);
                    break;
                case AutoCrafterPages.Manual:
                    _manualPageController.Show();
                    _homePageController.Hide();
                    _standbyPageController.Hide();
                    AutomaticPage.SetActive(false);
                    break;
                case AutoCrafterPages.StandBy:
                    _manualPageController.Hide();
                    _homePageController.Hide();
                    _standbyPageController.Show();
                    AutomaticPage.SetActive(false);
                    break;
            }
        }
        
        internal void SetStandByState(bool state,bool notify = false)
        {
            _standbyPageController.SetStandByState(state, notify);
        }

        internal void Clear()
        {
            ClearMissingItem();
            _targetItemIcon.sprite = SpriteManager.defaultSprite;
            ResetIngredientItems();
            OnTotalUpdate?.Invoke(0,0);
            _ingredientsGrid.DrawPage();
        }
        
        private void OnLoadIngredientsGrid(DisplayData data)
        {
            try
            {
                if (_mono?.Manager == null || _mono?.CraftManager == null || _ingredientItems == null || _mono?.CraftManager?.GetCraftingOperation() == null)
                {
                    ResetIngredientItems();
                    return;
                }


                var grouped = TechDataHelpers.GetIngredients(_mono.CraftManager.GetCraftingOperation().TechType);

                if(grouped==null) return;
                
                if (data.EndPosition > grouped.Count)
                {
                    data.EndPosition = grouped.Count;
                }

                ResetIngredientItems();

                for (int i = data.StartPosition; i < data.EndPosition; i++)
                {
                    _ingredientItems[i]?.Set(grouped[i], _mono?.Manager);
                }

            }
            catch (Exception e)
            {
                QuickLogger.Error("Error Caught");
                QuickLogger.Error($"Error Message: {e.Message}");
                QuickLogger.Error($"Error StackTrace: {e.StackTrace}");
            }
        }

        private void ResetIngredientItems()
        {
            for (int i = 0; i < 9; i++)
            {
                _ingredientItems[i]?.Reset();
            }
        }

        internal void LoadCraft(CraftingOperation operation)
        {
            _targetItemIcon.sprite = SpriteManager.Get(operation.TechType);
            _ingredientsGrid.DrawPage();

        }

        public override void TurnOffDisplay()
        {
            _canvas.SetActive(false);
        }

        public override void TurnOnDisplay()
        {
            _canvas.SetActive(true);
        }

        public void ClearMissingItem()
        {
            _sb.Clear();
            _reqItemsList.text = string.Empty;
        }

        public void AddMissingItem(string item, int amount)
        {
            _sb.Append($"{item} x{amount}");
            _sb.Append(Environment.NewLine);
            _reqItemsList.text = _sb.ToString();
        }

        public override void GoToPage(int index)
        {
            _manualPageController.GoToPage(index);
        }

        public DSSAutoCrafterController GetController()
        {
            return _mono;
        }

        public IPageController GetPageController(AutoCrafterPages standBy)
        {
            switch (standBy)
            {
                case AutoCrafterPages.Home:
                    return _homePageController;
                case AutoCrafterPages.Automatic:
                    return null;
                case AutoCrafterPages.Manual:
                    return _manualPageController;
                case AutoCrafterPages.StandBy:
                    return _standbyPageController;
            }

            return null;
        }
    }

    internal enum StandByModes
    {
        Crafting,
        Load
    }

    internal enum AutoCrafterPages
    {
        Home = 0,
        Automatic = 1,
        Manual = 2,
        StandBy = 3
    }
}