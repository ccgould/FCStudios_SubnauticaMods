using System;
using System.Collections.Generic;
using System.Linq;
using FCS_AlterraHub.API;
using FCS_AlterraHub.Models;
using FCS_AlterraHub.Models.Abstract;
using Nautilus.Handlers;
using UnityEngine;


namespace FCS_AlterraHub.Core.Helpers;

public static class WorldHelpers
{
    private static bool _useSystemTime;
    private static int _lastMinute;
    private static DigitalClockFormat _format;
    private static string _previousTime;
    public static List<TechType> Craftables { get; set; } = new();

    public static Dictionary<TechType, PickReturnsData> PickReturns = new()
    {
        //Marine Flora
        { TechType.AcidMushroomSpore, 
            new PickReturnsData 
            { ReturnType = TechType.AcidMushroom, 
                IsLandPlant = false 
            } 
        },
        {
            TechType.BloodOil,
            new PickReturnsData
            {
                ReturnType = TechType.BloodOil,
                IsLandPlant = false
            }
        },
        {
            TechType.BluePalmSeed,
            new PickReturnsData { ReturnType = TechType.BluePalmSeed, IsLandPlant = false }
        },
        {
            TechType.KooshChunk,
            new PickReturnsData { ReturnType = TechType.KooshChunk, IsLandPlant = false }
        },
        {
            TechType.PurpleBranchesSeed,
            new PickReturnsData { ReturnType = TechType.PurpleBranchesSeed, IsLandPlant = false }
        },
        {
            TechType.CreepvineSeedCluster,
            new PickReturnsData { ReturnType = TechType.CreepvineSeedCluster, IsLandPlant = false }
        },
        {
            TechType.CreepvinePiece,
            new PickReturnsData { ReturnType = TechType.CreepvinePiece, IsLandPlant = false }
        },
        {
            TechType.WhiteMushroomSpore,
            new PickReturnsData { ReturnType = TechType.WhiteMushroomSpore, IsLandPlant = false }
        },
        {
            TechType.EyesPlantSeed,
            new PickReturnsData { ReturnType = TechType.EyesPlantSeed, IsLandPlant = false }
        },
        {
            TechType.RedRollPlantSeed,
            new PickReturnsData { ReturnType = TechType.RedRollPlantSeed, IsLandPlant = false }
        },
        {
            TechType.GabeSFeatherSeed,
            new PickReturnsData { ReturnType = TechType.GabeSFeatherSeed, IsLandPlant = false }
        },
        {
            TechType.JellyPlantSeed,
            new PickReturnsData { ReturnType = TechType.JellyPlant, IsLandPlant = false }
        },
        {
            TechType.RedGreenTentacleSeed,
            new PickReturnsData { ReturnType = TechType.RedGreenTentacleSeed, IsLandPlant = false }
        },
        {
            TechType.SnakeMushroomSpore,
            new PickReturnsData { ReturnType = TechType.SnakeMushroomSpore, IsLandPlant = false }
        },
        {
            TechType.MembrainTreeSeed,
            new PickReturnsData { ReturnType = TechType.MembrainTreeSeed, IsLandPlant = false }
        },
        {
            TechType.SmallFanSeed,
            new PickReturnsData { ReturnType = TechType.SmallFanSeed, IsLandPlant = false }
        },
        {
            TechType.RedBushSeed,
            new PickReturnsData { ReturnType = TechType.RedBushSeed, IsLandPlant = false }
        },
        {
            TechType.RedConePlantSeed,
            new PickReturnsData { ReturnType = TechType.RedConePlantSeed, IsLandPlant = false }
        },
        {
            TechType.RedBasketPlantSeed,
            new PickReturnsData { ReturnType = TechType.RedBasketPlantSeed, IsLandPlant = false }
        },
        {
            TechType.SeaCrownSeed,
            new PickReturnsData { ReturnType = TechType.SeaCrownSeed, IsLandPlant = false }
        },
        {
            TechType.ShellGrassSeed,
            new PickReturnsData { ReturnType = TechType.ShellGrassSeed, IsLandPlant = false }
        },
        {
            TechType.SpottedLeavesPlantSeed,
            new PickReturnsData { ReturnType = TechType.SpottedLeavesPlantSeed, IsLandPlant = false }
        },
        {
            TechType.SpikePlantSeed,
            new PickReturnsData { ReturnType = TechType.SpikePlantSeed, IsLandPlant = false }
        },
        {
            TechType.PurpleFanSeed,
            new PickReturnsData { ReturnType = TechType.PurpleFanSeed, IsLandPlant = false }
        },
        {
            TechType.PurpleStalkSeed,
            new PickReturnsData { ReturnType = TechType.PurpleStalkSeed, IsLandPlant = false }
        },
        {
            TechType.PurpleTentacleSeed,
            new PickReturnsData { ReturnType = TechType.PurpleTentacleSeed, IsLandPlant = false }
        },
        //Land-Based Flora
        {
            TechType.BulboTreePiece,
            new PickReturnsData { ReturnType = TechType.BulboTreePiece, IsLandPlant = true }
        },
        {
            TechType.PurpleVegetable,
            new PickReturnsData { ReturnType = TechType.PurpleVegetable, IsLandPlant = true }
        },
        {
            TechType.FernPalmSeed,
            new PickReturnsData { ReturnType = TechType.FernPalmSeed, IsLandPlant = true }
        },
        {
            TechType.OrangeMushroomSpore,
            new PickReturnsData { ReturnType = TechType.OrangeMushroomSpore, IsLandPlant = true }
        },
        {
            TechType.OrangePetalsPlantSeed,
            new PickReturnsData { ReturnType = TechType.OrangePetalsPlantSeed, IsLandPlant = true }
        },
        {
            TechType.HangingFruit,
            new PickReturnsData { ReturnType = TechType.HangingFruit, IsLandPlant = true }
        },
        {
            TechType.MelonSeed, new PickReturnsData { ReturnType = TechType.Melon, IsLandPlant = true }
        },
        {
            TechType.PurpleVasePlantSeed,
            new PickReturnsData { ReturnType = TechType.PurpleVasePlantSeed, IsLandPlant = true }
        },
        {
            TechType.PinkMushroomSpore,
            new PickReturnsData { ReturnType = TechType.PinkMushroomSpore, IsLandPlant = true }
        },
        {
            TechType.PurpleRattleSpore,
            new PickReturnsData { ReturnType = TechType.PurpleRattleSpore, IsLandPlant = true }
        },
        {
            TechType.PinkFlowerSeed,
            new PickReturnsData { ReturnType = TechType.PinkFlowerSeed, IsLandPlant = true }
        },
        {
            TechType.PurpleBrainCoralPiece,
            new PickReturnsData { ReturnType = TechType.PurpleBrainCoralPiece, IsLandPlant = true }
        },
        //UnPlantable
        {
            TechType.StalkerTooth,
            new PickReturnsData { ReturnType = TechType.StalkerTooth, IsLandPlant = false }
        },
        {
            TechType.CoralChunk,
            new PickReturnsData { ReturnType = TechType.CoralChunk, IsLandPlant = false }
        },
        {
            TechType.JeweledDiskPiece, new PickReturnsData { ReturnType = TechType.JeweledDiskPiece, IsLandPlant = false }
        },
    };

