using System;
using System.Collections.Generic;
using System.IO;
using FCS_AlterraHub.API;
using FCS_AlterraHub.Helpers;
using FCS_HomeSolutions.Buildables;
using FCS_HomeSolutions.Configuration;
using FCS_HomeSolutions.Mods.PeeperLoungeBar.Mono;
using FCS_HomeSolutions.Spawnables;
using FCS_HomeSolutions.Structs;
using FCSCommon.Utilities;
#if SUBNAUTICA_STABLE
using Oculus.Newtonsoft.Json;
#else
using Newtonsoft.Json;
#endif
using SMLHelper.V2.Utility;
using UnityEngine;
#if SUBNAUTICA
using Sprite = Atlas.Sprite;

#endif

namespace FCS_HomeSolutions.Mods.PeeperLoungeBar.Buildable
{
    internal class PeeperLoungeBarPatch : DecorationEntryPatch
    {
        internal const string PeeperLoungeBarClassId = "PeeperLoungeBar";
        internal const string PeeperLoungeBarFriendly = "Peeper Lounge Bar";

        internal const string PeeperLoungeBarDescription =
            "AI-driven specialty food and drinks kiosk with small aquarium.";

        internal const string PeeperLoungeBarPrefabName = "PeeperLoungeBar";
        internal static string PeeperLoungeBarKitClassID = $"{PeeperLoungeBarClassId}_Kit";

        private static readonly Settings Settings = new Settings
        {
            KitClassID = PeeperLoungeBarKitClassID,
            AllowedInBase = true,
            AllowedOutside = true,
            AllowedOnGround = true,
            RotationEnabled = true,
            Cost = 45000,
            Size = new Vector3(2.020296f, 1.926336f, 2.067809f),
            Center = new Vector3(0, 1.5901f, 0),
            CategoryForPDA = TechCategory.Misc,
            GroupForPDA = TechGroup.Miscellaneous,
            IconName = "PeeperLoungeBar"
        };

        public PeeperLoungeBarPatch() : base(PeeperLoungeBarClassId, PeeperLoungeBarFriendly,
            PeeperLoungeBarDescription, ModelPrefab.GetPrefabFromGlobal(PeeperLoungeBarPrefabName), Settings)
        {
        }

        public override GameObject GetGameObject()
        {
            try
            {
                var prefab = GameObject.Instantiate(_prefab);

                //Disable the object so we can fill in the properties before awake
                prefab.SetActive(false);

                GameObjectHelpers.AddConstructableBounds(prefab, Settings.Size, Settings.Center);

                var model = prefab.FindChild("model");

                //========== Allows the building animation and material colors ==========// 
                Shader shader = Shader.Find("MarmosetUBER");
                Renderer[] renderers = prefab.GetComponentsInChildren<Renderer>();
                SkyApplier skyApplier = prefab.EnsureComponent<SkyApplier>();
                skyApplier.renderers = renderers;
                skyApplier.anchorSky = Skies.Auto;
                //========== Allows the building animation and material colors ==========// 

                // Add constructible
                var constructable = prefab.AddComponent<Constructable>();

                constructable.allowedOutside = Settings.AllowedOutside;
                constructable.allowedInBase = Settings.AllowedInBase;
                constructable.allowedOnGround = Settings.AllowedOnGround;
                constructable.allowedOnWall = Settings.AllowedOnWall;
                constructable.rotationEnabled = Settings.RotationEnabled;
                constructable.allowedOnCeiling = Settings.AllowedOnCeiling;
                constructable.allowedInSub = Settings.AllowedInSub;
                constructable.allowedOnConstructables = Settings.AllowedOnConstructables;
                constructable.model = model;
                constructable.techType = TechType;

                prefab.AddComponent<PrefabIdentifier>().ClassId = ClassID;
                prefab.AddComponent<TechTag>().type = TechType;
                prefab.AddComponent<DecorationController>();
                prefab.AddComponent<PeeperLoungeBarController>();

                //CreateWaterPark(prefab, constructable);
                CreateAquarium(prefab);

                prefab.SetActive(true);
                MaterialHelpers.ApplyGlassShaderTemplate(prefab, "_glass", Mod.ModPackID);
                return prefab;
            }
            catch (Exception e)
            {
                QuickLogger.Error(e.Message);
                QuickLogger.Error(e.StackTrace);
            }

            return null;
        }

