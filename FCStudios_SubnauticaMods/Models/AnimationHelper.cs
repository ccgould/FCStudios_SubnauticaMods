using System.Collections;
using UnityEngine.Events;
using UnityEngine;
using FCS_AlterraHub.Models.Enumerators;

namespace FCS_AlterraHub.Models;
internal class AnimationHelper
{
    public static IEnumerator ZoomIn(RectTransform Transform, float Speed, UnityEvent OnEnd)
    {
        float time = 0;
        while (time < 1)
        {
            Transform.localScale = Vector3.Lerp(Vector3.zero, Vector3.one, time);
            yield return null;
            time += Time.deltaTime * Speed;
        }

        Transform.localScale = Vector3.one;

        OnEnd?.Invoke();
    }

    public static IEnumerator ZoomOut(RectTransform Transform, float Speed, UnityEvent OnEnd)
    {
        float time = 0;
        while (time < 1)
        {
            Transform.localScale = Vector3.Lerp(Vector3.one, Vector3.zero, time);
            yield return null;
            time += Time.deltaTime * Speed;
        }

        Transform.localScale = Vector3.zero;
        OnEnd?.Invoke();
    }

    public static IEnumerator FadeIn(CanvasGroup CanvasGroup, float Speed, UnityEvent OnEnd)
    {
        CanvasGroup.blocksRaycasts = true;
        CanvasGroup.interactable = true;

        float time = 0;
        while (time < 1)
        {
            CanvasGroup.alpha = Mathf.Lerp(0, 1, time);
            yield return null;
            time += Time.deltaTime * Speed;
        }

        CanvasGroup.alpha = 1;
        OnEnd?.Invoke();
    }

    public static IEnumerator FadeOut(CanvasGroup CanvasGroup, float Speed, UnityEvent OnEnd)
    {
        CanvasGroup.blocksRaycasts = false;
        CanvasGroup.interactable = false;

        float time = 0;
        while (time < 1)
        {
            CanvasGroup.alpha = Mathf.Lerp(1, 0, time);
            yield return null;
            time += Time.deltaTime * Speed;
        }

        CanvasGroup.alpha = 0;
        OnEnd?.Invoke();
    }

    public static IEnumerator SlideIn(RectTransform Transform, Direction Direction, float Speed, UnityEvent OnEnd)
    {
        Vector2 startPosition;
        switch (Direction)
        {
            case Direction.UP:
                startPosition = new Vector2(0, -Screen.height);
                break;
            case Direction.RIGHT:
                startPosition = new Vector2(-Screen.width, 0);
                break;
            case Direction.DOWN:
                startPosition = new Vector2(0, Screen.height);
                break;
            case Direction.LEFT:
                startPosition = new Vector2(Screen.width, 0);
                break;
            default:
                startPosition = new Vector2(0, -Screen.height);
                break;
        }

        float time = 0;
        while (time < 1)
        {
            Transform.anchoredPosition = Vector2.Lerp(startPosition, Vector2.zero, time);
            yield return null;
            time += Time.deltaTime * Speed;
        }

        Transform.anchoredPosition = Vector2.zero;
        OnEnd?.Invoke();
    }

    public static IEnumerator SlideOut(RectTransform Transform, Direction Direction, float Speed, UnityEvent OnEnd)
    {
        Vector2 endPosition;
        switch (Direction)
        {
            case Direction.UP:
                endPosition = new Vector2(0, Screen.height);
                break;
            case Direction.RIGHT:
                endPosition = new Vector2(Screen.width, 0);
                break;
            case Direction.DOWN:
                endPosition = new Vector2(0, -Screen.height);
                break;
            case Direction.LEFT:
                endPosition = new Vector2(-Screen.width, 0);
                break;
            default:
                endPosition = new Vector2(0, Screen.height);
                break;
        }

        float time = 0;
        while (time < 1)
        {
            Transform.anchoredPosition = Vector2.Lerp(Vector2.zero, endPosition, time);
            yield return null;
            time += Time.deltaTime * Speed;
        }

        Transform.anchoredPosition = endPosition;
        OnEnd?.Invoke();
    }
}
