using System;
using System.Text;
using FCS_AlterraHub.Abstract;
using FCS_AlterraHub.Extensions;
using FCS_AlterraHub.Helpers;
using FCS_AlterraHub.Model;
using FCS_LifeSupportSolutions.Configuration;
using FCSCommon.Helpers;
using FCSCommon.Utilities;
using UnityEngine;
using UnityEngine.UI;
using WorldHelpers = FCS_AlterraHub.Helpers.WorldHelpers;

namespace FCS_LifeSupportSolutions.Mods.EnergyPillVendingMachine.mono
{
    internal class EnergyPillVendingMachineDisplay : AIDisplay
    {
        private Text _numberDisplay;
        private StringBuilder _sb;
        private EnergyPillVendingMachineController _mono;
        private Text _costLabel;



        internal void Setup(EnergyPillVendingMachineController mono)
        {
            _mono = mono;

            if (FindAllComponents())
            {
                _sb = new StringBuilder();
            }
        }

        public override void OnButtonClick(string btnName, object tag)
        {

        }

        public override bool FindAllComponents()
        {
            try
            {
                _numberDisplay = GameObjectHelpers.FindGameObject(gameObject, "InputField").GetComponentInChildren<Text>();
                
                var pillRed = GameObjectHelpers.FindGameObject(gameObject, "Pill_Red").AddComponent<FCSToolTip>();
                pillRed.RequestPermission += () => WorldHelpers.CheckIfInRange(pillRed.gameObject, Player.main.gameObject, 2);
                pillRed.ToolTipStringDelegate += () => "2 min. adrenaline boost.";
                var pillGreen = GameObjectHelpers.FindGameObject(gameObject, "Pill_Green").AddComponent<FCSToolTip>();
                pillGreen.RequestPermission += () => WorldHelpers.CheckIfInRange(pillGreen.gameObject, Player.main.gameObject, 2);
                pillGreen.ToolTipStringDelegate += () => "1 min. adrenaline boost.";
                var pillCyan = GameObjectHelpers.FindGameObject(gameObject, "Pill_Cyan").AddComponent<FCSToolTip>();
                pillCyan.RequestPermission += () => WorldHelpers.CheckIfInRange(pillCyan.gameObject, Player.main.gameObject, 2);
                pillCyan.ToolTipStringDelegate += () => "30 sec. adrenaline boost.";

                var purchaseBTNObj = GameObjectHelpers.FindGameObject(gameObject, "PurchaseBTN");
                var purchaseBTN = purchaseBTNObj.GetComponent<Button>();
                purchaseBTN.onClick.AddListener(() => { 
                    _mono.PurchaseItem(_sb.ToString()); 
                });
                var purchaseBTNText = purchaseBTNObj.GetComponentInChildren<Text>();
                purchaseBTNText.text = AuxPatchers.Purchase();
                
                var clearBTNObj = GameObjectHelpers.FindGameObject(gameObject, "ClearBTN");
                var clearBTN = clearBTNObj.GetComponent<Button>();
                clearBTN.onClick.AddListener(ClearDisplay);
                var clearBTNText = clearBTNObj.GetComponentInChildren<Text>();
                clearBTNText.text = AuxPatchers.Clear();

                _costLabel = GameObjectHelpers.FindGameObject(gameObject, "CostLabel").GetComponent<Text>();

                var dialPad = GameObjectHelpers.FindGameObject(gameObject, "DialPad").GetChildren();
                int index = 0;
                
                for (int i = 0; i < dialPad.Length; i++)
                {
                    var padBTN = dialPad[i].AddComponent<DialPadButton>();
                    padBTN.Initialize(this);
                    padBTN.Number = index;
                    index++;
                }
                
            }
            catch (Exception e)
            {
                QuickLogger.Debug(e.Message);
                QuickLogger.Debug(e.StackTrace);
                return false;
            }

            return true;
        }

        internal void AddItemToDisplay(int number)
        {
            var value = string.Empty;
            if (number < 11 && _sb.Length + 1 < 15 )
            {
                _sb.Append(number);
                value = _sb.ToString();
                _numberDisplay.text = value;
            }

            var result = _mono.TryGetPrice(value, out var cost);
            
            if (!result)
            {
                
            }

            UpdateCost(cost);
        }

        internal void UpdateCost(decimal amount)
        {
            _costLabel.text = AuxPatchers.CostFormat(amount);
        }

        private void ClearDisplay()
        {
            _numberDisplay.text = String.Empty;
            _costLabel.text = AuxPatchers.CostFormat(0);
            _sb.Clear();
        }

        public void ForceUpdateDisplay(string message)
        {
            _numberDisplay.text = message;
            _sb.Clear();
        }
    }

    internal class DialPadButton : MonoBehaviour
    {
        public int Number { get; set; }

        public void Initialize(EnergyPillVendingMachineDisplay display)
        {
            var btn = gameObject.GetComponent<Button>();
            btn.onClick.AddListener(() =>
            {
                display.AddItemToDisplay(Number);
            });
        }
    }
}
