using System;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace FCSAlterraShipping.Mono
{
    internal class FCSTextBoxController : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
    {
        private bool hover;
        [SerializeField]
        private Text _textBox;
        public Action OnLabelChanged;
        private string _description;
        private string _onHover;
        private Func<int> _getMethod;
        private Action<int> _setMethod;


        private void Initialize(string description, string onHover, GameObject textPrefab, Func<int> getMethod, Action<int> setMethod)
        {
            _description = description;
            _onHover = onHover;
            _textBox = textPrefab.GetComponent<Text>();
            _getMethod = getMethod;
            _setMethod = setMethod;

        }

        public void OnPointerClick(PointerEventData eventData)
        {
            uGUI.main.userInput.RequestString(_description, "Submit", GetAmount(), 25, new uGUI_UserInput.UserInputCallback(SetLabel));
        }

        private string GetAmount()
        {
            return _getMethod.Invoke().ToString();
        }

        internal void SetLabel(string newLabel)
        {
            var str = GetNumbers(newLabel);
            int result = int.Parse(str);
            _setMethod?.Invoke(result);
            _textBox.text = str;
            OnLabelChanged?.Invoke();
        }

        private static string GetNumbers(string input)
        {
            var str = input.Where(char.IsDigit).ToArray();
            return str.Length != 0 ? new string(str) : "0";
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            hover = true;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            hover = false;
        }

        private void Update()
        {
            if (!hover) return;
            HandReticle.main.SetIcon(HandReticle.IconType.Rename);
            HandReticle.main.SetInteractTextRaw(_onHover, "");
        }

        public static FCSTextBoxController Create(string description, string onHoverText, GameObject textPrefab, Func<int> getMethod, Action<int> setMethod)
        {
            var textBoxController = textPrefab.GetComponent<FCSTextBoxController>();
            textBoxController.Initialize(description, onHoverText, textPrefab, getMethod, setMethod);
            return textBoxController;
        }
    }
}
