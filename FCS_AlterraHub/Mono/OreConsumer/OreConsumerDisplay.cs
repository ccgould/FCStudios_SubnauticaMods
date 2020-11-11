using System;
using FCSCommon.Abstract;
using FCSCommon.Helpers;
using FCSCommon.Utilities;
using UnityEngine.Events;
using UnityEngine.UI;

namespace FCS_AlterraHub.Mono.OreConsumer
{
    internal class OreConsumerDisplay : AIDisplay
    {
        private Text _totalAmount;
        internal Action<decimal> onTotalChanged;
        internal UnityEvent onDumpButtonClicked = new UnityEvent();
        internal UnityEvent onTransferMoneyClicked = new UnityEvent();

        internal void Setup(OreConsumerController mono)
        {
            if(FindAllComponents())
            {
                mono.IsOperational = true;
                onTotalChanged += total => { _totalAmount.text = total.ToString("n0"); };
            }
        }

        public override void OnButtonClick(string btnName, object tag)
        {
            
        }

        public override bool FindAllComponents()
        {
            try
            {
                SetTotalLabel();

                SetTransferMoneyButtonText();

                FindTotalLabel();

                CreateDumpButton();

                CreateTransferMoneyButton();
            }
            catch (Exception e)
            {
                QuickLogger.DebugError($"Message: {e.Message} || StackTrace: {e.StackTrace}");
                return false;
            }

            return true;
        }

        private void CreateTransferMoneyButton()
        {
            var transferButton = GameObjectHelpers.FindGameObject(gameObject, "TransferMoney").GetComponent<Button>();
            transferButton.onClick.AddListener(() => { onTransferMoneyClicked?.Invoke(); });
        }

        private void CreateDumpButton()
        {
            var dumpButton = GameObjectHelpers.FindGameObject(gameObject, "AddBTN").GetComponent<Button>();
            dumpButton.onClick.AddListener(() => { onDumpButtonClicked?.Invoke(); });
        }

        private void FindTotalLabel()
        {
            _totalAmount = GameObjectHelpers.FindGameObject(gameObject, "TotalAmount").GetComponent<Text>();
        }

        private void SetTransferMoneyButtonText()
        {
            GameObjectHelpers.FindGameObject(gameObject, "Text").GetComponent<Text>().text =
                Buildables.AlterraHub.TransferMoney();
        }

        private void SetTotalLabel()
        {
            GameObjectHelpers.FindGameObject(gameObject, "Total").GetComponent<Text>().text = Buildables.AlterraHub.Total();
        }
    }
}
