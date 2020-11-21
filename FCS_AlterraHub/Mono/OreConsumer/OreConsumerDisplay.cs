using System;
using FCSCommon.Abstract;
using FCSCommon.Helpers;
using FCSCommon.Utilities;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace FCS_AlterraHub.Mono.OreConsumer
{
    internal class OreConsumerDisplay : AIDisplay
    {
        private Text _totalAmount;
        internal Action<decimal> onTotalChanged;
        internal UnityEvent onDumpButtonClicked = new UnityEvent();

        public CheckInteraction CheckInteraction { get; private set; }

        internal void Setup(OreConsumerController mono)
        {
            if(FindAllComponents())
            { 
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


                FindTotalLabel();

                CreateDumpButton();
            }
            catch (Exception e)
            {
                QuickLogger.DebugError($"Message: {e.Message} || StackTrace: {e.StackTrace}");
                return false;
            }

            return true;
        }
        
        private void CreateDumpButton()
        {
            var addBTN = GameObjectHelpers.FindGameObject(gameObject, "AddBTN");
            CheckInteraction = addBTN.AddComponent<CheckInteraction>();
            var dumpButton = addBTN.GetComponent<Button>();
            dumpButton.onClick.AddListener(() =>
            {
                onDumpButtonClicked?.Invoke();
            });

        }

        private void FindTotalLabel()
        {
            _totalAmount = GameObjectHelpers.FindGameObject(gameObject, "TotalAmount").GetComponent<Text>();
        }

        private void SetTotalLabel()
        {
            GameObjectHelpers.FindGameObject(gameObject, "Total").GetComponent<Text>().text = Buildables.AlterraHub.AccountTotal();
        }
    }

    internal class CheckInteraction : MonoBehaviour, IPointerHoverHandler,IPointerExitHandler
    {
        public void OnPointerHover(PointerEventData eventData)
        {
            IsHovered = true;
        }

        public bool IsHovered { get; set; }
        public void OnPointerExit(PointerEventData eventData)
        {
            IsHovered = false;
        }
    }
}
