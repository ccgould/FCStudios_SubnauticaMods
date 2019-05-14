using FCS_Alterra_Refrigeration_Solutions.Configuration;
using FCS_Alterra_Refrigeration_Solutions.Logging;
using FCSCommon.Objects;
using Oculus.Newtonsoft.Json;
using SMLHelper.V2.Handlers;
using System;
using System.Collections.Generic;
using System.IO;

namespace FCS_Alterra_Refrigeration_Solutions.Managers
{
    public static class FileManager
    {
        private static List<TechType> _foodList;

        /// <summary>
        /// Loads the list of allow techTypes from the configure file
        /// </summary>
        /// <param name="listLocation"></param>
        /// <returns></returns>
        public static List<TechType> LoadFoodTypes(string listLocation)
        {
            _foodList = new List<TechType>();

            // Read the config file
            var json = File.ReadAllText(Path.Combine(Information.ASSETSFOLDER, listLocation));

            // Deserialize the config file into a JsonObject
            var jObj = JsonConvert.DeserializeObject<List<string>>(json);

            foreach (var techType in jObj)
            {
                AddAndCheckTechType(techType);
            }

            return _foodList;
        }

        public static List<EatableEntities> LoadEatableEntities(string listName)
        {
            var fileString = File.ReadAllText(Path.Combine(Information.ASSETSFOLDER, listName));
            return JsonConvert.DeserializeObject<List<EatableEntities>>(fileString);
        }
        private static void AddAndCheckTechType(string item)
        {

            //Log.Info(item);

            // Check if the TechType exist
            if (!Enum.IsDefined(typeof(TechType), item))
            {
                //If the item is an custom item try to find the TechType if not found report an error
                Log.Info($"Trying to get custom TechType: {item}");
                if (TechTypeHandler.TryGetModdedTechType(item, out TechType customTechType))
                {
                    //Log.Info($"Added {item}");
                    _foodList.Add(customTechType);
                }
                else
                {
                    Log.Error($"TechType {item} is invalid");
                }
            }
            else
            {
                //Log.Info($"Added {item}");

                _foodList.Add((TechType)Enum.Parse(typeof(TechType), item));
            }
        }

        /// <summary>
        /// Destroys set objects in the class
        /// </summary>
        public static void Destroy()
        {
            _foodList.Clear();
        }

    }
}
