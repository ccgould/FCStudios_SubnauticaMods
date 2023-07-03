using FCS_AlterraHub.ModItems.FCSPDA.Enums;
using FCSCommon.Utilities;
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
    //private readonly Dictionary<PDAPages, RadialMenuEntry> _entries = new();
    private float Radius = 280;
    private KeyValuePair<PDAPages, RadialMenuEntry> selected;
    private readonly SortedList<PDAPages, RadialMenuEntry> _entries = new();
    [SerializeField]
    private GameObject _radialMenuEntryPrefab;
    [SerializeField]
    private Text _parentPageNameLabel;

    internal RadialMenuEntry AddEntry(FCSAlterraHubGUI controller, Sprite pIcon, Text pageLabel, string buttonName, PDAPages pages,bool isActive = true)
    {
        GameObject entry = Instantiate(_radialMenuEntryPrefab, transform);
        entry.SetActive(isActive);
        entry.transform.localScale = new Vector3(1.4f, 1.4f, 1.4f);
        RadialMenuEntry rme = entry.GetComponent<RadialMenuEntry>();
        rme.Initialize(controller, _parentPageNameLabel, pIcon, pageLabel, buttonName, pages);
        _entries.Add(pages,rme);
        Rearrange();
        return rme;
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
        if (!_entries[page].gameObject.activeSelf) return;
        _entries[page].gameObject.SetActive(false);
        Rearrange();
    }

    internal void EnableTab(PDAPages page)
    {
        if (_entries[page].gameObject.activeSelf) return;
        _entries[page].gameObject.SetActive(true);
        Rearrange();
    }

    internal void SelectNextItem()
    {
        ResetEntries();
        var result = CheckIfItemSelected();

        if (result)
        {
            var index = _entries.IndexOfValue(selected.Value);

            if (index + 1 < _entries.Count)
            { 
                var next = _entries.ElementAt(index + 1);

                next.Value.Select();
                selected = next;
            }
            else
            {
                var next = _entries.First();

                next.Value.Select();
                selected = next;
            }

        }


    }

    private bool CheckIfItemSelected()
    {
        if (selected.Value is null)
        {
            selected = _entries.First();
            selected.Value.Select();
            return false;
        }
        return true;
    }

    private void ResetEntries()
    {
        foreach (var entry in _entries)
        {
            var go = entry.Value;
            go.Deselect();
        }
    }

    internal void SelectPrevItem()
    {
        ResetEntries();

        var result = CheckIfItemSelected();

        if (result)
        {
            var index = _entries.IndexOfValue(selected.Value);

            if (index - 1 > -1)
            {
                var prev = _entries.ElementAt(index - 1);

                prev.Value.Select();
                selected = prev;
            }
            else
            {
                var prev = _entries.Last();

                prev.Value.Select();
                selected = prev;
            }
        }

    }

    internal void PressSelectedButton()
    {
        selected.Value?.OnPointerClick(null);
    }

    internal void ClearSelectedItem()
    {
        selected = new();
    }
}
