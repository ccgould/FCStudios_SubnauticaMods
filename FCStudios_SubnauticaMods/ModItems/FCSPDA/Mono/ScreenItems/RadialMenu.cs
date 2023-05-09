using FCS_AlterraHub.API;
using FCS_AlterraHub.ModItems.FCSPDA.Enums;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
//#if SUBNAUTICA
//using Sprite = Atlas.Sprite;
//#endif

namespace FCS_AlterraHub.ModItems.FCSPDA.Mono.ScreenItems;

public class RadialMenu : MonoBehaviour
{
    private readonly Dictionary<PDAPages, RadialMenuEntry> _entries = new();
    private float Radius = 280;

    internal RadialMenuEntry AddEntry(FCSAlterraHubGUI controller, Sprite pIcon, Text pageLabel, string buttonName, PDAPages pages,bool isActive = true)
    {
        GameObject entry = Instantiate(FCSAssetBundlesService.PublicAPI.GetLocalPrefab("RadialMenuEntry"), transform);
        entry.SetActive(isActive);
        entry.transform.localScale = new Vector3(1.4f, 1.4f, 1.4f);
        RadialMenuEntry rme = entry.EnsureComponent<RadialMenuEntry>();
        rme.Initialize(controller, transform.parent.Find("PageName").gameObject.GetComponent<Text>(), pIcon, pageLabel, buttonName, pages);
        _entries.Add(pages,rme);
        Rearrange();
        return rme;
    }

    private void Update()
    {
        if (_entries.Count == 0 || !_entries.ContainsKey(PDAPages.BaseDevices)) return;

        if (Player.main.IsInBase())
        {
            if (!_entries[PDAPages.BaseDevices].gameObject.activeSelf)
            {
                EnableTab(PDAPages.BaseDevices);
            }
        }
        else
        {
            if (_entries[PDAPages.BaseDevices].gameObject.activeSelf)
            {
                DisableTab(PDAPages.BaseDevices);
            }
        }
    }


    internal void Rearrange()
    {
        var activeButtons = _entries.Where(x=>x.Value.gameObject.activeSelf).ToList();

        var radiansOfSeperation = Mathf.PI * 2 / activeButtons.Count;
        for (int i = 0; i < activeButtons.Count; i++)
        {
            var go = activeButtons[i].Value.gameObject;

            if (!go.activeSelf) continue;

            var x = Mathf.Sin(radiansOfSeperation * i) * Radius;
            var y = Mathf.Cos(radiansOfSeperation * i) * Radius;

            go.GetComponent<RectTransform>().anchoredPosition = new Vector2(x, y);
        }
    }

    internal void DisableTab(PDAPages page)
    {
        _entries[page].gameObject.SetActive(false);
        Rearrange();
    }

    internal void EnableTab(PDAPages page)
    {
        _entries[page].gameObject.SetActive(true);
        Rearrange();
    }
}
