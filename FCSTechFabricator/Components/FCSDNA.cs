using FCSCommon.Utilities;
using FCSTechFabricator.Enums;
using SMLHelper.V2.Handlers;
using UnityEngine;


namespace FCSTechFabricator.Components
{
    public class FCSDNA : MonoBehaviour
    {
        private bool _initialized;

        public TechType GiveItem { get; set; }
        public TechType TechType { get; set; }
        public FCSEnvironment Environment { get; set; }
        public bool IsPlantable { get; set; }
        public GameObject Model { get; set; }
        
        private void GetPickupableData(GameObject go)
        {
            var pickable = go.GetComponent<Pickupable>();
            if (pickable)
            {
                if (pickable.GetTechType() == TechType.CoralChunk)
                {
                    Environment = FCSEnvironment.Water;
                    GiveItem = TechType.CoralChunk;
                    IsPlantable = true;
                }

                if (pickable.GetTechType() == TechType.CrashPowder)
                {
                    Environment = FCSEnvironment.Water;
                    GiveItem = TechType.CrashPowder;
                    Model = CraftData.GetPrefabForTechType(TechType.CrashHome);
                    IsPlantable = true;
                }

                if (pickable.GetTechType() == TechType.JeweledDiskPiece)
                {
                    Environment = FCSEnvironment.Water;
                    GiveItem = TechType.JeweledDiskPiece;
                    IsPlantable = true;
                }
            }
        }

        private bool GetCreatureData(GameObject go)
        {
            var creature = go.GetComponentInChildren<Creature>();
            if (creature)
            {
                if (creature.gameObject.GetComponentInChildren<Skyray>())
                {
                    Environment = FCSEnvironment.Air;
                    IsPlantable = false;
                }

                Environment = FCSEnvironment.Water;
                IsPlantable = false;
                return true;
            }

            return false;
        }

        private bool GetPlantableData(GameObject go)
        {
            var plantable = go.GetComponentInChildren<Plantable>();
            if (plantable)
            {
                QuickLogger.Debug("Is Plantable");

                if (plantable.aboveWater)
                {
                    Environment = FCSEnvironment.Air;
                    IsPlantable = true;
                }

                if (plantable.underwater)
                {
                    Environment = FCSEnvironment.Water;
                    IsPlantable = true;
                }

                GiveItem = TechType;

                if (GiveItem == TechType.None)
                {
                    QuickLogger.Error($"Failed to get the PickTech from {TechType}", true);
                }

                Model = plantable.model;
                QuickLogger.Debug($"Model Is {Model?.name}");
                return true;
            }

            return false;
        }

        private bool GetPrefabForTechType(TechType? ingredientTechType, out GameObject go)
        {
            go = null;
            if (ingredientTechType == null || ingredientTechType == TechType.None) return true;

            TechType = (TechType) ingredientTechType;

            go = CraftData.GetPrefabForTechType(TechType, false);
            return false;
        }

        private static bool GetIngredientTechType(TechType techType, out TechType? ingredientTechType)
        {
            ingredientTechType = TechType.None;
            if (techType == TechType.None) return true;

            ingredientTechType = CraftDataHandler.GetModdedTechData(techType)?.GetIngredient(0)?.techType;
            return false;
        }

        private TechType GetTechType()
        {
            var techType = CraftData.GetTechType(gameObject);
            return techType;
        }

        public void GetDnaData()
        {
            if (!_initialized)
            {
                var techType = GetTechType();

                if (GetIngredientTechType(techType, out var ingredientTechType)) return;

                if (GetPrefabForTechType(ingredientTechType, out var go)) return;

                if (GetPlantableData(go)) return;

                if (GetCreatureData(go)) return;

                GetPickupableData(go);

                _initialized = true;
            }
        }
    }
}
