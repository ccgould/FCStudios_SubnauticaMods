using FCS_Alterra_Refrigeration_Solutions.Logging;
using FCS_Alterra_Refrigeration_Solutions.Models.Components;
using FCS_Alterra_Refrigeration_Solutions.Models.Interfaces;
using UnityEngine;
using UnityEngine.EventSystems;

namespace FCS_Alterra_Refrigeration_Solutions.Models.Base
{
    /**
     * Component that buttons on the power storage ui will inherit from. Handles working on whether something is hovered via IsHovered as well as interaction text.
     */
    public abstract class OnScreenButton : MonoBehaviour
    {
        private IARSolutionDisplay<ARSolutionsSeaBreezeController> _display;

        public IARSolutionDisplay<ARSolutionsSeaBreezeController> Display
        {
            get => _display;
            set
            {
                _display = value;

                Log.Info($"Display Storage Count : {value.Controller.StorageItems.Count}");
            }
        }

        protected bool IsHovered { get; set; }
        protected string TextLineOne { get; set; }
        protected string TextLineTwo { get; set; }
        private bool isHoveredOutOfRange;

        public virtual void OnDisable()
        {
            IsHovered = false;
            isHoveredOutOfRange = false;
        }

        public virtual void Update()
        {
            bool inInteractionRange = InInteractionRange();
            //Log.Info($"In Interaction Range: {inInteractionRange} || Is Hovered: {IsHovered}");
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
                Log.Info($"IsHovered: {IsHovered}");
            }
            isHoveredOutOfRange = true;
            Display.ResetIdleTimer();
        }

        public virtual void OnPointerExit(PointerEventData eventData)
        {
            IsHovered = false;
            isHoveredOutOfRange = false;
            Display.ResetIdleTimer();
        }

        public virtual void OnPointerClick(PointerEventData eventData)
        {
            Log.Info("OnPointerClick");
            Display.ResetIdleTimer();
        }

        protected bool InInteractionRange()
        {
            return Mathf.Abs(Vector3.Distance(gameObject.transform.position, Player.main.transform.position)) <= Display.MAX_INTERACTION_DISTANCE;
        }
    }
}
