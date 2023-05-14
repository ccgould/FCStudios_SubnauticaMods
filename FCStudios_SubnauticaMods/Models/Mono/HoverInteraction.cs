using FCS_AlterraHub.Core.Helpers;
using FCS_AlterraHub.Core.Services;
using FCS_AlterraHub.Models.Abstract;
using System;
using UnityEngine;

namespace FCS_AlterraHub.Models.Mono;

public class HoverInteraction : HandTarget, IHandTarget
{

    public TechType TechType;
    [SerializeField]
    private FCSDevice _controller;

    public event Action onSettingsKeyPressed;
    public void OnHandHover(GUIHand hand)
    {
        var techType = TechType;

        var data1 = new[]
        {
            Language.main.Get(techType),
            LanguageService.PressToInteractWith(Plugin.Configuration.PDASettingsKeyCode,_controller?.UnitID),
            LanguageService.PowerPerMinute(0f)
        };
        data1.HandHoverPDAHelperEx(techType);

        if (Input.GetKeyDown(Plugin.Configuration.PDAInfoKeyCode))
        {
            //TODO V2 FIx

            EncyclopediaService.OpenEncyclopedia(_controller.GetTechType());
        }

        if (Input.GetKeyDown(Plugin.Configuration.PDASettingsKeyCode))
        {
            onSettingsKeyPressed?.Invoke();
        }
    }

    public void OnHandClick(GUIHand hand)
    {

    }
}
