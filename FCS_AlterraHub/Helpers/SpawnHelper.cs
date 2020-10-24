using System.Collections.Generic;
using FCS_AlterraHub.Mono;
using FCSCommon.Helpers;
using UnityEngine;

namespace FCS_AlterraHub.Helpers
{
    public static class SpawnHelper
    {

        private static Dictionary<string, string> PlantResourceDictionary = new Dictionary<string, string>
        {
            { "[CORAL_REEF_PLANT_MIDDLE]","WorldEntities/Doodads/Coral_reef/coral_reef_plant_middle_03"},
            { "[CORAL_REEF_G3]","WorldEntities/Doodads/Coral_reef/coral_reef_grass_03"},
            { "[CORAL_REEF_SMALL_DECO]","WorldEntities/Doodads/Coral_reef/coral_reef_small_deco_14"},
            { "[PURPLE_FAN]","WorldEntities/Doodads/Coral_reef/Coral_reef_purple_fan"},
        };

        public static GameObject SpawnAtPoint(string location, Transform trans, float scale = 0.179f)
        {
            var obj = GameObject.Instantiate(Resources.Load<GameObject>(PlantResourceDictionary[location]));
            MaterialHelpers.ChangeWavingSpeed(obj,new Vector4(0f,0f,0.10f,0f));
            obj.transform.SetParent(trans,false);
            obj.transform.position = trans.position;
            obj.transform.localScale *= scale;
            Object.Destroy(obj.GetComponent<Rigidbody>());
            Object.Destroy(obj.GetComponent<WorldForces>());
            return obj;
        }

        public static bool ContainsPlant(string plantKey)
        {
            return PlantResourceDictionary.ContainsKey(plantKey);
        }
    }
}
