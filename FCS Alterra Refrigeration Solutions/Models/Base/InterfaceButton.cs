using FCS_Alterra_Refrigeration_Solutions.Logging;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace FCS_Alterra_Refrigeration_Solutions.Models.Base
{
    /// <summary>
    /// This class is a component for all interface buttons except the color picker and the paginator.
    /// For the color picker see the <see cref="ColorItemButton"/>
    /// For the paginator see the <see cref="PaginatorButton"/> 
    /// </summary>
    public class InterfaceButton : OnScreenButton, IPointerEnterHandler, IPointerClickHandler, IPointerExitHandler
    {
        #region Private Member
        private static readonly Color HOVER_COLOR = new Color(0.07f, 0.38f, 0.7f, 1f);
        private static Color STARTING_COLOR = Color.white;
        #endregion

        #region Public Properties
        /// <summary>
        /// The name of the radial button
        /// </summary>
        public string BtnName { get; set; }
        public string HoverTextLineOne { get; set; }
        public string HoverTextLineTwo { get; set; }
        #endregion

        #region Public Methods
        public void Start()
        {

            STARTING_COLOR = GetComponent<Image>().color;
            TextLineOne = HoverTextLineOne;
            TextLineTwo = HoverTextLineTwo;
        }

        public void OnEnable()
        {
            if (GetComponent<Image>() != null)
            {
                GetComponent<Image>().color = STARTING_COLOR;
            }
        }

        #endregion

        #region Public Overrides
        public override void OnDisable()
        {
            base.OnDisable();

            GetComponent<Image>().color = STARTING_COLOR;
        }

        public override void OnPointerEnter(PointerEventData eventData)
        {
            base.OnPointerEnter(eventData);

            Log.Info("Entered Button");

            if (IsHovered)
            {
                GetComponent<Image>().color = HOVER_COLOR;
                Display.Controller.Container.enabled = false;
            }
        }

        public override void OnPointerExit(PointerEventData eventData)
        {
            base.OnPointerExit(eventData);
            GetComponent<Image>().color = STARTING_COLOR;
            if (!Display.Controller.HasBreakerTripped)
            {
                Display.Controller.Container.enabled = true;
            }
        }

        public override void OnPointerClick(PointerEventData eventData)
        {
            base.OnPointerClick(eventData);

            Log.Info($"Clicked {BtnName}");

            if (IsHovered)
            {
                Log.Info("Going to On Button Clicked");
                Display.OnButtonClick(BtnName);
            }
        }
        #endregion

    }
}
