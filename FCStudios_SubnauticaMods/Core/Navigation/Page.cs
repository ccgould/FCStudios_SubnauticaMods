using FCS_AlterraHub.Models;
using FCS_AlterraHub.Models.Enumerators;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using static RootMotion.FinalIK.IKSolverLimb;
using static UnityEngine.UI.ContentSizeFitter;

namespace FCS_AlterraHub.Core.Navigation;

[RequireComponent(typeof(AudioSource),typeof(CanvasGroup))]
[DisallowMultipleComponent]
public class Page : MonoBehaviour
{
    private AudioSource audioSource;
    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;

    [SerializeField]
    private float animationSpeed  = 1f;
    [SerializeField]
    private AudioClip entryClip;
    [SerializeField]
    private AudioClip exitClip;
    [SerializeField]
    private PageEntryMode entryMode = PageEntryMode.SLIDE;
    [SerializeField]
    private Direction entryDirection = Direction.LEFT;
    [SerializeField]
    private PageEntryMode exitMode = PageEntryMode.SLIDE;
    [SerializeField]
    private Direction exitDirection = Direction.LEFT;

    [SerializeField]
    private UnityEvent prePushAction;
    [SerializeField]
    private UnityEvent postPushAction;
    [SerializeField]
    private UnityEvent prePopAction;
    [SerializeField]
    private UnityEvent postPopAction;


    private Coroutine animationCoroutine;
    private Coroutine audioCoroutine;

    public bool ExitOnNewPagePush = false;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();

        if(audioSource is not null )
        {
            audioSource.playOnAwake = false;
            audioSource.loop = false;
            audioSource.spatialBlend = 0;
        }
    }

    public void Enter(bool PlayAudio)
    {
        prePushAction?.Invoke();

        switch (entryMode)
        {
            case PageEntryMode.SLIDE:
                SlideIn(PlayAudio);
                break;
            case PageEntryMode.ZOOM:
                ZoomIn(PlayAudio);
                break;
            case PageEntryMode.FADE:
                FadeIn(PlayAudio);
                break;
        }
    }

    public void Exit(bool PlayAudio)
    {
        prePopAction?.Invoke();

        switch (exitMode)
        {
            case PageEntryMode.SLIDE:
                SlideOut(PlayAudio);
                break;
            case PageEntryMode.ZOOM:
                ZoomOut(PlayAudio);
                break;
            case PageEntryMode.FADE:
                FadeOut(PlayAudio);
                break;
        }
    }

    private void SlideIn(bool PlayAudio)
    {
        if (animationCoroutine != null)
        {
            StopCoroutine(animationCoroutine);
        }
        animationCoroutine = StartCoroutine(AnimationHelper.SlideIn(rectTransform, entryDirection, animationSpeed, postPushAction));

        PlayEntryClip(PlayAudio);
    }

    private void SlideOut(bool PlayAudio)
    {
        if (animationCoroutine != null)
        {
            StopCoroutine(animationCoroutine);
        }
        animationCoroutine = StartCoroutine(AnimationHelper.SlideOut(rectTransform, exitDirection, animationSpeed, postPopAction));

        PlayExitClip(PlayAudio);
    }

    private void ZoomIn(bool PlayAudio)
    {
        if (animationCoroutine != null)
        {
            StopCoroutine(animationCoroutine);
        }
        animationCoroutine = StartCoroutine(AnimationHelper.ZoomIn(rectTransform, animationSpeed, postPushAction));

        PlayEntryClip(PlayAudio);
    }

    private void ZoomOut(bool PlayAudio)
    {
        if (animationCoroutine != null)
        {
            StopCoroutine(animationCoroutine);
        }
        animationCoroutine = StartCoroutine(AnimationHelper.ZoomOut(rectTransform, animationSpeed, postPopAction));

        PlayExitClip(PlayAudio);
    }

    private void FadeIn(bool PlayAudio)
    {
        if (animationCoroutine != null)
        {
            StopCoroutine(animationCoroutine);
        }
        animationCoroutine = StartCoroutine(AnimationHelper.FadeIn(canvasGroup, animationSpeed, postPushAction));

        PlayEntryClip(PlayAudio);
    }

    private void FadeOut(bool PlayAudio)
    {
        if (animationCoroutine != null)
        {
            StopCoroutine(animationCoroutine);
        }
        animationCoroutine = StartCoroutine(AnimationHelper.FadeOut(canvasGroup, animationSpeed, postPopAction));

        PlayExitClip(PlayAudio);
    }

    private void PlayEntryClip(bool PlayAudio)
    {
        if (PlayAudio && entryClip != null && audioSource != null)
        {
            if (audioCoroutine != null)
            {
                StopCoroutine(audioCoroutine);
            }

            audioCoroutine = StartCoroutine(PlayClip(entryClip));
        }
    }

    private void PlayExitClip(bool PlayAudio)
    {
        if (PlayAudio && exitClip != null && audioSource != null)
        {
            if (audioCoroutine != null)
            {
                StopCoroutine(audioCoroutine);
            }

            audioCoroutine = StartCoroutine(PlayClip(exitClip));
        }
    }

    private IEnumerator PlayClip(AudioClip Clip)
    {
        audioSource.enabled = true;

        WaitForSeconds Wait = new WaitForSeconds(Clip.length);

        audioSource.PlayOneShot(Clip);

        yield return Wait;

        audioSource.enabled = false;
    }

    internal bool IsVisible()
    {
        return canvasGroup.alpha > 0;
    }
}