    public static HashSet<TechType> IsNonePlantableAllowedList = new()
    {

        TechType.StalkerTooth,
        TechType.GasPod,
        TechType.JeweledDiskPiece,
        TechType.Floater,
        TechType.TreeMushroomPiece,
        TechType.CoralChunk,
        TechType.CrashPowder,
    };

    public static HashSet<TechType> Eatables = new HashSet<TechType>
    {
        TechType.CookedPeeper,
        TechType.CookedHoleFish,
        TechType.CookedGarryFish,
        TechType.CookedReginald,
        TechType.CookedBladderfish,
        TechType.CookedHoverfish,
        TechType.CookedSpadefish,
        TechType.CookedBoomerang,
        TechType.CookedEyeye,
        TechType.CookedOculus,
        TechType.CookedHoopfish,
        TechType.CookedSpinefish,
        TechType.CookedLavaEyeye,
        TechType.CookedLavaBoomerang,
#if BELOWZERO
        TechType.CookedSpinnerfish,
        TechType.CookedSymbiote,
        TechType.CookedArcticPeeper,
        TechType.CookedArrowRay,
        TechType.CookedNootFish,
        TechType.CookedTriops,
        TechType.CookedFeatherFish,
        TechType.CookedFeatherFishRed,
        TechType.CookedDiscusFish,
        TechType.CuredSpinnerfish,
        TechType.CuredSymbiote,
        TechType.CuredArcticPeeper,
        TechType.CuredArrowRay,
        TechType.CuredNootFish,
        TechType.CuredTriops,
        TechType.CuredFeatherFish,
        TechType.CuredFeatherFishRed,
        TechType.CuredDiscusFish,
#endif
        TechType.CuredPeeper,
        TechType.CuredHoleFish,
        TechType.CuredGarryFish,
        TechType.CuredReginald,
        TechType.CuredBladderfish,
        TechType.CuredHoverfish,
        TechType.CuredSpadefish,
        TechType.CuredBoomerang,
        TechType.CuredEyeye,
        TechType.CuredOculus,
        TechType.CuredHoopfish,
        TechType.CuredSpinefish,
        TechType.CuredLavaEyeye,
        TechType.CuredLavaBoomerang,
    };
    public static string GetGameTimeFormat()
    {
        string period = string.Empty;
        TimeSpan timeSpan = GetTime();
        if (_lastMinute != timeSpan.Minutes)
        {
            if (_format == DigitalClockFormat.TWELVE_HOUR)
            {
                bool flag;
                timeSpan = TwentyFourHourToTwelveHourFormat(timeSpan, out flag);
                period = (flag ? "AM" : "PM");
            }

            _lastMinute = timeSpan.Minutes;
            _previousTime =
                $"{EncodeMinHourToString(EncodeMinuteAndHour(timeSpan.Minutes, timeSpan.Hours))} {period}";
        }

        return _previousTime;
    }

