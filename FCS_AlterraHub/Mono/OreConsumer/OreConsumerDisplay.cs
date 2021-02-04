using System;
using FCS_AlterraHub.Systems;
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
        internal Action<decimal> onTotalChanged;
        internal UnityEvent onDumpButtonClicked = new UnityEvent();
        private OreConsumerController _mono;
        private Text _totalAmount;
        private CardSystem cardSystem => CardSystem.main;
        public CheckInteraction CheckInteraction { get; private set; }

        internal void Setup(OreConsumerController mono)
        {
            _mono = mono;
            if(FindAllComponents())
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
                if(!CardSystem.main.HasBeenRegistered())
                {
                    QuickLogger.ModMessage(Buildables.AlterraHub.AccountNotFoundFormat());
                    return;
                }

                if (!_mono.IsOnPlatform)
                {
                    QuickLogger.ModMessage(Buildables.AlterraHub.PleaseBuildOnPlatForm());
                    return;
                }

                onDumpButtonClicked?.Invoke();
            });

        }

        private void FindTotalLabel()
        {
            _totalAmount = GameObjectHelpers.FindGameObject(gameObject, "TotalAmount").GetComponent<Text>();
            onTotalChanged += total =>
            {
                if(_totalAmount != null && _mono.IsOperational && CardSystem.main.HasBeenRegistered())
                    _totalAmount.text = total.ToString("n0");
            };

        }

        private void SetTotalLabel()
        {
            GameObjectHelpers.FindGameObject(gameObject, "Total").GetComponent<Text>().text = Buildables.AlterraHub.AccountTotal();
        }

        public void ForceRefresh(decimal total)
        {
            if (_totalAmount != null)
            {
                _totalAmount.text = total.ToString("n0");
            }
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
