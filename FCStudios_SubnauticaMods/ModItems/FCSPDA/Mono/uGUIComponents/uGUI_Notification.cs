using System;
using TMPro;
using UnityEngine;

namespace FCS_AlterraHub.ModItems.FCSPDA.Mono.uGUIComponents;
internal class uGUI_Notification : MonoBehaviour
{
    private const string POPUP = "Popup";
    [SerializeField] private TextMeshProUGUI messageText;
    [SerializeField] private uGUI_NotificationManager notificationManager;
    private Animator animator;

    public event EventHandler<OnDeletedArg> OnDeleted;
    public class OnDeletedArg : EventArgs
    {
        public uGUI_Notification notification;
    }

    private void Awake()
    {
        animator = GetComponentInChildren<Animator>();
    }

    public string GetMessage()
    {
        return messageText.text;
    }

    public void SetMessage(string message)
    {
        gameObject.SetActive(true);
        animator.SetTrigger(POPUP);
        messageText.text = message;
    }

    public void OnCloseBTNClicked()
    {
        OnDeleted?.Invoke(this, new OnDeletedArg
        {
            notification = this
        });
    }
}
