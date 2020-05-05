using System;
using DataStorageSolutions.Buildables;
using FCSCommon.Components;
using FCSCommon.Utilities;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace DataStorageSolutions.Mono
{
    internal class SearchField : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
    {
        private bool hover;
        private InputField _inputField;
        internal Action<string> OnSearchValueChanged;

        private void Update()
        {
            if (!hover) return;
            HandReticle.main.SetIcon(HandReticle.IconType.Rename);
#if SUBNAUTICA
            HandReticle.main.SetInteractTextRaw(AuxPatchers.SearchForItemsMessage(), "");
#elif BELOWZERO
            HandReticle.main.SetTextRaw(HandReticle.TextType.Hand, AuxPatchers.SearchForItemsMessage());
#endif
        }

        private void Awake()
        {
            _inputField = GetComponent<InputField>();
            _inputField.onValueChanged.AddListener(OnValueChanged);
            _inputField.onEndEdit.AddListener(OnEndEdit);
        }

        private void OnEndEdit(string arg0)
        {
            Player.main.playerController.SetEnabled(true);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            QuickLogger.Debug("Searching",true);
            Player.main.playerController.SetEnabled(false);
        }

        private void OnValueChanged(string newSearch)
        {
            OnSearchValueChanged?.Invoke(newSearch);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            hover = true;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            hover = false;
        }
    }
}
