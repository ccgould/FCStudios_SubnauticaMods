using System;
using System.Collections.Generic;
using System.Text;
using FCS_AlterraHub.Helpers;
using FCS_AlterraHub.Model;
using FCS_AlterraHub.Mono;
using FCSCommon.Helpers;
using SMLHelper.V2.Crafting;
using SMLHelper.V2.Handlers;
using UnityEngine;
using UnityEngine.UI;
using WorldHelpers = FCS_AlterraHub.Helpers.WorldHelpers;

namespace FCS_ProductionSolutions.Mods.AutoCrafter
{
    internal class CraftableItem : MonoBehaviour
    {
        private uGUI_Icon _icon;
        private Toggle _button;
        private TechType _techType;
        private FCSToolTip _toolTip;
        private StringBuilder _sb = new StringBuilder();
        private StringBuilder _sb2 = new StringBuilder();
        private Dictionary<TechType,int> _ingredients = new Dictionary<TechType, int>();
        private DSSAutoCrafterController _mono;
        private const float _maxInteraction = 0.9f;
        
        public Action<TechType,bool> OnButtonClick { get; set; }

        internal void Initialize(DSSAutoCrafterController mono)
        {
            _mono = mono;
            _icon = GameObjectHelpers.FindGameObject(gameObject, "Icon").AddComponent<uGUI_Icon>();
            _button = gameObject.GetComponentInChildren<Toggle>();
            _button.onValueChanged.AddListener((value => {OnButtonClick?.Invoke(_techType,value);}));
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
                _sb2.AppendFormat("\n<size=20><color=#{0}>{1} x{2}{3}</color></size>", color,Language.main.Get(ingredient.Key),ingredient.Value,addSpace);
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