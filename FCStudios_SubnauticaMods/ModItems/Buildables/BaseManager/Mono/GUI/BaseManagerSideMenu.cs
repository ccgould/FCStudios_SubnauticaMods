using System;
using UnityEngine;

namespace FCS_AlterraHub.ModItems.Buildables.BaseManager.Mono.GUI;
internal class BaseManagerSideMenu : MonoBehaviour
{
    public event EventHandler OnWorkUnitsButtonClickedEvent;
    public event EventHandler OnConnectionsButtonClickedEvent;
    private CanvasGroup canvasGroup;
    [SerializeField] private Transform grid;
    [SerializeField] private uGUI_BaseManager uGUI_BaseManager;
    [SerializeField] private Transform menuButtonTemplate;
    
    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
    }

    public void OnWorkUnitsButtonClicked()
    {
        OnWorkUnitsButtonClickedEvent?.Invoke(this,EventArgs.Empty);
        Hide();
    }

    public void OnConnectionsButtonClicked()
    {
        OnConnectionsButtonClickedEvent?.Invoke(this, EventArgs.Empty);
        Hide();
    }

    public void Show()
    {
        canvasGroup.alpha = 1;
        canvasGroup.blocksRaycasts = true;
        canvasGroup.interactable = true;
    }

    public void Hide()
    {
        canvasGroup.alpha = 0;
        canvasGroup.blocksRaycasts = false;
        canvasGroup.interactable = false;
    }

    internal bool IsOpen()
    {
        return canvasGroup.alpha > 0;
    }
}
