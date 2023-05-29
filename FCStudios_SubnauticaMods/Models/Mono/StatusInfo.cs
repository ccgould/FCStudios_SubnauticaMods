using System;
using TMPro;
using UnityEngine;

namespace FCS_AlterraHub.Models.Mono;

internal class StatusInfo : MonoBehaviour
{
    [SerializeField]
    private TMP_Text statusText;

    [SerializeField]
    private uGUI_Icon image;

    private Func<string> CallBack;

    private float timeLeft;

    internal void Initialize(Atlas.Sprite icon, Func<string> func)
    {
        image.sprite = icon;
        CallBack = func;
    }

    private void Update()
    {
        timeLeft -= Time.deltaTime;
        if (timeLeft < 0)
        {
            var status = CallBack?.Invoke();
            statusText.text = status;
            timeLeft = 1;
        }
    }
}
