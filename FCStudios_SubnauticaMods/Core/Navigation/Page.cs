using FCS_AlterraHub.Models;
using FCS_AlterraHub.Models.Enumerators;
using FCS_AlterraHub.ModItems.FCSPDA.Enums;
using System;
using UnityEngine;
using UnityEngine.Events;
using static FCS_AlterraHub.ModItems.FCSPDA.Mono.uGUIComponents.uGUI_PDANavigationController;

namespace FCS_AlterraHub.Core.Navigation;

[RequireComponent(typeof(CanvasGroup))]
[DisallowMultipleComponent]
public class Page : MonoBehaviour
{
    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;

    [SerializeField]
    private bool showHud = false;
    [SerializeField]
    private bool showBackButton = false;
    [SerializeField]
    private bool showStorageButton = false;
    [SerializeField]
    private bool showSettingsButton = false;
    [SerializeField]
    private bool showInfoButton = false;
    [SerializeField]
    private bool showLabel = false;

    [SerializeField]
    private PDAPages PageType;

    public NavigationLabelState NavigationLabelState;

    [SerializeField]
    private float animationSpeed  = 4f;
    [SerializeField]
    private PageEntryMode entryMode = PageEntryMode.FADE;
    [SerializeField]
    private Direction entryDirection = Direction.NONE;
    [SerializeField]
    private PageEntryMode exitMode = PageEntryMode.FADE;
    [SerializeField]
    private Direction exitDirection = Direction.NONE;

    [SerializeField]
    private UnityEvent prePushAction;
    [SerializeField]
    private UnityEvent postPushAction;
    [SerializeField]
    private UnityEvent prePopAction;
    [SerializeField]
    private UnityEvent postPopAction;


    private Coroutine animationCoroutine;

    public bool ExitOnNewPagePush = false;

    public virtual void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
    }

    public virtual void Enter(object arg = null)
    {
        if(!gameObject.activeSelf)
        {
            gameObject.SetActive(true);
        }

        prePushAction?.Invoke();

        switch (entryMode)
        {
            case PageEntryMode.SLIDE:
                SlideIn();
                break;
            case PageEntryMode.ZOOM:
                ZoomIn();
                break;
            case PageEntryMode.FADE:
                FadeIn();
                break;
        }
    }

    public virtual void Exit()
    {
        prePopAction?.Invoke();

        switch (exitMode)
        {
            case PageEntryMode.SLIDE:
                SlideOut();
                break;
            case PageEntryMode.ZOOM:
                ZoomOut();
                break;
            case PageEntryMode.FADE:
                FadeOut();
                break;
        }
    }

    private void SlideIn()
    {
        if (animationCoroutine != null)
        {
            StopCoroutine(animationCoroutine);
        }
        animationCoroutine = StartCoroutine(AnimationHelper.SlideIn(rectTransform, entryDirection, animationSpeed, postPushAction));
    }

    private void SlideOut()
    {
        if (animationCoroutine != null)
        {
            StopCoroutine(animationCoroutine);
        }
        animationCoroutine = StartCoroutine(AnimationHelper.SlideOut(rectTransform, exitDirection, animationSpeed, postPopAction));
    }

    private void ZoomIn()
    {
        if (animationCoroutine != null)
        {
            StopCoroutine(animationCoroutine);
        }
        animationCoroutine = StartCoroutine(AnimationHelper.ZoomIn(rectTransform, animationSpeed, postPushAction));
    }

    private void ZoomOut()
    {
        if (animationCoroutine != null)
        {
            StopCoroutine(animationCoroutine);
        }
        animationCoroutine = StartCoroutine(AnimationHelper.ZoomOut(rectTransform, animationSpeed, postPopAction));
    }

    private void FadeIn()
    {
        if (animationCoroutine != null)
        {
            StopCoroutine(animationCoroutine);
        }
        animationCoroutine = StartCoroutine(AnimationHelper.FadeIn(canvasGroup, animationSpeed, postPushAction));
    }

    private void FadeOut()
    {
        if (animationCoroutine != null)
        {
            StopCoroutine(animationCoroutine);
        }
        animationCoroutine = StartCoroutine(AnimationHelper.FadeOut(canvasGroup, animationSpeed, postPopAction));
    }

    internal bool IsVisible()
    {
        return canvasGroup.alpha > 0;
    }

    internal bool ShowHud()
    {
        return showHud;
    }
    
    internal bool ShowBackButton()
    {
        return showBackButton;
    }
    
    internal bool ShowStorageButton()
    {
        return showStorageButton;
    }
    
    internal bool ShowSettingsButton()
    {
        return showSettingsButton;
    }
    
    internal bool ShowInfoButton()
    {
        return showInfoButton;
    }
    internal bool ShowLabel()
    {
        return showLabel;
    }
    internal PDAPages PDAGetPageType() => PageType;

}



