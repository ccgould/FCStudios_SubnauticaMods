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

                if (ingredientTechType == null || ingredientTechType == TechType.None) return;

                TechType = (TechType) ingredientTechType;

                var go = CraftData.GetPrefabForTechType(TechType, false);

                var plantable = go.GetComponentInChildren<Plantable>();
                var creature = go.GetComponentInChildren<Creature>();
                var pickable = go.GetComponent<Pickupable>();

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
                        QuickLogger.Error($"Failed to get the PickTech from {TechType}",true);
                    }

                    Model = plantable.model;
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
                    return;
                }

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

                }

                _initialized = true;
            }
        }
        public TechType GiveItem { get; set; }
        public TechType TechType { get; set; }
        public FCSEnvironment Environment { get; set; }
        public bool IsPlantable { get; set; }
        private bool _initialized;
        public GameObject Model { get; set; }
    }
}
