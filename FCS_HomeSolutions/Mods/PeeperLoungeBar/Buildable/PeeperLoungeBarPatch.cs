using System;
using System.IO;
using FCS_AlterraHub.API;
using FCS_AlterraHub.Helpers;
using FCS_HomeSolutions.Buildables;
using FCS_HomeSolutions.Configuration;
using FCS_HomeSolutions.Mods.PeeperLoungeBar.Mono;
using FCS_HomeSolutions.Spawnables;
using FCS_HomeSolutions.Structs;
using FCSCommon.Helpers;
using FCSCommon.Utilities;
using SMLHelper.V2.Utility;
using UnityEngine;
#if SUBNAUTICA
using Sprite = Atlas.Sprite;

#endif
namespace FCS_HomeSolutions.Mods.PeeperLoungeBar.Buildable
{
    internal class PeeperLoungeBarPatch : DecorationEntryPatch
    {
        public PeeperLoungeBarPatch() : base(Mod.PeeperLoungeBarClassId, Mod.PeeperLoungeBarFriendly, Mod.PeeperLoungeBarDescription, ModelPrefab.GetPrefab(Mod.PeeperLoungeBarPrefabName, true), new Settings
        {
            KitClassID = "ahsSweetWaterBar_kit",
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
        })
        {

        }

        public override GameObject GetGameObject()
        {
            try
            {

                var prefab = GameObject.Instantiate(_prefab);

                //Disable the object so we can fill in the properties before awake
                prefab.SetActive(false);

                GameObjectHelpers.AddConstructableBounds(prefab, _settings.Size, _settings.Center);

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

                constructable.allowedOutside = _settings.AllowedOutside;
                constructable.allowedInBase = _settings.AllowedInBase;
                constructable.allowedOnGround = _settings.AllowedOnGround;
                constructable.allowedOnWall = _settings.AllowedOnWall;
                constructable.rotationEnabled = _settings.RotationEnabled;
                constructable.allowedOnCeiling = _settings.AllowedOnCeiling;
                constructable.allowedInSub = _settings.AllowedInSub;
                constructable.allowedOnConstructables = _settings.AllowedOnConstructables;
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
                Prefab = FCSAssetBundlesService.PublicAPI.GetPrefabByName("SpicyChips", FCSAssetBundlesService.PublicAPI.GlobalBundleName),
                ClassId = "SpicyChips",
                Friendly = "MrSpicy Chips",
                Description = "Your taste buds will be screaming in delight from the very first bite of MrSpicy Chips. Get energized for the big project or running for your life on an alien planet, or celebrate your victories over even the most mundane tasks with MrSpicy Chips: an adventure in every bag.",
                Cost = 205,
                Food = 5,
                Water = -1
            });
            mrSpicyChip.Patch();
            Mod.PeeperBarFoods.Add(mrSpicyChip.TechType, mrSpicyChip.Cost);

            var z1Chips = new FoodSpawnable(new PeeperBarFoodItemData
            {
                Prefab = FCSAssetBundlesService.PublicAPI.GetPrefabByName("Z1Chips", FCSAssetBundlesService.PublicAPI.GlobalBundleName),
                ClassId = "Z1Snacks",
                Friendly = "Z1 Snacks",
                Description = "A Countdown to flavor and adventuring, Z1 Snacks bring out your inner intrepid adventurer. Whether you’re running toward adventure or running away because you found it, Z1 Snacks are the ideal compliment. Perfect taste on Z3...Z2...Z1 Snacks!",
                Cost = 205,
                Food = 15,
                Water = -1
            });
            z1Chips.Patch();
            Mod.PeeperBarFoods.Add(z1Chips.TechType, z1Chips.Cost);

            var nutritionPackage = new FoodSpawnable(new PeeperBarFoodItemData
            {
                Prefab = FCSAssetBundlesService.PublicAPI.GetPrefabByName("NutritionPackage", FCSAssetBundlesService.PublicAPI.GlobalBundleName),
                ClassId = "NutritionPackage",
                Friendly = "Alterra Nutrient Pouch",
                Description = "Packed with more nutrients than a momma marsupial could provide, you can feel confident of writing home of your good health and not getting in trouble later at the family reunion. It’s even tastier than the packaging it comes in!",
                Cost = 100,
                Food = 15,
                Water = 1
            });
            nutritionPackage.Patch();
            Mod.PeeperBarFoods.Add(nutritionPackage.TechType, nutritionPackage.Cost);

            var spicyPop = new FoodSpawnable(new PeeperBarFoodItemData
            {
                Prefab = FCSAssetBundlesService.PublicAPI.GetPrefabByName("fcs_Dirnk01", FCSAssetBundlesService.PublicAPI.GlobalBundleName),
                ClassId = "SpicyPop",
                Friendly = "MrSpicy Pop",
                Description = "Wake up those taste buds and the rest of you too. A delicious compliment MrSpicy chips -- IF you’re brave enough -- Spicy Pop packs a crate full of flavor in every bottle. Made with zero caffeine because who could take a nap with all that flavor on your tongue!",
                Cost = 300,
                Food = 0,
                Water = 60
            });
            spicyPop.Patch();
            Mod.PeeperBarFoods.Add(spicyPop.TechType, spicyPop.Cost);

