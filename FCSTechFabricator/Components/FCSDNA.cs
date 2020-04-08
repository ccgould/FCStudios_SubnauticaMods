using FCSCommon.Utilities;
using FCSTechFabricator.Enums;
using SMLHelper.V2.Handlers;
using UnityEngine;


namespace FCSTechFabricator.Components
{
    public class FCSDNA : MonoBehaviour
    {
        public void GetData()
        {
            if (!_initialized)
            {
                var techType = CraftData.GetTechType(gameObject);

                if (techType == TechType.None) return;

                var ingredientTechType = CraftDataHandler.GetModdedTechData(techType)?.GetIngredient(0)?.techType;

                QuickLogger.Debug($"Trying to GetData from {ingredientTechType}");

                if (ingredientTechType == null || ingredientTechType == TechType.None) return;

                TechType = (TechType) ingredientTechType;

                var go = CraftData.GetPrefabForTechType(TechType, false);

                var plantable = go.GetComponentInChildren<Plantable>();

                QuickLogger.Debug($"Planter Result: {plantable?.name}");

                var creature = go.GetComponentInChildren<Creature>();
                QuickLogger.Debug($"Creature Result: {creature?.name}");

                var pickable = go.GetComponent<Pickupable>();

                QuickLogger.Debug($"Pickable Result: {pickable?.name}");

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

                    Model = plantable.model;
                    _initialized = true;
                    QuickLogger.Debug($"Model Is {Model?.name}");
                    return;
                }

                if (creature)
                {
                    if (creature.gameObject.GetComponentInChildren<Skyray>())
                    {
                        Environment = FCSEnvironment.Air;
                        IsPlantable = false;
                    }

                    Environment = FCSEnvironment.Water;
                    IsPlantable = false;
                    _initialized = true;
                    return;
                }

                if (pickable)
                {
                    if (pickable.GetTechType() == TechType.CoralChunk)
                    {
                        Environment = FCSEnvironment.Water;
                        IsPlantable = true;
                        _initialized = true;
                    }

                }
            }
        }


        public TechType TechType { get; set; }
        public FCSEnvironment Environment { get; set; }
        public bool IsPlantable { get; set; }
        private bool _initialized;
        public GameObject Model { get; set; }
    }
}
