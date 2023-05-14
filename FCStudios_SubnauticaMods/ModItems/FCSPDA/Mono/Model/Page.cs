using UnityEngine;

namespace FCS_AlterraHub.ModItems.FCSPDA.Mono.Model;

public abstract class Page : MonoBehaviour
{
    public readonly bool ExitOnNewPagePush = true;
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
}