            var extraHotCoffee = new FoodSpawnable(new PeeperBarFoodItemData
            {
                Prefab = FCSAssetBundlesService.PublicAPI.GetPrefabByName("ExtraHotCoffee", FCSAssetBundlesService.PublicAPI.GlobalBundleName),
                ClassId = "ExtraHotCoffee",
                Friendly = "Extra Hot Coffee",
                Description = "Whether you’re chilled to the bone and about to die from arctic winds or just keep getting distracted by co-workers, Alterra Extra Hot Coffee will keep you going with rich taste, deep flavor, and always the right amount of cream, sugar, salt, hot chili sauce, bacon, or...whatever makes coffee great:  Alterra knows and Alterra delivers.",
                Cost = 250,
                Food = 0,
                Water = 50
            });
            extraHotCoffee.Patch();
            Mod.PeeperBarFoods.Add(extraHotCoffee.TechType, extraHotCoffee.Cost);

            var fcsOrangeSoda = new FoodSpawnable(new PeeperBarFoodItemData
            {
                Prefab = FCSAssetBundlesService.PublicAPI.GetPrefabByName("fcsOrangeSoda", FCSAssetBundlesService.PublicAPI.GlobalBundleName),
                ClassId = "FCSOrangeSoda",
                Friendly = "FCS Orange Soda",
                Description = "Ah, the simple, joyful flavor of True-Natural synthetic oranges and a little carbon-dioxide. A simple and delicious soda to sparkle the tongue and enjoy youthful memories.",
                Cost = 300,
                Food = 0,
                Water = 60
            });
            fcsOrangeSoda.Patch();
            Mod.PeeperBarFoods.Add(fcsOrangeSoda.TechType, fcsOrangeSoda.Cost);

            var peeperWhiskey = new FoodSpawnable(new PeeperBarFoodItemData
            {
                Prefab = FCSAssetBundlesService.PublicAPI.GetPrefabByName("PeeperWhiskey", FCSAssetBundlesService.PublicAPI.GlobalBundleName),
                ClassId = "PeeperWhiskey",
                Friendly = "Peeper Whiskey",
                Description = "Always a unique flavor, this special whiskey is created from uniquely local ingredients for the perfect taste of your far-away place. Specially tailored for alcoholic effects on peepers; please do not give Peeper Whiskey to underage peepers.",
                Cost = 425,
                Food = 0,
                Water = 85
            });
            peeperWhiskey.Patch();
            Mod.PeeperBarFoods.Add(peeperWhiskey.TechType, peeperWhiskey.Cost);

            var alterraScotch = new FoodSpawnable(new PeeperBarFoodItemData
            {
                Prefab = FCSAssetBundlesService.PublicAPI.GetPrefabByName("AlterraScotch", FCSAssetBundlesService.PublicAPI.GlobalBundleName),
                ClassId = "AlterraScotch",
                Friendly = "Alterra Scotch",
                Description = "An aged whisky made by a single distillery using only malted barley and water, with a complex and mellow flavor. Perfect for taking the edge off a rough day.  (Dealcoholized for your comfort and convenience and reduced chance of hangover)",
                Cost = 475,
                Food = 0,
                Water = 95
            });
            alterraScotch.Patch();
            Mod.PeeperBarFoods.Add(alterraScotch.TechType, alterraScotch.Cost);


            Mod.PeeperBarFoods.Add(TechType.NutrientBlock, 100);
        }

        internal static void LoadPeeperLoungeTracks()
        {
            Mod.AudioClips.Add("PLB_AnnoyingFish", new SoundEntry
            {
                Sound = AudioUtils.CreateSound(Path.Combine(Mod.GetAssetPath(), "Audio", "PeeperLoungeBar_AnnoyingFish.mp3")),
                Message = "These fish are so annoying. Would you mind taking them out?"
            });
            Mod.AudioClips.Add("PLB_Hello", new SoundEntry
            {
                Sound = AudioUtils.CreateSound(Path.Combine(Mod.GetAssetPath(), "Audio", "PeeperLoungeBar_Hello.mp3")),
                Message = "Hello Ryley! would you like to purchase something?"
            });
            Mod.AudioClips.Add("PLB_ThankYou", new SoundEntry
            {
                Sound = AudioUtils.CreateSound(Path.Combine(Mod.GetAssetPath(), "Audio", "PeeperLoungeBar_ThankYou.mp3")),
                Message = "Thank You! Please come back again!"
            });
            Mod.AudioClips.Add("PLB_Intro", new SoundEntry
            {
                Sound = AudioUtils.CreateSound(Path.Combine(Mod.GetAssetPath(), "Audio", "PeeperLoungeBar_Intro.mp3")),
                Message = "Hello, my name is Peeper Lounge, nice to meet you. My function is to make your leisure time fun and efficient with a great selection of drinks and snacks from the Alterra Corporation. I am able to speak to you through your FCS PDA, isn't that great? However, if you prefer, you can disable my voice in the settings for FCS products."
            });
            Mod.AudioClips.Add("PLB_FishRemoved", new SoundEntry
            {
                Sound = AudioUtils.CreateSound(Path.Combine(Mod.GetAssetPath(), "Audio", "PeeperLoungeBar_FishRemoved.mp3")),
                Message = "Thank you for taking those pesky fish out!"
            });
            Mod.AudioClips.Add("PLB_NoCardDetected", new SoundEntry
            {
                Sound = AudioUtils.CreateSound(Path.Combine(Mod.GetAssetPath(), "Audio", "PeeperLoungeBar_NoCardDetected.mp3")),
                Message = "I maybe blind since I am just a robot, but I can't seem to locate your debit card on your body anywhere!"
            });
            Mod.AudioClips.Add("PLB_NotEnoughCredit", new SoundEntry
            {
                Sound = AudioUtils.CreateSound(Path.Combine(Mod.GetAssetPath(), "Audio", "PeeperLoungeBar_NotEnoughCredit.mp3")),
                Message = "TI'm sorry, but it seems you do not have enough credit for this purchase."
            });
        }
    }
}
