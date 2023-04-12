using FCS_AlterraHub.Core.Helpers;
using FCS_AlterraHub.Core.Services;
using FCS_AlterraHub.Models.Abstract;
using FCS_AlterraHub.ModItems.FCSPDA.Mono;
using FCSCommon.Utilities;
using SMLHelper.Handlers;
using System;
using UnityEngine;

namespace FCS_AlterraHub.Models.Mono
{
    public class HoverInteraction : HandTarget, IHandTarget
    {

        public TechType TechType;
        private FCSDevice _controller;

        public event Action onSettingsKeyPressed;
        public void OnHandHover(GUIHand hand)
        {
            if(_controller is null)
            {
                _controller = gameObject.GetComponentInChildren<FCSDevice>();
            }

            var techType = TechType;


            var data1 = new[]
            {
                Language.main.Get(techType),
                LanguageService.PressToInteractWith(Main.Configuration.PDASettingsKeyCode,_controller?.UnitID),
                LanguageService.PowerPerMinute(0f)
            };
            data1.HandHoverPDAHelperEx(techType);

            if (Input.GetKeyDown(Main.Configuration.PDAInfoKeyCode))
            {
                //TODO V2 FIx

                //FCSPDAController.Main.OpenEncyclopedia(_controller.GetTechType());
            }

            if (Input.GetKeyDown(Main.Configuration.PDASettingsKeyCode))
            {
                onSettingsKeyPressed?.Invoke();
            }
        }

        public void OnHandClick(GUIHand hand)
        {

        }
    }
}
