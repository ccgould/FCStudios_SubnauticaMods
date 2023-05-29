using FCS_AlterraHub.ModItems.FCSPDA.Enums;
using FCSCommon.Utilities;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace FCS_AlterraHub.ModItems.FCSPDA.Mono.Model;

public abstract class Page : MonoBehaviour
{
    public readonly bool ExitOnNewPagePush = true;
    public abstract PDAPages PageType { get; }
    protected virtual bool showHud { get; } = true;

    protected AudioClip EntryClip;
    protected AudioClip ExitClip;


    public virtual void Enter(object arg = null)
    {
        gameObject.SetActive(true);
    }

    public virtual void Exit()
    {
        gameObject.SetActive(false);
    }

    internal bool ShowHud()
    {
        return showHud;
    }

    public abstract void OnBackButtonClicked();

    public virtual void OnSettingsButtonClicked() { }

    public virtual void OnInfoButtonClicked() { }

    public virtual bool OnButtonDown(GameInput.Button button)
    {
        QuickLogger.Debug("On Button Down");
        if (button == GameInput.Button.UINextTab)
        {
            //this.pda.OpenTab(this.pda.GetNextTab());
            //return true;
        }
        if (button == GameInput.Button.UIPrevTab)
        {
            OnBackButtonClicked();
            return true;
        }
        if (button == uGUI.button1)
        {
            this.ClosePDA();
            return true;
        }
        return false;
    }

    protected void ClosePDA()
    {
        FCSPDAController.Main.Close();
    }

    // ======================================================== //

    public bool ShowSelector => true;

    public bool EmulateRaycast => true;
    private uGUI_ListEntry selectedEntry
    {
        get
        {
            if (UISelection.HasSelection)
            {
                return UISelection.selected as uGUI_ListEntry;
            }
            return null;
        }
    }

    public object GetSelectedItem()
    {
        return this.selectedEntry;
    }

    public Graphic GetSelectedIcon()
    {
        uGUI_ListEntry selectedEntry = this.selectedEntry;
        if (!(selectedEntry != null))
        {
            return null;
        }
        return selectedEntry.background;
    }

    public void SelectItem(object item)
    {
        uGUI_ListEntry uGUI_ListEntry = item as uGUI_ListEntry;
        if (uGUI_ListEntry == null)
        {
            return;
        }
        this.DeselectItem();
        UISelection.selected = uGUI_ListEntry;
        uGUI_ListEntry.OnPointerEnter(null);
        //this.listScrollRect.ScrollTo(uGUI_ListEntry.rectTransform, true, false, new Vector4(10f, 10f, 10f, 10f));
    }

    public void DeselectItem()
    {
        if (this.selectedEntry == null)
        {
            return;
        }
        this.selectedEntry.OnPointerExit(null);
        UISelection.selected = null;
    }
    private uGUI_ListEntry activeEntry;
    public bool SelectFirstItem()
    {
        if (this.activeEntry != null)
        {
            //this.ExpandTo(this.activeEntry.Key);
            this.SelectItem(this.activeEntry);
            return true;
        }
        //using (IEnumerator<CraftNode> enumerator = this.tree.GetEnumerator())
        //{
        //    if (enumerator.MoveNext())
        //    {
        //        this.SelectItem(uGUI_EncyclopediaTab.GetNodeListEntry(enumerator.Current));
        //        return true;
        //    }
        //}
        return false;
    }

    public bool SelectItemClosestToPosition(Vector3 worldPos)
    {
        return false;
    }

    public bool SelectItemInDirection(int dirX, int dirY)
    {
        if (this.selectedEntry == null)
        {
            return this.SelectFirstItem();
        }
        if (dirY == 0)
        {
            return false;
        }
        int siblingIndex = this.selectedEntry.transform.GetSiblingIndex();
        Transform parent = this.selectedEntry.transform.parent;
        int num = (dirY > 0) ? (siblingIndex + 1) : (siblingIndex - 1);
        int num2 = (dirY > 0) ? 1 : -1;
        int num3 = num;
        while (num3 >= 0 && num3 < parent.childCount)
        {
            uGUI_ListEntry component = parent.GetChild(num3).GetComponent<uGUI_ListEntry>();
            if (component.gameObject.activeInHierarchy)
            {
                this.SelectItem(component);
                return true;
            }
            num3 += num2;
        }

        return false;
    }

    public uGUI_INavigableIconGrid GetNavigableGridInDirection(int dirX, int dirY)
    {
        return null;
    }
}
