using UnityEngine;

namespace FCS_AlterraHub.ModItems.Buildables.BaseManager.Mono.GUI.Pages;
internal class uGUI_ConnectionsPage : MonoBehaviour
{
    [SerializeField] private BaseManagerSideMenu baseManagerSideMenu;

    private void Start()
    {
        baseManagerSideMenu.OnConnectionsButtonClickedEvent += BaseManagerSideMenu_OnConnectionsButtonClickedEvent;
    }

    private void BaseManagerSideMenu_OnConnectionsButtonClickedEvent(object sender, System.EventArgs e)
    {
        Show();
    }

    public void Show()
    {
        gameObject.SetActive(true);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }
}
