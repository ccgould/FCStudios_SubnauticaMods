using FCS_AlterraHub.Models.Abstract;
using FCSCommon.Helpers;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace FCS_AlterraHub.Models;

public class FCSKit : FCSSpawnableModBase
{
    public FCSKit(string classID,string friendlyName, string modName) : base(modName, "MainConstructionKit", FileSystemHelper.ModDirLocation,classID, $"{friendlyName} Kit")
    {
      
    }


    public override IEnumerator GetGameObjectAsync(IOut<GameObject> gameObject)
    {
        var prefab = GameObject.Instantiate(Prefab);

        yield return ModifyPrefab(prefab);

        gameObject.Set(prefab);
        _cachedPrefab = prefab;
        yield break;
    }

    protected override IEnumerator ModifyPrefab(GameObject prefab)
    {
        prefab.GetComponentInChildren<Text>().text = FriendlyName;
        yield break;
    }
}