    public static void SetFormat(DigitalClockFormat format)
    {
        _format = format;
    }

    public static string EncodeMinHourToString(int encoded)
    {
        if (encoded >= 0 && encoded < 1440)
        {
            return TimeCache.S_TimeCache[encoded];
        }

        return "Er:rr";
    }

    public static int EncodeMinuteAndHour(int minute, int hour)
    {
        return hour * 60 + minute;
    }

    public static TimeSpan TwentyFourHourToTwelveHourFormat(TimeSpan timeSpan, out bool isMorning)
    {
        int num = timeSpan.Hours;
        isMorning = (num < 12);
        num %= 12;
        if (num == 0)
        {
            num += 12;
        }

        return new TimeSpan(num, timeSpan.Minutes, timeSpan.Seconds);
    }

    private static TimeSpan GetTime()
    {
        if (_useSystemTime)
        {
            DateTime now = DateTime.Now;
            new TimeSpan(now.Hour, now.Minute, now.Second);
            return new TimeSpan(now.Hour, now.Minute, now.Second);
        }

        float dayScalar = DayNightCycle.main.GetDayScalar();
        int hours = Mathf.FloorToInt(dayScalar * 24f);
        int minutes = Mathf.FloorToInt(Mathf.Repeat(dayScalar * 24f * 60f, 60f));
        return new TimeSpan(hours, minutes, 0);
    }

    public static List<TechType> BlackList = new List<TechType>() { TechType.Titanium, TechType.Copper };
    private static float _temperature;

    public enum DigitalClockFormat
    {
        TWELVE_HOUR,
        TWENTY_FOUR_HOUR
    }

    public static bool CheckIfInRange(FCSDevice currentDevice, FCSDevice device, float range)
    {
        if (currentDevice == null || device == null || !device.gameObject.activeSelf ||
            !currentDevice.IsConstructed) return false;
        float distance = Vector3.Distance(currentDevice.gameObject.transform.position,
            device.gameObject.transform.position);
        return distance <= range;
    }

    public static bool CheckIfPlayerInRange(FCSDevice device, float range)
    {
        if (device == null || !device.gameObject.activeSelf ||
            !device.IsConstructed) return false;
        float distance = Vector3.Distance(Player.main.gameObject.transform.position,
            device.gameObject.transform.position);
        return distance <= range;
    }

