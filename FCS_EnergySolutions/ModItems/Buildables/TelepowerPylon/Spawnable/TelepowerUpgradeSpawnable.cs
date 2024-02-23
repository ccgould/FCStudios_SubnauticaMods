using FCS_AlterraHub.API;
using FCS_AlterraHub.Models.Abstract;
using FCS_AlterraHub.Models.Enumerators;
using FCSCommon.Helpers;
using System.Collections;
using UnityEngine;

namespace FCS_EnergySolutions.ModItems.Buildables.TelepowerPylon.Spawnable;
internal class TelepowerUpgradeSpawnable : FCSSpawnableModBase
{
    private Color _color;

    public static TechType PatchedTechType { get; private set; }

    public TelepowerUpgradeSpawnable(string classID, string friendlyName, Color color) : base(PluginInfo.PLUGIN_NAME, "PylonUpgradeDataBox", FileSystemHelper.ModDirLocation, classID, friendlyName)
    {
        _color = color;
        OnStartRegister += () =>
        {
            PatchedTechType = TechType;
            FCSModsAPI.PublicAPI.CreateStoreEntry(TechType, TechType, 1, _settings.ItemCost, StoreCategory.Misc, true);
        };
    }

    protected override IEnumerator ModifyPrefab(GameObject prefab)
    {
        yield break;
    }
}
