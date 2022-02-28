using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FCSCommon.Utilities;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace FCS_AlterraHub.Mono
{
    public class SearchField : uGUI_InputGroup, uGUI_IButtonReceiver, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
    {
        private bool hover;
        private InputField _inputField;
        public Action<string> OnSearchValueChanged;
        public Action<string> OnEnterPressed;

        public string HoverMessage { get; set; } = Buildables.AlterraHub.SearchForItemsMessage();
        public bool IgnoreInputGroup { get; set; }


        public override void Update()
        {
            base.Update();

            if (focused && GameInput.GetButtonDown(GameInput.Button.PDA))
            {
                //if(IgnoreLocking) return;
                Deselect();
            }


            if (!hover) return;
            HandReticle.main.SetIcon(HandReticle.IconType.Rename);
#if SUBNAUTICA
            HandReticle.main.SetInteractTextRaw(HoverMessage, "");
#elif BELOWZERO
            HandReticle.main.SetTextRaw(HandReticle.TextType.Hand, Buildables.AlterraHub.SearchForItemsMessage());
#endif
        }


        public override void Awake()
        {
            base.Awake();
            _inputField = GetComponentInChildren<InputField>();
            _inputField.onValueChanged.AddListener(OnValueChanged);
            _inputField.onEndEdit.AddListener(OnEndEdit);
        }

        private void OnEndEdit(string text)
        {
            if(IgnoreInputGroup) return;
            QuickLogger.Debug("End Edit", true);
            LockMovement(false);
            InterceptInput(false);
            //OnEnterPressed?.Invoke(text);
        }

        public bool IsHovered()
        {
            return hover;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (IgnoreInputGroup) return;
            QuickLogger.Debug("Searching", true);
            InterceptInput(true);
            LockMovement(true);
            //Player.main.playerController.SetEnabled(false);
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

        public string GetText()
        {
            return _inputField.text;
        }
        
        public bool OnButtonDown(GameInput.Button button)
        {
            if (IgnoreInputGroup) return false;
            if (button == GameInput.Button.UICancel || button == GameInput.Button.PDA)
            {
                //if (IgnoreLocking) return true;
                Deselect();
                return true;
            }

            return false;
        }
    }
}
