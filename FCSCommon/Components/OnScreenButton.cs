using UnityEngine;
using UnityEngine.EventSystems;

namespace FCSCommon.Components
{
    /// <summary>
    /// Component that buttons on the power storage ui will inherit from. Handles working on whether something is hovered via IsHovered as well as interaction text. 
    /// </summary>
    /// <seealso cref="UnityEngine.MonoBehaviour" />
    internal abstract class OnScreenButton : MonoBehaviour
    {
        protected bool IsHovered { get; set; }
        internal string TextLineOne { get; set; }
        internal string TextLineTwo { get; set; }

        private bool isHoveredOutOfRange;
        internal bool Disabled { get; set; }
        internal float MaxInteractionRange { get; set; }  = 2.5f;


        public virtual void OnDisable()
        {
            this.IsHovered = false;
            isHoveredOutOfRange = false;
            Disabled = true;
        }

        public virtual void Update()
        {
            bool inInteractionRange = InInteractionRange();

            if (this.IsHovered && inInteractionRange)
            {
                if(string.IsNullOrEmpty(TextLineOne) && string.IsNullOrEmpty(TextLineTwo)) return;
                HandReticle.main.SetInteractTextRaw(this.TextLineOne, this.TextLineTwo);
            }

            if (this.IsHovered && inInteractionRange == false)
            {
                this.IsHovered = false;
            }

            if (this.IsHovered == false && isHoveredOutOfRange && inInteractionRange)
            {
                this.IsHovered = true;
            }
        }

        public virtual void OnPointerEnter(PointerEventData eventData)
        {
            if (InInteractionRange())
            {
                this.IsHovered = true;
            }

            isHoveredOutOfRange = true;
        }

        public virtual void OnPointerExit(PointerEventData eventData)
        {
            this.IsHovered = false;
            isHoveredOutOfRange = false;
        }

        public virtual void OnPointerClick(PointerEventData pointerEventData)
        {

        }

        protected bool InInteractionRange()
        {
            return Mathf.Abs(Vector3.Distance(this.gameObject.transform.position, Player.main.transform.position)) <= MaxInteractionRange;
        }

    }
}
