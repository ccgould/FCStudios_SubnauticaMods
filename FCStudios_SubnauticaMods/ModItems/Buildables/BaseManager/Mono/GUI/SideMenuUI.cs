using UnityEngine;

namespace FCS_AlterraHub.ModItems.Buildables.BaseManager.Mono.GUI;
internal class SideMenuUI : MonoBehaviour
{
    [SerializeField]
    private Transform grid;
    [SerializeField]
    private uGUI_BaseManager uGUI_BaseManager;
    [SerializeField]
    private Transform menuButtonTemplate;

    private void Start()
    {
        uGUI_BaseManager.OnMenuButtonAction += UGUI_BaseManager_OnMenuButtonClicked;
        Hide();
    }

    private void UGUI_BaseManager_OnMenuButtonClicked(object sender, System.EventArgs e)
    {
        if(gameObject.activeSelf)
        {
            Hide();
        }
        else
        {
            Show();
        }

    }

    private void Show()
    {
        gameObject.SetActive(true);
    }

    private void Hide()
    {
        gameObject.SetActive(false);
    }

}
