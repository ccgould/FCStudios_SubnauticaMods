using FCS_AlterraHub.Core.Services;
using FCS_AlterraHub.Models.Abstract;
using FCS_AlterraHub.ModItems.Spawnables.DebitCard.Mono;
using FCSCommon.Helpers;
using System.Collections;
using UnityEngine;

namespace FCS_AlterraHub.ModItems.Spawnables.DebitCard.Spawnable;

internal class DebitCardSpawnable : FCSSpawnableModBase
{
    public override string AssetsFolder => ModRegistrationService.GetModPackData(Main.MODNAME)?.GetAssetPath();

    public static TechType PatchedTechType { get; private set; }

    public DebitCardSpawnable() : base(Main.MODNAME, "CreditCard", FileSystemHelper.ModDirLocation, "DebitCard", "Debit Card")
    {
        OnFinishedPatching += () =>
        {
            PatchedTechType = TechType;
        };
    }

    public override IEnumerator GetGameObjectAsync(IOut<GameObject> gameObject)
    {
        yield return base.GetGameObjectAsync(gameObject);

        Prefab.AddComponent<FcsCard>();

        yield break;
    }
}
