
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace FCSCommon.Extensions
{
    internal static class GameObjectExtensions
    {

        public static string GetFoodValue(string itemLocation)
        {
            if (string.IsNullOrEmpty(itemLocation)) return "No location";

            StringBuilder sb = new StringBuilder();

            GameObject OriginalFabricator = Resources.Load<GameObject>(itemLocation);

            GameObject prefab = GameObject.Instantiate(OriginalFabricator);


            var eatable = prefab?.GetComponent<Eatable>();

            if (eatable != null)
            {
                sb.Append("{");
                sb.Append(Environment.NewLine);
                sb.Append($"\"Name\":\"{eatable.name}\",");
                sb.Append(Environment.NewLine);
                sb.Append($"\"WaterValue\":{eatable.GetWaterValue()},");
                sb.Append(Environment.NewLine);
                sb.Append($"\"FoodValue\":{eatable.GetFoodValue()},");
                sb.Append(Environment.NewLine);
                sb.Append($"\"kDecayRate\":{eatable.kDecayRate},");
                sb.Append(Environment.NewLine);
                sb.Append("}");
            }

            return sb.ToString();
        }

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
}
