using FCS_StorageSolutions.ModItems.Buildables.DataStorageSolutions.Mono.C40Terminal.Enumerator;
using FCS_StorageSolutions.ModItems.Buildables.DataStorageSolutions.Mono.C40Terminal.Mono;
using FCSCommon.Utilities;
using UnityEngine;
using UnityEngine.UI;

namespace FCS_StorageSolutions.ModItems.Buildables.DataStorageSolutions.Mono.C40Terminal;
internal class uGUI_TerminalFilterList : MonoBehaviour
{
    [SerializeField] private Text targetFilterLbl;
    [SerializeField] private DSSTerminalController controller;

    public void ToggleVisibility()
    {
        gameObject.SetActive(!isActiveAndEnabled);
    }

    public void OnFiltrerButtonClicked(int id)
    {
        QuickLogger.Debug($"Filter Button Id: {id}",true);
        switch(id) 
        {
            case 0:
                targetFilterLbl.text = "Show All";
                break; 
            case 1:
                targetFilterLbl.text = "Servers";
                break; 
            case 2:
                targetFilterLbl.text = "Remote Storage";
                break; 
            case 3:
                targetFilterLbl.text = "Storage Locker";
                break; 
            case 4:
                targetFilterLbl.text = "SeaBreeze";
                break;
            case 5:
                targetFilterLbl.text = "Harvester";
                break;
            case 6:
                targetFilterLbl.text = "Replicator";
                break;
        }

        controller.SetFilter((DSSTerminalFilterOptions)id);
    }
}
