using FCS_AlterraHub.Core.Components;
using FCS_AlterraHub.Core.Services;
using FCS_AlterraHub.Models.Abstract;
using FCSCommon.Utilities;
using System;
using System.ComponentModel;
using System.Text;
using UnityEngine;

namespace FCS_AlterraHub.Models.Mono;

public class HoverInteraction : HandTarget, IHandTarget
{
    [SerializeField]
    private FCSDevice _controller;
    [SerializeField]
    private Constructable _constructable;
    [SerializeField]
    private HandReticle.IconType icon = HandReticle.IconType.Hand;
    [SerializeField]
    private HandReticle.IconType errorIcon = HandReticle.IconType.HandDeny;
    [SerializeField]
    private GameInput.Button button = GameInput.Button.LeftHand;
    [SerializeField]
    private string mainText = "Open Settings";
    [SerializeField]
    private string errorText;
    [SerializeField]
    [Description("This component is for use on canvas where the HoverInteraction needs to be ignored to allow UI clicking")]
    private InterfaceInteraction interfaceInteraction;


    public event Action<TechType> onSettingsKeyPressed;
    public event Action<FCSPDAController> onPDAClosed;
    public event Func<bool> IsAllowedToInteract;

    private static readonly StringBuilder Sb = new();

    public void OnHandHover(GUIHand hand)
    {
        if (!base.enabled || (IsAllowedToInteract is not null && !IsAllowedToInteract.Invoke()))
        {
            var main = HandReticle.main;

            main.SetText(HandReticle.TextType.Hand, errorText, false);
            main.SetText(HandReticle.TextType.HandSubscript, Sb.ToString(), false, GameInput.Button.None);
            main.SetIcon(errorIcon, 1f);

            return;
        }
        
        if (!_constructable || _constructable.constructed)
        {
            if (_controller is null)
            {
                QuickLogger.Error("Control on Hover is null");
                return;
            }

            HandHoverPDAHelper(_controller);

            if (Input.GetKeyDown(Plugin.Configuration.PDAInfoKeyCode))
            {
                //TODO V2 FIx

                EncyclopediaService.OpenEncyclopedia(_controller);
            }

            //if (Input.GetKeyDown(Plugin.Configuration.PDASettingsKeyCode))
            //{
                
            //}
        }
    }

    public void OnHandClick(GUIHand hand)
    {
        if (!base.enabled || (IsAllowedToInteract is not null && !IsAllowedToInteract.Invoke()))
        {
            return;
        }

        var main = HandReticle.main;

        if (interfaceInteraction is not null && interfaceInteraction.IsInRange)
        {
            return;
        }

        if (!_constructable || _constructable.constructed)
        {
            onSettingsKeyPressed?.Invoke(_controller.GetTechType());
        }
    }

    private void HandHoverPDAHelper(FCSDevice controller, float progess = 0f)
    {
        var main = HandReticle.main;
        
        if(interfaceInteraction is not null && interfaceInteraction.IsInRange)
        {
            main.SetIcon(HandReticle.IconType.Default);
            return;
        }


        var strings = controller.GetDeviceStats();

        if (strings == null || main == null) return;
       
        CreateText(controller, strings);

        main.SetText(HandReticle.TextType.Hand, mainText, false, button);
        main.SetText(HandReticle.TextType.HandSubscript, Sb.ToString(), false, GameInput.Button.None);
        main.SetIcon(icon, 1f);

        Sb.Clear();

        if (icon == HandReticle.IconType.Progress)
        {
            main.SetProgress(progess);
        }
    }

    private void CreateText(FCSDevice controller, string[] strings)
    {
        var pda = FCSPDAController.Main;
        var text = pda.GetGUI()?.CheckIfPDAHasEntry(controller.GetTechType()) ?? false ? LanguageService.ViewInPDA() : string.Empty;

        Sb.Append(LanguageService.DeviceNameStructure(controller));

        Sb.Append(Environment.NewLine);
        for (var i = 0; i < strings.Length; i++)
        {
            string s = strings[i];
            if (string.IsNullOrEmpty(s)) continue;
            Sb.Append(s);
            if (strings.Length == 1 || i == strings.Length - 1) continue;
            Sb.Append(Environment.NewLine);
        }
        Sb.Append(Environment.NewLine);
        if(!_controller.IsConnectedToBase && !_controller.GetBypassConnection())
        {
            Sb.Append(LanguageService.NotConnectedToBaseManager());
            Sb.Append(Environment.NewLine);
        }
        Sb.Append(text);
        //Sb.Append(LanguageService.PressToInteractWith(Plugin.Configuration.PDASettingsKeyCode));
    }

}
