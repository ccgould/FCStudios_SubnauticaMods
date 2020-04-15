using System.Collections.Generic;
using System.IO;
using FCSCommon.Utilities;
using FCSTechFabricator.Components;
using FCSTechFabricator.Configuration;
using SMLHelper.V2.Assets;
using SMLHelper.V2.Crafting;
using SMLHelper.V2.Utility;
using UnityEngine;

namespace FCSTechFabricator.Craftables
{
    public class FCSDNASample : Craftable
    {
        private string _assetFolder = Mod.GetAssetPath();
        private TechType _ingredient;
        private int _amountToReturn;


        public FCSDNASample(string classId, string friendlyName, string description, TechType ingredient, int amountToReturn) : base(classId, friendlyName, description)
        {
            _ingredient = ingredient;
            _amountToReturn = amountToReturn;
        }
        
        public override GameObject GetGameObject()
        {
            var prefab = GameObject.Instantiate(CraftData.GetPrefabForTechType(TechType.HatchingEnzymes));
            prefab.name = this.PrefabFileName;
            var dna = prefab.AddComponent<FCSDNA>();
            
            prefab.AddComponent<FCSTechFabricatorTag>();
            return prefab;
        }

#if SUBNAUTICA
        protected override TechData GetBlueprintRecipe()
        {
            QuickLogger.Debug($"Creating recipe...");
            // Create and associate recipe to the new TechType
            var customFabRecipe = new TechData()
            {
                craftAmount = 1,
                Ingredients = new List<Ingredient>()
                {
                    new Ingredient(_ingredient,_amountToReturn)
                }
            };
            return customFabRecipe;
        }

        protected override Atlas.Sprite GetItemSprite()
        {
            return ImageUtils.LoadSpriteFromFile(Path.Combine(_assetFolder, $"{ClassID}.png"));
        }

#elif BELOWZERO
        protected override RecipeData GetBlueprintRecipe()
        {
                        QuickLogger.Debug($"Creating recipe...");
            // Create and associate recipe to the new TechType
            var customFabRecipe = new RecipeData()
            {
                craftAmount = 1,
                Ingredients = new List<Ingredient>()
                {
                    new Ingredient(_ingredient,_amountToReturn)
                }
            };
            return customFabRecipe;
        }

        protected override Sprite GetItemSprite()
        {
            return ImageUtils.LoadSpriteFromFile(Path.Combine(_assetFolder,  $"{ClassID}.png"));
        }
#endif
        public void ChangeIconLocation(string assetFolder)
        {
            _assetFolder = assetFolder;
        }

        public override TechGroup GroupForPDA => TechGroup.Resources;
        public override TechCategory CategoryForPDA => TechCategory.AdvancedMaterials;
        //public override string IconFileName => _iconFileName;
        public override string AssetsFolder => _assetFolder;
    }
}