    public static bool CheckIfPlayerInRange(GameObject device, float range)
    {
        if (device == null || !device.activeSelf) return false;
        float distance = Vector3.Distance(Player.main.gameObject.transform.position,
            device.gameObject.transform.position);
        return distance <= range;
    }

    //public static bool CheckIfInRange(FCSDevice currentDevice, BaseManager device, float range)
    //{
    //    Collider[] hitColliders = Physics.OverlapSphere(currentDevice.GetPosition(), range);
    //    foreach (var hitCollider in hitColliders)
    //    {
    //        var baseManager = BaseManager.FindManager(hitCollider.gameObject);
    //        QuickLogger.Debug($"Base Returned: {baseManager?.BaseFriendlyID ?? "None"}, {hitCollider.gameObject.name}", true);
    //        if (baseManager == device)
    //        {
    //            return true;
    //        }
    //    }
    //    return false;
    //}

    public static float GetDistance(FCSDevice currentDevice, FCSDevice device)
    {
        if (currentDevice == null || device == null) return 0f;
        return Vector3.Distance(currentDevice.gameObject.transform.position, device.gameObject.transform.position);
    }

    public static float GetDistance(FCSDevice currentDevice, GameObject device)
    {
        if (currentDevice == null || device == null) return 0f;
        return Vector3.Distance(currentDevice.gameObject.transform.position, device.transform.position);
    }

    public static bool CheckIfInRange(GameObject mainGameObject, GameObject gameObject, float range)
    {
        return Vector3.Distance(mainGameObject.transform.position, gameObject.transform.position) <= range;
    }

    public static PingType CreatePingType(string id, string pingName, Atlas.Sprite pingSprite)
    {
        SpriteHandler.RegisterSprite(SpriteManager.Group.Pings, id, pingSprite);

        PingType pingType = EnumHandler.AddEntry<PingType>(pingName).WithIcon(pingSprite);

        FCSModsAPI.InternalAPI.AddPingType(pingName,pingType);

        return pingType;
    }

    public static float GetDepth(GameObject gameObject)
    {
        return gameObject == null ? 0f : Ocean.GetDepthOf(gameObject);
    }

    public static PingInstance CreateBeacon(GameObject gameObject, PingType pingType, string label, bool isEnable = true)
    {
        var pingInstance = gameObject.EnsureComponent<PingInstance>();

        if (pingInstance != null)
        {
            pingInstance.enabled = false;
            pingInstance.pingType = pingType;
            pingInstance.origin = gameObject.transform;
            pingInstance.SetLabel(label);
            pingInstance.enabled = isEnable;
        }

        return pingInstance;
    }

    public static bool KnownPickTypesContains(TechType techType)
    {
        return PickReturns.ContainsKey(techType);
    }

    public static PickReturnsData GetPickTypeData(TechType techType)
    {
        if (PickReturns.ContainsKey(techType))
        {
            return PickReturns[techType];
        }

        return new PickReturnsData();
    }

    public static GameObject FindNearestObject(List<Transform> gameObjects, Transform transform)
    {
        Transform bestTarget = null;
        float closestDistanceSqr = Mathf.Infinity;
        Vector3 currentPosition = transform.position;
        foreach (Transform potentialTarget in gameObjects)
        {
            Vector3 directionToTarget = potentialTarget.position - currentPosition;
            float dSqrToTarget = directionToTarget.sqrMagnitude;
            if (dSqrToTarget < closestDistanceSqr)
            {
                closestDistanceSqr = dSqrToTarget;
                bestTarget = potentialTarget;
            }
        }

        return bestTarget?.gameObject;
    }

    public static void GetCraftables()
    {
        Craftables.Clear();

        var fabricator = CraftTree.GetTree(CraftTree.Type.Fabricator);
        GetCraftTreeData(fabricator.nodes);

        var cyclopsFabricator = CraftTree.GetTree(CraftTree.Type.CyclopsFabricator);
        GetCraftTreeData(cyclopsFabricator.nodes);

        var workbench = CraftTree.GetTree(CraftTree.Type.Workbench);
        GetCraftTreeData(workbench.nodes);

        var maproom = CraftTree.GetTree(CraftTree.Type.MapRoom);
        GetCraftTreeData(maproom.nodes);

        var seamothUpgrades = CraftTree.GetTree(CraftTree.Type.SeamothUpgrades);
        GetCraftTreeData(seamothUpgrades.nodes);
    }

