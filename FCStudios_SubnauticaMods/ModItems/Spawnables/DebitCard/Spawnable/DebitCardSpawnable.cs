using FCS_AlterraHub.Core.Services;
using FCS_AlterraHub.Models.Abstract;
using FCS_AlterraHub.ModItems.Spawnables.DebitCard.Mono;
using FCSCommon.Helpers;
using System.Collections;
using UnityEngine;

namespace FCS_AlterraHub.ModItems.Spawnables.DebitCard.Spawnable;

internal class DebitCardSpawnable : FCSSpawnableModBase
{
    //public override string AssetsFolder => ModRegistrationService.GetModPackData(PluginInfo.PLUGIN_NAME)?.GetAssetPath();

    public static TechType PatchedTechType { get; private set; }

    public DebitCardSpawnable() : base(PluginInfo.PLUGIN_NAME, "CreditCard", FileSystemHelper.ModDirLocation, "DebitCard", "Debit Card")
    {
        OnFinishRegister += () =>
        {
            PatchedTechType = TechType;
        };
    }

    protected override IEnumerator ModifyPrefab(GameObject prefab)
    {
        prefab.AddComponent<FcsCard>();

        yield break;
    }
}
