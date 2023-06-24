using FCS_AlterraHub.Models.Abstract;
using FCS_AlterraHub.Models.Interfaces;
using FCS_StorageSolutions.Configuation;
using UnityEngine;
using UnityEngine.UI;

namespace FCS_StorageSolutions.ModItems.Buildables.RemoteStorage.Mono;
internal class RemoteStorageController : FCSDevice, IFCSSave<SaveData>
{
    [SerializeField] private Transform grid;
    [SerializeField] private Transform uGUI_RemoteStorageItemTemplate;
    [SerializeField] private Text storageAmountLbl;
    

    public override void ReadySaveData()
    {
        
    }

    public void Save(SaveData newSaveData, ProtobufSerializer serializer = null)
    {
        
    }
}
