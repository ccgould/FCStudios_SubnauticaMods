using FCS_AlterraHub.Helpers;
using FCS_AlterraHub.Mono;
using SMLHelper.Crafting;
using UnityEngine;
using UnityEngine.UI;

namespace FCS_AlterraHub.Model
{
    public class IngredientItem : MonoBehaviour
    {
        private bool _isInitialized;
        private Text _title;
        private uGUI_Icon _icon;
        private BaseManager _baseManager;
        private TechType _techType;
        private GameObject _isNotAvailable;
        private int _amount;
        public bool IsNotAvailable { get; private set; }

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

            if (_baseManager != null)
            {
                IsNotAvailable = _baseManager.GetItemCount(_techType) < _amount;
                _isNotAvailable.SetActive(IsNotAvailable);
            }
        }

        //Continue to set up the crafting item icon amount and hover
        public void Set(Ingredient ingredient, BaseManager baseManager)
        {
            Initialize();
            _baseManager = baseManager;
            _title.text = ingredient.amount.ToString();
            _icon.sprite = SpriteManager.Get(ingredient.techType);
            _techType = ingredient.techType;
            _amount = ingredient.amount;
            gameObject.SetActive(true);
        }

        public void Reset()
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