    private static void GetCraftTreeData(CraftNode innerNodes)
    {
        foreach (CraftNode craftNode in innerNodes)
        {
            //QuickLogger.Debug($"Craftable: {craftNode.id} | {craftNode.string0} | {craftNode.string1} | {craftNode.techType0}");

            if (string.IsNullOrWhiteSpace(craftNode.id)) continue;
            if (craftNode.id.Equals("CookedFood") || craftNode.id.Equals("CuredFood")) return;
            if (craftNode.techType0 != TechType.None)
            {
                if (!IsItemUnlocked(craftNode.techType0, true)) continue;
                Craftables.Add(craftNode.techType0);
            }

            if (craftNode.childCount > 0)
            {
                GetCraftTreeData(craftNode);
            }
        }
    }

    public static bool IsItemUnlocked(TechType techType, bool useDefault = false)
    {
#if DEBUG
        QuickLogger.Debug($"Checking if {Language.main.Get(techType)} is unlocked");
#endif
        if (useDefault)
        {
            return CrafterLogic.IsCraftRecipeUnlocked(techType);
        }


        if (UWEHelpers.RequiresIngredients())
        {
            if (!BepInEx.Bootstrap.Chainloader.PluginInfos.Values.Any(x => x.Metadata.Name.Equals("UITweaks")))
            {
                
                CraftData.TechData data = GetData(techType);
                int ingredientCount = data?.ingredientCount ?? 0;
                for (int i = 0; i < ingredientCount; i++)
                {
                    CraftData.Ingredient ingredient = data._ingredients[i];
                    if (!BlackList.Contains(techType) && !CrafterLogic.IsCraftRecipeUnlocked(ingredient.techType))
                    {
#if DEBUG
                        QuickLogger.Debug($"{Language.main.Get(ingredient.techType)} is locked");
#endif
                        return false;
                    }
                }
            }
            else
            {
#if SUBNAUTICA
                if (CraftData.techData.TryGetValue(techType, out CraftData.TechData data))
                {
                    int ingredientCount = data?.ingredientCount ?? 0;
                    for (int i = 0; i < ingredientCount; i++)
                    {
                        IIngredient ingredient = data.GetIngredient(i);
                        if (!BlackList.Contains(techType) &&
                            !CrafterLogic.IsCraftRecipeUnlocked(ingredient.techType))
                        {
#if DEBUG
                            QuickLogger.Debug($"{Language.main.Get(techType)} is locked");
#endif
                            return false;
                        }
                    }
                }
#elif BELOWZERO
#endif
            }
        }

#if DEBUG
        QuickLogger.Debug($"{Language.main.Get(techType)} is unlocked");
#endif
        return true;
    }

    internal static CraftData.TechData GetData(TechType techType)
    {
        return CraftData.techData[techType];
    }

    public static bool IsNear(Vector3 a, Vector3 b)
    {
        return a.x < b.x + 0.1f && a.x > b.x - 0.1f && a.y < b.y + 0.1f && a.y > b.y - 0.1f && a.z < b.z + 0.1f && a.z > b.z - 0.1f;
    }

    public static GameObject GetRoot(GameObject go)
    {
        if (go == null) return null;

        GameObject gameObject = UWE.Utils.GetEntityRoot(go);
        if (!gameObject)
        {
            gameObject = go;
        }

        return gameObject;
    }

    public static bool CheckIfPaused()
    {
        return Time.timeScale <= 0;
    }

    public static float GetOceanDepth()
    {
        return Ocean.GetOceanLevel();
    }

    public static string QueryTemperature(Transform transform)
    {
        WaterTemperatureSimulation main = WaterTemperatureSimulation.main;

        if (main)
        {
            _temperature = Mathf.Max(_temperature, main.GetTemperature(transform.position));
            var temp = Language.main.GetFormat("ThermalPlantCelsius", _temperature);

            return string.IsNullOrWhiteSpace(temp) ? "LOADING" : temp;
        }

        return "LOADING";
    }
   
}

public struct PickReturnsData
{
    public TechType ReturnType { get; set; }
    public bool IsLandPlant { get; set; }
}
