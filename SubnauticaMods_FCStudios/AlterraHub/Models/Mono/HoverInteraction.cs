using FCS_AlterraHub.Core.Helpers;
using System;
using UnityEngine;

namespace FCS_AlterraHub.Models.Mono
{
    public class HoverInteraction : HandTarget, IHandTarget
    {

        public TechType TechType;
        public event Action onSettingsKeyPressed;
        public void OnHandHover(GUIHand hand)
        {
            var techType = TechType;


            var data1 = new[]
            {
                ""
                //Language.main.GetFormat(AuxPatchers.JetStreamOnHover(),
                //    _mono.UnitID,Mathf.RoundToInt(this._powerSource.GetPower()),
                //    Mathf.RoundToInt(this._powerSource.GetMaxPower()),
                //    (_energyPerSec * 60).ToString("N1")),
                //AuxPatchers.JetStreamOnHoverInteractionFormatted("E", _powerState.ToString())

            };
            //data1.HandHoverPDAHelperEx(techType);

            if (Input.GetKeyDown(Main.Configuration.PDAInfoKeyCode))
            {
                //TODO V2 FIx
                //FCSPDAController.Main.OpenEncyclopedia(_mono.GetTechType());
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
