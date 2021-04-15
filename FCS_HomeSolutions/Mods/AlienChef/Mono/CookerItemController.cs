using System.Collections.Generic;
using System.Text;
using FCS_HomeSolutions.SeaBreeze.Display;
using SMLHelper.V2.Crafting;
using SMLHelper.V2.Handlers;
using UnityEngine;
using UnityEngine.EventSystems;

namespace FCS_HomeSolutions.Mods.AlienChef.Mono
{
    internal class CookerItemController : OnScreenButton, IPointerEnterHandler, IPointerClickHandler, IPointerExitHandler
    {
        private GameObject _hover;
        private CookerItemDialog _dialog;
        private bool _isInitialized;
        private uGUI_Icon _icon;
        private StringBuilder _sb2 = new StringBuilder();

        private Dictionary<TechType, int> _ingredients = new Dictionary<TechType, int>();
        public CookerMode Mode { get; private set; }
        public CookingItem CookingItem { get; private set; }
        public void Initialize()
        {
            if(_isInitialized) return;
            _hover = gameObject.FindChild("Hover");
            _icon = gameObject.FindChild("Icon").EnsureComponent<uGUI_Icon>();
            _isInitialized = true;
        }

        public void Set(CookingItem cookingItem, CookerItemDialog dialog)
        {
            _dialog = dialog;
            CookingItem = cookingItem;
            Mode = cookingItem.CookerMode;
            _icon.sprite = SpriteManager.Get(cookingItem.ReturnItem);
            TextLineOne = Language.main.Get(cookingItem.ReturnItem);
            
            var craftData = CraftDataHandler.GetTechData(cookingItem.ReturnItem);
            if(craftData != null)
            {
                foreach (Ingredient ingredient in craftData.Ingredients)
                {
                    _ingredients.Add(ingredient.techType, ingredient.amount);
                }
            }
            TextLineTwo = BuildIngredients();
            Show();
        }

        private string BuildIngredients()
        {
            _sb2.Clear();
            foreach (KeyValuePair<TechType, int> ingredient in _ingredients)
            {
                var addSpace = _ingredients.Count > 1 ? "," : string.Empty;
                _sb2.AppendFormat("\n<size=20><color=white>{0} x{1}{2}</color></size>", Language.main.Get(ingredient.Key), ingredient.Value, addSpace);
            }

            return _sb2.ToString();
        }

        public override void Update()
        {
            base.Update();
            if (_hover == null) return;
            _hover.SetActive(CheckForIngredients(1));
        }

        internal bool CheckForIngredients(int amount)
        {
            var items = new Dictionary<TechType,int>();

            foreach (KeyValuePair<TechType, int> ingredient in _ingredients)
            {
                if (Inventory.main.container.Contains(ingredient.Key))
                {
                    if (items.ContainsKey(ingredient.Key))
                    {
                        items[ingredient.Key] += Inventory.main.container.GetCount(ingredient.Key);
                    }
                    else
                    {
                       items.Add(ingredient.Key,Inventory.main.container.GetCount(ingredient.Key)); 
                    }
                }

                if (_dialog.GetController().PullFromDataStorage)
                {
                    if (_dialog.GetController().Manager.HasItem(ingredient.Key))
                    {
                        if (items.ContainsKey(ingredient.Key))
                        {
                            items[ingredient.Key] += _dialog.GetController().Manager.GetItemCount(ingredient.Key);
                        }
                        else
                        {
                            items.Add(ingredient.Key, _dialog.GetController().Manager.GetItemCount(ingredient.Key));
                        }
                    }
                }
            }

            foreach (KeyValuePair<TechType, int> ingredient in _ingredients)
            {
                if (items.ContainsKey(ingredient.Key))
                {
                    if (items[ingredient.Key] < ingredient.Value * amount)
                    {
                        return false;
                    }
                }
                else
                {
                    return false;
                }
            }

            return true;
        }

        public override void OnPointerClick(PointerEventData pointerEventData)
        {
            base.OnPointerClick(pointerEventData);
            _dialog.Show(this);
        }

        public CookerItemDialog GetCookerItemDialog()
        {
            return _dialog;
        }

        public void Reset()
        {
            _icon.sprite = SpriteManager.defaultSprite;
            TextLineOne = string.Empty;
            _ingredients.Clear();
            
            Hide();
        }

        private void Hide()
        {
            gameObject.SetActive(false);
        }

        private void Show()
        {
            gameObject.SetActive(true);
        }

        public Dictionary<TechType, int> GetIngredients()
        {
            return _ingredients;
        }

        public void Consume()
        {
            foreach (KeyValuePair<TechType, int> ingredient in _ingredients)
            {
                for (int i = 0; i < ingredient.Value; i++)
                {
                    if (Inventory.main.container.Contains(ingredient.Key))
                    {
                        Destroy(Inventory.main.container.RemoveItem(ingredient.Key));
                    }

                    if (_dialog.GetController().PullFromDataStorage)
                    {
                        if (_dialog.GetController().Manager.HasItem(ingredient.Key))
                        {
                            Destroy(_dialog.GetController().Manager.TakeItem(ingredient.Key));
                        }
                    }
                }
            }
        }
    }

    internal struct CookingItem
    {
        public TechType TechType { get; set; }
        public TechType ReturnItem { get; set; }
        public CookerMode CookerMode { get; set; }
    }
}