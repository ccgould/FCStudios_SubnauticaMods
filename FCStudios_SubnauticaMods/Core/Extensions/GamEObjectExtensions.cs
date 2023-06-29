using System.Collections.Generic;
using UnityEngine;

namespace FCS_AlterraHub.Core.Extensions;
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
}
