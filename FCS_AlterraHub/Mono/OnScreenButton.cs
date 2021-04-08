using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace FCS_AlterraHub.Mono
{
    /// <summary>
    /// Component that buttons on the power storage ui will inherit from. Handles working on whether something is hovered via IsHovered as well as interaction text. 
    /// </summary>
    /// <seealso cref="UnityEngine.MonoBehaviour" />
    public abstract class OnScreenButton : MonoBehaviour
    {
        protected bool IsHovered { get; set; }
        public string TextLineOne { get; set; }
        public string TextLineTwo { get; set; }
        public bool UseSetUseTextRaw { get; set; } = false;
        public bool GetAdditionalDataFromString { get; set; } = false;
        public Func<object, string> GetAdditionalString { get; set; }
        private bool isHoveredOutOfRange;
        public bool Disabled { get; set; }
        public InteractionRequirement InteractionRequirement { get; set; } = InteractionRequirement.None;
        public virtual object Tag { get; set; }
        public float MaxInteractionRange { get; set; }  = 2.5f;

        public virtual void OnDisable()
        {
            this.IsHovered = false;
            isHoveredOutOfRange = false;
            Disabled = true;
        }

        public virtual void Update()
        {
            bool inInteractionRange = InInteractionRange();

            HandReticle main = HandReticle.main;

            if (this.IsHovered && inInteractionRange == false)
            {
                this.IsHovered = false;
            }

            if (this.IsHovered == false && isHoveredOutOfRange && inInteractionRange)
            {
                this.IsHovered = true;
            }

#if SUBNAUTICA
            if (this.IsHovered && inInteractionRange)
            {
                if(string.IsNullOrEmpty(TextLineOne) && string.IsNullOrEmpty(TextLineTwo) && !GetAdditionalDataFromString) return;

                if (ShowMouseClick)
                {
                    if (GetAdditionalDataFromString)
                    {
                        main.SetIcon(IconType);
                        if (!string.IsNullOrWhiteSpace(TextLineOne))
                        {
                            main.SetInteractText(TextLineOne ?? string.Empty, GetAdditionalString?.Invoke(Tag) ?? string.Empty);
                        }
                        else
                        {
                            main.SetInteractText(GetAdditionalString?.Invoke(Tag) ?? string.Empty);
                        }
                    }
                    else
                    {
                        main.SetIcon(IconType);
                        main.SetInteractText(this.TextLineOne ?? String.Empty,this.TextLineTwo ?? string.Empty);
                    }
                }
                else
                {
                    if (UseSetUseTextRaw)
                    {
                        if (GetAdditionalDataFromString)
                        {
                            main.SetIcon(IconType);
                            main.SetUseTextRaw(this.TextLineOne ?? string.Empty, GetAdditionalString?.Invoke(Tag) ?? string.Empty);
                        }
                        else
                        {
                            main.SetUseTextRaw(this.TextLineOne ?? string.Empty, this.TextLineTwo ?? string.Empty);
                        }
                    }
                    else
                    {
                        if (GetAdditionalDataFromString)
                        {
                            main.SetIcon(IconType);
                            main.SetInteractTextRaw(this.TextLineOne ?? string.Empty, GetAdditionalString?.Invoke(Tag) ?? string.Empty);
                        }
                        else
                        {
                            main.SetInteractTextRaw(this.TextLineOne ?? string.Empty, this.TextLineTwo ?? string.Empty);
                        }
                    }
                }

            }
#elif BELOWZERO
            if (this.IsHovered && inInteractionRange)
            {
                if (string.IsNullOrEmpty(TextLineOne) && string.IsNullOrEmpty(TextLineTwo)) return;
                HandReticle.main.SetTextRaw(HandReticle.TextType.Hand, this.TextLineOne);
                HandReticle.main.SetTextRaw(HandReticle.TextType.HandSubscript, this.TextLineTwo);
            }

#endif
        }

        public bool ShowMouseClick { get; set; } = false;

        public HandReticle.IconType IconType { get; set; } = HandReticle.IconType.Default;

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
            return Mathf.Abs(Vector3.Distance(this.gameObject.transform.position, Player.main.transform.position)) <= MaxInteractionRange && InteractionRequirementCheck();
        }

        private bool InteractionRequirementCheck()
        {
            switch (InteractionRequirement)
            {
                case InteractionRequirement.IsInside:
                    return Player.main.IsInBase() || Player.main.IsInSub();
                case InteractionRequirement.IsOutSide:
                    return !Player.main.IsInBase() || !Player.main.IsInSub() || !Player.main.IsInClawExosuit();
                case InteractionRequirement.None:
                    return true;
                default:
                    return true;
            }
        }
    }

    public enum InteractionRequirement
    {
        None,
        IsOutSide,
        IsInside,
    }
}
