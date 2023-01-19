using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace FCS_AlterraHub.Extensions
{
    public static class GameObjectExtensions
    {
        public static Transform[] GetChildrenT(this GameObject go)
        {
            List<Transform> children = new List<Transform>();
            foreach (Transform tran in go.transform)
            {
                children.Add(tran);
            }
            return children.ToArray();
        }

        public static GameObject[] GetChildren(this GameObject go)
        {
            List<GameObject> children = new List<GameObject>();
            foreach (Transform tran in go.transform)
            {
                children.Add(tran.gameObject);
            }
            return children.ToArray();
        }

        public static T[] GetChildren<T>(this GameObject go)
        {
            List<T> children = new List<T>();
            foreach (Transform tran in go.transform)
            {
                var comp = tran.gameObject.GetComponent<T>();
                if (comp == null) continue;
                children.Add(comp);
            }
            return children.ToArray();
        }
    }
}