        private void CreateAquarium(GameObject prefab)
        {
            var aquarium = prefab.AddComponent<Aquarium>();

            var sRootGo = GameObjectHelpers.FindGameObject(prefab, "StorageRoot");
            var sc = prefab.AddComponent<StorageContainer>();
            var sRoot = sRootGo.EnsureComponent<ChildObjectIdentifier>();
            sRoot.classId = ClassID;
            sc.storageRoot = sRoot;
            sc.width = 1;
            sc.height = 3;

            aquarium.fishRoot = GameObjectHelpers.FindGameObject(prefab, "grownPlant");
            aquarium.trackObjects = new[]
            {
                GameObjectHelpers.FindGameObject(prefab, "FishItem1"),
                GameObjectHelpers.FindGameObject(prefab, "FishItem2"),
                GameObjectHelpers.FindGameObject(prefab, "FishItem3"),
            };
            aquarium.storageContainer = sc;
            sc.enabled = false;
        }

        private void CreateWaterPark(GameObject prefab, Constructable constructable)
        {
            var sRootGo = GameObjectHelpers.FindGameObject(prefab, "StorageRoot");
            var slot1 = GameObjectHelpers.FindGameObject(prefab, "slot1");
            var slot2 = GameObjectHelpers.FindGameObject(prefab, "slot2");
            var slot3 = GameObjectHelpers.FindGameObject(prefab, "slot3");
            var slot4 = GameObjectHelpers.FindGameObject(prefab, "slot4");

            var gPlant = GameObjectHelpers.FindGameObject(prefab, "grownPlant");
            var gPlantCo = gPlant.AddComponent<ChildObjectIdentifier>();
            gPlantCo.classId = ClassID;
            var sc = prefab.AddComponent<StorageContainer>();
            var sRoot = sRootGo.EnsureComponent<ChildObjectIdentifier>();
            sRoot.classId = ClassID;
            sc.storageRoot = sRoot;
            sc.width = 2;
            sc.height = 2;
            var planter = prefab.AddComponent<Planter>();
            planter.storageContainer = sc;
            planter.slots = new[]
            {
                slot1.transform,
                slot2.transform,
                slot3.transform,
                slot4.transform,
            };

            planter.grownPlantsRoot = gPlant.transform;
            planter.environment = Planter.PlantEnvironment.Water;
            planter.constructable = constructable;
            planter.isIndoor = true;

            //Reactivate mesh now that settings are set

            var wp = prefab.AddComponent<WaterPark>();
            wp.planter = planter;
            wp.wpPieceCapacity = 2;
            wp.itemsRoot = sRootGo.transform;
            //wp.height = 0.4824555f;
        }

