using FCS_AlterraHub.API;
using FCS_AlterraHub.Core.Services;
using FCS_AlterraHub.ModItems.FCSPDA.Enums;
using FCS_AlterraHub.ModItems.FCSPDA.Mono;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
//#if SUBNAUTICA
//using Sprite = Atlas.Sprite;
//#endif

namespace FCS_AlterraHub.ModItems.FCSPDA.Mono.ScreenItems
{
    public class RadialMenu : MonoBehaviour
    {
        private readonly List<RadialMenuEntry> _entries = new List<RadialMenuEntry>();
        private float Radius = 280;
        public int TabAmount = 8;

        internal RadialMenuEntry AddEntry(FCSAlterraHubGUI controller, Sprite pIcon, Text pageLabel, string buttonName, PDAPages pages)
        {
            
            GameObject entry = Instantiate(FCSAssetBundlesService.PublicAPI.GetLocalPrefab("RadialMenuEntry"), transform);
            entry.transform.localScale = new Vector3(1.4f, 1.4f, 1.4f);
            RadialMenuEntry rme = entry.EnsureComponent<RadialMenuEntry>();
            rme.Initialize(controller, transform.parent.Find("PageName").gameObject.GetComponent<Text>(), pIcon, pageLabel, buttonName, pages);
            _entries.Add(rme);
            return rme;
        }

        internal void Rearrange()
        {
            var radiansOfSeperation = Mathf.PI * 2 / _entries.Count;
            for (int i = 0; i < _entries.Count; i++)
            {
                var x = Mathf.Sin(radiansOfSeperation * i) * Radius;
                var y = Mathf.Cos(radiansOfSeperation * i) * Radius;

                _entries[i].GetComponent<RectTransform>().anchoredPosition = new Vector2(x, y);
            }
        }
    }
}
