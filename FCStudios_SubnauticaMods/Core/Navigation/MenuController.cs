﻿using FCS_AlterraHub.ModItems.FCSPDA.Mono.uGUIComponents;
using FCSCommon.Utilities;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace FCS_AlterraHub.Core.Navigation;

[DisallowMultipleComponent]
public class MenuController : MonoBehaviour
{
    [SerializeField]
    private Page InitialPage;

    [SerializeField]
    private GameObject FirstFocusItem;

    private Canvas RootCanvas;

    private Stack<Page> PageStack = new Stack<Page>();

    public event EventHandler<OnMenuControllerEventArg> OnMenuContollerPop;
    public event EventHandler<OnMenuControllerEventArg> OnMenuContollerPush;
    public class OnMenuControllerEventArg : EventArgs
    {
        public Page Page { get; set; }
    }


    private void Awake()
    {
        RootCanvas = GetComponentInParent<Canvas>();
    }

    private void Start()
    {
        if (FirstFocusItem != null)
        {
            EventSystem.current.SetSelectedGameObject(FirstFocusItem);
        }

        if (InitialPage != null)
        {
            PushPage(InitialPage);
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

    public bool IsPageInStack(Page Page)
    {
        return PageStack.Contains(Page);
    }

    public bool IsPageOnTopOfStack(Page Page)
    {
        return PageStack.Count > 0 && Page == PageStack.Peek();
    }

    public void PushPage(Page Page,object arg = null)
    {
        Page.Enter(arg);

        if (PageStack.Count > 0)
        {
            Page currentPage = PageStack.Peek();

            if (currentPage.ExitOnNewPagePush)
            {
                currentPage.Exit();
            }

            OnMenuContollerPush?.Invoke(this, new OnMenuControllerEventArg { Page = Page });
        }

        PageStack.Push(Page);
    }

    public void PushPage(Page page)
    {
        PushPage(page,null);
    }

    public Page PopAndPeek()
    {
        if (PageStack.Count > 1)
        {
            Page page = PageStack.Pop();
            page.Exit();

            Page newCurrentPage = PageStack.Peek();
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

    public void PopAllPages()
    {
        QuickLogger.Debug($"PopAllPages: {PageStack.Count}",true);
        for (int i = PageStack.Count - 1; i >= 0; i--)
        {
            QuickLogger.Debug($"PopAllPages  Index: {i}", true);
            PopAndPeek();
        }
    }
}