        internal static void PatchFood()
        {
            var mrSpicyChip = new FoodSpawnable(new PeeperBarFoodItemData
            {
                Prefab = FCSAssetBundlesService.PublicAPI.GetPrefabByName("SpicyChips",
                    FCSAssetBundlesService.PublicAPI.GlobalBundleName),
                ClassId = "SpicyChips",
                Friendly = "MrSpicy Chips",
                Description =
                    "Your taste buds will be screaming in delight from the very first bite of MrSpicy Chips. Get energized for the big project or running for your life on an alien planet, or celebrate your victories over even the most mundane tasks with MrSpicy Chips: an adventure in every bag.",
                Cost = 205,
                Food = 5,
                Water = -1
            });
            mrSpicyChip.Patch();
            Mod.PeeperBarFoods.Add(mrSpicyChip.TechType, mrSpicyChip.Cost);

            var z1Chips = new FoodSpawnable(new PeeperBarFoodItemData
            {
                Prefab = FCSAssetBundlesService.PublicAPI.GetPrefabByName("Z1Chips",
                    FCSAssetBundlesService.PublicAPI.GlobalBundleName),
                ClassId = "Z1Snacks",
                Friendly = "Z1 Snacks",
                Description =
                    "A Countdown to flavor and adventuring, Z1 Snacks bring out your inner intrepid adventurer. Whether you’re running toward adventure or running away because you found it, Z1 Snacks are the ideal compliment. Perfect taste on Z3...Z2...Z1 Snacks!",
                Cost = 205,
                Food = 15,
                Water = -1
            });
            z1Chips.Patch();
            Mod.PeeperBarFoods.Add(z1Chips.TechType, z1Chips.Cost);

            var nutritionPackage = new FoodSpawnable(new PeeperBarFoodItemData
            {
                Prefab = FCSAssetBundlesService.PublicAPI.GetPrefabByName("NutritionPackage",
                    FCSAssetBundlesService.PublicAPI.GlobalBundleName),
                ClassId = "NutritionPackage",
                Friendly = "Alterra Nutrient Pouch",
                Description =
                    "Packed with more nutrients than a momma marsupial could provide. (It’s even tastier than the packaging it comes in!)",
                Cost = 100,
                Food = 15,
                Water = 1
            });
            nutritionPackage.Patch();
            Mod.PeeperBarFoods.Add(nutritionPackage.TechType, nutritionPackage.Cost);

            var spicyPop = new FoodSpawnable(new PeeperBarFoodItemData
            {
                Prefab = FCSAssetBundlesService.PublicAPI.GetPrefabByName("SpicyPop",
                    FCSAssetBundlesService.PublicAPI.GlobalBundleName),
                ClassId = "SpicyPop",
                Friendly = "MrSpicy Pop",
                Description =
                    "Wake up those taste buds and the rest of you too. A delicious compliment MrSpicy chips -- IF you’re brave enough -- Spicy Pop packs a crate full of flavor in every bottle. Made with zero caffeine because who could take a nap with all that flavor on your tongue!",
                Cost = 300,
                Food = 0,
                Water = 60
            });
            spicyPop.Patch();
            Mod.PeeperBarFoods.Add(spicyPop.TechType, spicyPop.Cost);

            var extraHotCoffee = new FoodSpawnable(new PeeperBarFoodItemData
            {
                Prefab = FCSAssetBundlesService.PublicAPI.GetPrefabByName("ExtraHotCoffee",
                    FCSAssetBundlesService.PublicAPI.GlobalBundleName),
                ClassId = "ExtraHotCoffee",
                Friendly = "Extra Hot Coffee",
                Description =
                    "Whether you’re chilled to the bone and about to die from arctic winds or just keep getting distracted by co-workers, Alterra Extra Hot Coffee will keep you going with rich taste, deep flavor, and always the right amount of cream, sugar, salt, hot chili sauce, bacon, or...whatever makes coffee great:  Alterra knows and Alterra delivers.",
                Cost = 250,
                Food = 0,
                Water = 50
            });
            extraHotCoffee.Patch();
            Mod.PeeperBarFoods.Add(extraHotCoffee.TechType, extraHotCoffee.Cost);

            var fcsOrangeSoda = new FoodSpawnable(new PeeperBarFoodItemData
            {
                Prefab = FCSAssetBundlesService.PublicAPI.GetPrefabByName("fcsOrangeSoda",
                    FCSAssetBundlesService.PublicAPI.GlobalBundleName),
                ClassId = "FCSOrangeSoda",
                Friendly = "FCS Orange Soda",
                Description =
                    "Ah, the simple, joyful flavor of True-Natural synthetic oranges and a little carbon-dioxide. A simple and delicious soda to sparkle the tongue and enjoy youthful memories.",
                Cost = 300,
                Food = 0,
                Water = 60
            });
            fcsOrangeSoda.Patch();
            Mod.PeeperBarFoods.Add(fcsOrangeSoda.TechType, fcsOrangeSoda.Cost);

            var peeperWhiskey = new FoodSpawnable(new PeeperBarFoodItemData
            {
                Prefab = FCSAssetBundlesService.PublicAPI.GetPrefabByName("PeeperWhiskey",
                    FCSAssetBundlesService.PublicAPI.GlobalBundleName),
                ClassId = "PeeperWhiskey",
                Friendly = "Peeper Whiskey",
                Description =
                    "Always a unique flavor, this special whiskey is created from uniquely local ingredients for the perfect taste of your far-away place. Specially tailored for alcoholic effects on peepers; please do not give Peeper Whiskey to underage peepers.",
                Cost = 425,
                Food = 0,
                Water = 85
            });
            peeperWhiskey.Patch();
            Mod.PeeperBarFoods.Add(peeperWhiskey.TechType, peeperWhiskey.Cost);

            var alterraScotch = new FoodSpawnable(new PeeperBarFoodItemData
            {
                Prefab = FCSAssetBundlesService.PublicAPI.GetPrefabByName("AlterraScotch",
                    FCSAssetBundlesService.PublicAPI.GlobalBundleName),
                ClassId = "AlterraScotch",
                Friendly = "Alterra Scotch",
                Description =
                    "An aged whisky made by a single distillery using only malted barley and water, with a complex and mellow flavor. Perfect for taking the edge off a rough day.  (Dealcoholized for your comfort and convenience and reduced chance of hangover)",
                Cost = 475,
                Food = 0,
                Water = 95
            });
            alterraScotch.Patch();
            Mod.PeeperBarFoods.Add(alterraScotch.TechType, alterraScotch.Cost);


            var coffeePackage = new FoodSpawnable(new PeeperBarFoodItemData
            {
                Prefab = FCSAssetBundlesService.PublicAPI.GetPrefabByName("CoffeePackage",
                    FCSAssetBundlesService.PublicAPI.GlobalBundleName),
                ClassId = "CoffeePackage",
                Friendly = "Coffee Package",
                Description =
                    "Freshly roasted and ground coffee beans, packed in argon gas, and three-fold wrapped in foil. You’ll swear you were standing next to the roaster when you breathe in the heady aroma of perfectly roasted coffee beans.",
                Cost = 300,
                Food = 2,
                Water = 0
            });
            coffeePackage.Patch();
            Mod.PeeperBarFoods.Add(coffeePackage.TechType, coffeePackage.Cost);

            var carolinaPeeperBalls = new FoodSpawnable(new PeeperBarFoodItemData
            {
                Prefab = FCSAssetBundlesService.PublicAPI.GetPrefabByName("SpicyCheeseBalls",
                    FCSAssetBundlesService.PublicAPI.GlobalBundleName),
                ClassId = "CarolinaPeeperBalls",
                Friendly = "Carolina Peeper Balls",
                Description =
                    "Peeper Balls, Caroline style. Ingredients: Sugar, Gum Base, Carolina Reaper Peppers, Balls of Peeper (make of that what you will), Carnauba Wax, Corn Starch, Artificial Flavors and Colors. Scoville Heat Units: ERRNumericOverflow.",
                Cost = 300,
                Food = 20,
                Water = -5
            });
            carolinaPeeperBalls.Patch();
            Mod.PeeperBarFoods.Add(carolinaPeeperBalls.TechType, carolinaPeeperBalls.Cost);

            var alterraMug01 = new FoodSpawnable(new PeeperBarFoodItemData
            {
                Prefab = FCSAssetBundlesService.PublicAPI.GetPrefabByName("AlterraMug01",
                    FCSAssetBundlesService.PublicAPI.GlobalBundleName),
                ClassId = "AlterraCoffeeMug",
                Friendly = "Alterra Coffee (Mug)",
                Description =
                    "Coffee… Delicious, sumptuous coffee. Proof Alterra Corporation  loves us and wants us to be happy.",
                Cost = 300,
                Food = 0,
                Water = 50
            });
            alterraMug01.Patch();
            Mod.PeeperBarFoods.Add(alterraMug01.TechType, alterraMug01.Cost);

            var alterraMug02 = new FoodSpawnable(new PeeperBarFoodItemData
            {
                Prefab = FCSAssetBundlesService.PublicAPI.GetPrefabByName("AlterraMug02",
                    FCSAssetBundlesService.PublicAPI.GlobalBundleName),
                ClassId = "FCSCoffeeMug",
                Friendly = "FCS Coffee (Mug)",
                Description = "Coffee… Delicious, sumptuous coffee. Proof FCStudios loves us and wants us to be happy.",
                Cost = 300,
                Food = 0,
                Water = 50
            });
            alterraMug02.Patch();
            Mod.PeeperBarFoods.Add(alterraMug02.TechType, alterraMug02.Cost);

            var alterraMug03 = new FoodSpawnable(new PeeperBarFoodItemData
            {
                Prefab = FCSAssetBundlesService.PublicAPI.GetPrefabByName("AlterraMug03",
                    FCSAssetBundlesService.PublicAPI.GlobalBundleName),
                ClassId = "MrSpicyCoffeeMug",
                Friendly = "MrSpicy Coffee (Mug)",
                Description = "Coffee… Delicious, sumptuous coffee. Proof MrSpicy loves us and wants us to be happy.",
                Cost = 300,
                Food = 0,
                Water = 50
            });
            alterraMug03.Patch();
            Mod.PeeperBarFoods.Add(alterraMug03.TechType, alterraMug03.Cost);

            var alterraMug04 = new FoodSpawnable(new PeeperBarFoodItemData
            {
                Prefab = FCSAssetBundlesService.PublicAPI.GetPrefabByName("AlterraMug04",
                    FCSAssetBundlesService.PublicAPI.GlobalBundleName),
                ClassId = "Z1GamingCoffeeMug",
                Friendly = "Z1 Gaming Coffee (Mug)",
                Description =
                    "Coffee… Delicious, sumptuous coffee. Proof Z1 Gaming  loves us and wants us to be happy.",
                Cost = 300,
                Food = 0,
                Water = 50
            });
            alterraMug04.Patch();
            Mod.PeeperBarFoods.Add(alterraMug04.TechType, alterraMug04.Cost);

            var alterraMug05 = new FoodSpawnable(new PeeperBarFoodItemData
            {
                Prefab = FCSAssetBundlesService.PublicAPI.GetPrefabByName("AlterraMug05",
                    FCSAssetBundlesService.PublicAPI.GlobalBundleName),
                ClassId = "MonikaCinnyRollCoffeeMug",
                Friendly = "Monika C.R. Coffee (Mug)",
                Description =
                    "Coffee… Delicious, sumptuous coffee. Proof Monkia CinnyRoll loves us and wants us to be happy.",
                Cost = 300,
                Food = 0,
                Water = 50
            });
            alterraMug05.Patch();
            Mod.PeeperBarFoods.Add(alterraMug05.TechType, alterraMug05.Cost);

            var alterraMug06 = new FoodSpawnable(new PeeperBarFoodItemData
            {
                Prefab = FCSAssetBundlesService.PublicAPI.GetPrefabByName("AlterraMug06",
                    FCSAssetBundlesService.PublicAPI.GlobalBundleName),
                ClassId = "AlterraCoffeeMug02",
                Friendly = "Alterra Coffee (Mug) 2",
                Description =
                    "Delicious, sumptuous coffee. Proof Alterra Corporation loves us and wants us to be happy.",
                Cost = 300,
                Food = 0,
                Water = 50
            });
            alterraMug06.Patch();
            Mod.PeeperBarFoods.Add(alterraMug06.TechType, alterraMug06.Cost);

            var alterraMug07 = new FoodSpawnable(new PeeperBarFoodItemData
            {
                Prefab = FCSAssetBundlesService.PublicAPI.GetPrefabByName("AlterraMug07",
                    FCSAssetBundlesService.PublicAPI.GlobalBundleName),
                ClassId = "PeeperCoffeeMug",
                Friendly = "Peeper Coffee (Mug)",
                Description =
                    "Delicious, sumptuous coffee. Proof the Alterra omniLounge Artificial Intelligence loves us and wants us to be happy.",
                Cost = 300,
                Food = 0,
                Water = 50
            });
            alterraMug07.Patch();
            Mod.PeeperBarFoods.Add(alterraMug07.TechType, alterraMug07.Cost);

            Mod.PeeperBarFoods.Add(TechType.NutrientBlock, 100);
        }

