using System.Collections.Generic;
using UnityEngine;

namespace FCS_AlterraHub.Extensions
{
    public static class GameObjectExtensions
    {
        public static GameObject[] GetChildren(GameObject parent, bool recursive = false)
        {
            List<GameObject> items = new List<GameObject>();
            for (int i = 0; i < parent.transform.childCount; i++)
            {
                items.Add(parent.transform.GetChild(i).gameObject);
                if (recursive)
                { // set true to go through the hiearchy.
                    items.AddRange(GetChildren(parent.transform.GetChild(i).gameObject, recursive));
                }
            }
            return items.ToArray();
        }
    }
}
