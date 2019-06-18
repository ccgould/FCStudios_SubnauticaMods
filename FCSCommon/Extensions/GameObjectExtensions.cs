using FCSCommon.Objects;
using Oculus.Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

namespace FCSCommon.Extensions
{
    public static class GameObjectExtensions
    {
        /// <summary>
        /// Returns the component of Type type. If one doesn't already exist on the GameObject it will be added.
        /// </summary>
        /// <typeparam name="T">The type of Component to return.</typeparam>
        /// <param name="gameObject">The GameObject this Component is attached to.</param>
        /// <returns>Component</returns>
        public static T GetOrAddComponent<T>(this GameObject gameObject) where T : Component
        {
            return gameObject.GetComponent<T>() ?? gameObject.AddComponent<T>();
        }

        private static List<EatableEntities> _eatableEntites = new List<EatableEntities>();

        /// <summary>
        /// /
        /// </summary>
        /// <param name="ASSETSFOLDER">Location of the directory for the file.</param>
        /// <param name="fileToRead">Name of the file to read the list of prefab locations</param>
        public static void GetAllFoodValues(string ASSETSFOLDER, string fileToRead)
        {
            var g = File.ReadAllLines(Path.Combine(ASSETSFOLDER, fileToRead));

            for (int i = 0; i < g.Length; i++)
            {

                GameObject OriginalFabricator = Resources.Load<GameObject>(g[i]);

                GameObject prefab = GameObject.Instantiate(OriginalFabricator);


                var eatable = prefab?.GetComponent<Eatable>();

                if (eatable != null)
                {
                    _eatableEntites.Add(new EatableEntities
                    {
                        Name = eatable.name,
                        FoodValue = eatable.foodValue,
                        WaterValue = eatable.waterValue,
                        KDecayRate = eatable.kDecayRate
                    });
                }
            }

            string output = JsonConvert.SerializeObject(_eatableEntites, Formatting.Indented);
            File.WriteAllText(Path.Combine(ASSETSFOLDER, "Output.txt"), output);
        }

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
    }
}