        internal static void LoadPeeperLoungeTracks()
        {
            QuickLogger.Debug(
                $"Peeper Lounge Path: {Path.Combine(Mod.GetAudioFolderPath(), "PeeperLoungeBarTrackConfig.json")}");

            // read file into a string and deserialize JSON to a type
            var audioEntries = JsonConvert.DeserializeObject<List<AudioData>>(
                File.ReadAllText(Path.Combine(Mod.GetAudioFolderPath(), "PeeperLoungeBarTrackConfig.json")));

            if (audioEntries == null)
            {
                QuickLogger.Error("Failed to load peeper lounge bar audio tracks");
                return;
            }

            QuickLogger.Debug(
                $"Attempting to load Peeper Lounge Bar audio tracks. Collection size: {audioEntries.Count}");


            foreach (AudioData audioData in audioEntries)
            {
                Mod.AudioClips.Add(audioData.Key, new SoundEntry
                {
                    Sound = AudioUtils.CreateSound(Path.Combine(Mod.GetAssetPath(), "Audio",
                        $"{audioData.TrackName}.mp3")),
                    Message = audioData.Caption,
                    IsRandom = audioData.IsRandom
                });
                QuickLogger.Debug($"Loaded Peeper Lounge Bar track: {audioData.TrackName}");
            }
        }
    }

    internal class AudioData
    {
        public string Key { get; set; }
        public string TrackName { get; set; }
        public string Caption { get; set; }
        public bool IsRandom { get; set; }
    }
}