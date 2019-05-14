using FCSAlterraIndustrialSolutions.Models.Abstract;
using UnityEngine;
using UnityEngine.EventSystems;

namespace FCSAlterraIndustrialSolutions.Models.Buttons
{
    /**
     * Component that buttons on the power storage ui will inherit from. Handles working on whether something is hovered via IsHovered as well as interaction text.
     */
    public abstract class OnScreenButton : MonoBehaviour
    {
        public AIDisplay Display { get; set; }
        protected bool IsHovered { get; set; }
        public string TextLineOne { get; set; }
        public string TextLineTwo { get; set; }
        private bool isHoveredOutOfRange;

        public virtual void OnDisable()
        {
            IsHovered = false;
            isHoveredOutOfRange = false;
        }

        public virtual void Update()
        {
            bool inInteractionRange = InInteractionRange();

            if (IsHovered && inInteractionRange)
            {
                HandReticle.main.SetInteractTextRaw(TextLineOne, TextLineTwo);
            }

            if (IsHovered && inInteractionRange == false)
            {
                IsHovered = false;
            }

            if (IsHovered == false && isHoveredOutOfRange && inInteractionRange)
            {
                IsHovered = true;
            }
        }

        public virtual void OnPointerEnter(PointerEventData eventData)
        {
            if (InInteractionRange())
            {
                IsHovered = true;
            }

            isHoveredOutOfRange = true;
        }

        public virtual void OnPointerExit(PointerEventData eventData)
        {
            IsHovered = false;
            isHoveredOutOfRange = false;
        }

        public virtual void OnPointerClick(PointerEventData eventData)
        {

        }

        protected bool InInteractionRange()
        {
            //if (Display == null)
            //{
            //    Log.Error("Display is null");
            //}

            //if (gameObject == null)
            //{
            //    Log.Error("gameObject is null");
            //}

            return Mathf.Abs(Vector3.Distance(gameObject.transform.position, Player.main.transform.position)) <= Display.MAX_INTERACTION_DISTANCE;
        }
    }
}
