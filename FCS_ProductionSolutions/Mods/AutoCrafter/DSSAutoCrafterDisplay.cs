using System;
using System.Collections.Generic;
using System.Text;
using FCS_AlterraHub.Model;
using FCS_AlterraHub.Mono;
using FCSCommon.Abstract;
using FCSCommon.Helpers;
using FCSCommon.Utilities;
using FMOD;
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
        private List<IngredientItem> _ingredientItems = new List<IngredientItem>();
        private Text _reqItemsList;
        private readonly StringBuilder _sb = new StringBuilder();

        internal void Setup(DSSAutoCrafterController mono)
        {
            _mono = mono;
            if (FindAllComponents())
            {
            }
        }
        
        public override void OnButtonClick(string btnName, object tag)
        {

        }

        public override bool FindAllComponents()
        {
            try
            {
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

                
                _ingredientsGrid = ingredientsGrid.EnsureComponent<GridHelperV2>();
                _ingredientsGrid.OnLoadDisplay += OnLoadIngredientsGrid;
                _ingredientsGrid.Setup(9, gameObject, Color.gray, Color.white, null);

                #endregion

            }
            catch (Exception e)
            {
                QuickLogger.Error(e.StackTrace);
                QuickLogger.Error(e.Message);
                return false;
            }

            return true;
        }

        private void OnLoadIngredientsGrid(DisplayData data)
        {
            try
            {
                if (_mono == null) return;

                var grouped = _mono.GetCraftingItem()?.Ingredients;

                if(grouped==null)return;
                
                if (data.EndPosition > grouped.Count)
                {
                    data.EndPosition = grouped.Count;
                }

                for (int i = data.EndPosition; i < data.MaxPerPage - 1; i++)
                {
                    _ingredientItems[i].Reset();
                }

                for (int i = data.StartPosition; i < data.EndPosition; i++)
                {
                    _ingredientItems[i].Set(grouped[i], _mono.Manager);
                }

            }
            catch (Exception e)
            {
                QuickLogger.Error("Error Caught");
                QuickLogger.Error($"Error Message: {e.Message}");
                QuickLogger.Error($"Error StackTrace: {e.StackTrace}");
            }
        }

        internal void LoadCraft(CraftingOperation operation)
        {
            _targetItemIcon.sprite = SpriteManager.Get(operation.TechType);
            _ingredientsGrid.DrawPage();

        }

        public void Refresh()
        {

        }

        public void RemoveCraftingItem(CraftingOperation craftingItem)
        {

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
        }

        public void AddMissingItem(string item, int amount)
        {
            _sb.Append($"{item} x{amount}");
            _sb.Append(Environment.NewLine);
            _reqItemsList.text = _sb.ToString();
        }
    }
}