using FCSCommon.Utilities;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.EventSystems;
using FCS_StorageSolutions.ModItems.Buildables.DataStorageSolutions.Mono.Base;
using FCS_StorageSolutions.ModItems.Buildables.DataStorageSolutions.Mono.C40Terminal.Mono;

namespace FCS_StorageSolutions.ModItems.Buildables.DataStorageSolutions.Managers;
[DisallowMultipleComponent]
public class DSSPageController : MonoBehaviour
{
    [SerializeField]
    private DSSPage InitialPage;

    [SerializeField]
    private GameObject FirstFocusItem;

    private Canvas RootCanvas;

    private Stack<DSSPage> PageStack = new Stack<DSSPage>();

    internal event EventHandler<OnMenuControllerEventArg> OnMenuContollerPop;
    internal event EventHandler<OnMenuControllerEventArg> OnMenuContollerPush;
    internal class OnMenuControllerEventArg : EventArgs
    {
        public DSSPage Page { get; set; }
    }


    private void Awake()
    {
        RootCanvas = GetComponentInParent<Canvas>();

        if (FirstFocusItem != null)
        {
            EventSystem.current.SetSelectedGameObject(FirstFocusItem);
        }

        if (InitialPage != null)
        {
            PushPage(InitialPage,GetComponent<DSSTerminalController>());
        }


    }

    private void OnCancel()
    {
        if (RootCanvas.enabled && RootCanvas.gameObject.activeInHierarchy)
        {
            if (PageStack.Count != 0)
            {
                PopAndPeek();
            }
        }
    }

    internal bool IsPageInStack(DSSPage Page)
    {
        return PageStack.Contains(Page);
    }

    internal bool IsPageOnTopOfStack(DSSPage Page)
    {
        return PageStack.Count > 0 && Page == PageStack.Peek();
    }

    public void PushPage(DSSPage Page, object arg = null)
    {
        Page.Enter(arg);

        if (PageStack.Count > 0)
        {
            DSSPage currentPage = PageStack.Peek();

            if (currentPage.ExitOnNewPagePush && !Page.IsOverlay)
            {
                currentPage.Exit();
            }

            OnMenuContollerPush?.Invoke(this, new OnMenuControllerEventArg { Page = Page });
        }

        PageStack.Push(Page);
    }

    public void PushPage(DSSPage page)
    {
        if(PageStack.Count > 0 && page == PageStack.Peek())
        {
            QuickLogger.Warning("Page already open. Skipping.");
            return;
        }
        PushPage(page, null);
    }

    internal DSSPage PopAndPeek()
    {
        if (PageStack.Count > 1)
        {
            var isOverlay = PageStack.Peek().IsOverlay;

            if (!isOverlay)
            {

            }

            DSSPage page = PageStack.Pop();
            page.Exit();

            DSSPage newCurrentPage = PageStack.Peek();

            if (newCurrentPage.ExitOnNewPagePush)
            {
                QuickLogger.Debug("Pop Page : Exit On New Page Enter");
                newCurrentPage.Enter();
            }

            OnMenuContollerPop?.Invoke(this, new OnMenuControllerEventArg { Page = newCurrentPage });
        }
        else
        {
            Debug.LogWarning("Trying to pop a page but only 1 page remains in the stack!");
        }

        return PageStack.Peek();
    }

    public void PopPage()
    {
        PopAndPeek();
    }

    public DSSPage PopAllPages()
    {
        QuickLogger.Debug($"PopAllPages: {PageStack.Count}", true);
        DSSPage page = null;
        for (int i = PageStack.Count - 1; i >= 0; i--)
        {
            QuickLogger.Debug($"PopAllPages  Index: {i}", true);
           page = PopAndPeek();
        }

        return page;
    }

    public void GoHome()
    {
        PopAllPages().Enter();
       // PushPage(InitialPage);
    }
}
