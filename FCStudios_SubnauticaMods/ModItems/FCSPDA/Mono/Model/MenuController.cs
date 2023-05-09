using FCSCommon.Utilities;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace FCS_AlterraHub.ModItems.FCSPDA.Mono.Model;

internal class MenuController : MonoBehaviour
{
    public Page InitialPage;
    private GameObject FirstFocusItem;
    private Canvas RootCanvas;
    private Stack<Page> PageStack = new Stack<Page>();

    private void Awake()
    {
        RootCanvas = GetComponent<Canvas>();

    }

    public bool IsPageInStack(Page page)
    {
        return PageStack.Contains(page);
    }

    public bool IsPageOnTopOfStack(Page page)
    {
        return PageStack.Count > 0 && page == PageStack.Peek();
    }

    public void PushPage(Page page,object arg = null)
    {
        page.Enter(arg);

        if(PageStack.Count > 0)
        {
            Page currentPage = PageStack.Peek();

            if (currentPage.ExitOnNewPagePush)
            {
                currentPage.Exit();
            }
        }

        PageStack.Push(page);
    }

    public Page PopPage()
    {
        if (PageStack.Count > 1)
        {
            Page page = PageStack.Pop();
            page.Exit();
            Page newCurrentPage = PageStack.Peek();
            if(newCurrentPage.ExitOnNewPagePush) 
            {
                newCurrentPage.Enter(); 
            }
            return newCurrentPage;
        }
        else
        {
            QuickLogger.Warning("Warning trying to pop a page nut only 1 remains in the stack!");
        }
        return PageStack.Peek();
    }

    public void PopAllPages()
    {
        for (int i = 0; i < PageStack.Count; i++)
        {
            PopPage();
        }
    }

    public void OnCancel()
    {
        if(RootCanvas.enabled && RootCanvas.gameObject.activeInHierarchy)
        {
            if(PageStack.Count != 0)
            {
                PopPage();
            }
        }
    }

    internal void Initialize()
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
}
