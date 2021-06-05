using System.Collections.Generic;
using FCS_AlterraHub.Mods.FCSPDA.Enums;
using UnityEngine;
using UnityEngine.UI;

namespace FCS_AlterraHub.Mods.FCSPDA.Mono.Model
{
    public class RadialMenu : MonoBehaviour
    {
        private readonly List<RadialMenuEntry> _entries = new List<RadialMenuEntry>();
        private float Radius = 280;
        public int TabAmount = 8;

        internal RadialMenuEntry AddEntry(FCSPDAController controller, Atlas.Sprite pIcon, Text pageLabel, string buttonName, PDAPages pages)
        {
            GameObject entry = Instantiate(Buildables.AlterraHub.PDARadialMenuEntryPrefab, transform);
            entry.transform.localScale = new Vector3(1.4f, 1.4f, 1.4f);
            RadialMenuEntry rme = entry.EnsureComponent<RadialMenuEntry>();
            rme.Initialize(controller,transform.parent.Find("PageName").gameObject.GetComponent<Text>(),pIcon,pageLabel,buttonName,pages);
            _entries.Add(rme);
            return rme;
        }

        internal void Rearrange()
        {
            var radiansOfSeperation = (Mathf.PI * 2) / _entries.Count;
            for (int i = 0; i < _entries.Count; i++)
            {
                var x = Mathf.Sin(radiansOfSeperation * i) * Radius;
                var y = Mathf.Cos(radiansOfSeperation * i) * Radius;

                _entries[i].GetComponent<RectTransform>().anchoredPosition = new Vector2(x, y);
            }
        }
    }
}
