using FCS_AlterraHub.Helpers;
using FCS_HomeSolutions.Mods.SeaBreeze.Display;
using FCSCommon.Helpers;
using FCSCommon.Utilities;
using UnityEngine;
using UnityEngine.UI;

namespace FCS_HomeSolutions.Mods.AlienChef.Mono
{
    internal class CookerItemDialog : MonoBehaviour
    {
        private Text _amountLbl;
        internal TechType TechType { get; private set; }
        private AlienChefController _mono;
        private Text _title;
        private CookerItemController _itemController;
        public int Amount { get; set; } = 1;

        internal void Setup(AlienChefController mono)
        {
            _mono = mono;
            var minusBTNObj = GameObjectHelpers.FindGameObject(gameObject, "subtractBTN");
            var minsBTN = minusBTNObj.EnsureComponent<InterfaceButton>();
            minsBTN.OnButtonClick += (s, o) =>
            {
                if (Amount > 1)
                {
                    Amount--;
                    _amountLbl.text = Amount.ToString("D2");
                }
            };

            var addBTNObj = GameObjectHelpers.FindGameObject(gameObject, "addBTN");
            var addBTN = addBTNObj.EnsureComponent<InterfaceButton>();
            addBTN.OnButtonClick += (s, o) =>
            {
                Amount++;
                _amountLbl.text = Amount.ToString("D2");
            };

            var addToOrderBTN = GameObjectHelpers.FindGameObject(gameObject, "AddToOrderBtn").GetComponent<Button>();
            addToOrderBTN.onClick.AddListener((() => {
                QuickLogger.Debug($"Adding {Amount} {TechType} to Order", true);
                var result = _mono.AttemptToCook(_itemController, Amount, false);
                if (result)
                {
                    mono.AddToOrder(_itemController, Amount);
                    Hide();
                }
                else
                {
                    QuickLogger.ModMessage("Alien Chef cant find all the required ingredients for this craft or is full");
                }
            }));

            var cookBTN = GameObjectHelpers.FindGameObject(gameObject, "CookBtn").GetComponent<Button>();
            cookBTN.onClick.AddListener((() => {
                var result =  _mono.AttemptToCook(_itemController,Amount);
                if (result)
                {
                    Hide();
                }
                else
                {
                    QuickLogger.ModMessage("Alien Chef cant find all the required ingredients for this craft or is full");
                }
            }));

            var closeBTN = GameObjectHelpers.FindGameObject(gameObject, "CloseBtn").GetComponent<Button>();
            closeBTN.onClick.AddListener((() => {
                Hide();
            }));

            _amountLbl = GameObjectHelpers.FindGameObject(gameObject, "Text").GetComponent<Text>();
            _title = GameObjectHelpers.FindGameObject(gameObject, "Name").GetComponent<Text>();
        }

        internal void Show(CookerItemController itemController)
        {
            _itemController = itemController;
            TechType = itemController.CookingItem.ReturnItem;
            _title.text = Language.main.Get(itemController.CookingItem.ReturnItem);
            _amountLbl.text = Amount.ToString("D2");
            gameObject.SetActive(true);
        }

        internal void Hide()
        {
            gameObject.SetActive(false);
            TechType = TechType.None;
            _title.text = string.Empty;
            Amount = 1;

        }

        public AlienChefController GetController()
        {
            return _mono;
        }
    }
}