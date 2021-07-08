using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FCS_AlterraHub.Model;
using FCS_AlterraHub.Mono;
using FCS_AlterraHub.Registration;
using FCSCommon.Utilities;
using SMLHelper.V2.Handlers;
using SMLHelper.V2.Utility;
using UnityEngine;
using UnityEngine.UI;

namespace FCS_AlterraHub.Helpers
{
    public static class WorldHelpers
    {
        private static bool _useSystemTime;
        private static int _lastMinute;
        private static DigitalClockFormat _format;
        private static string _previousTime;

        public static Dictionary<TechType, PickReturnsData> PickReturns = new()
        {
            //Marine Flora
            { TechType.AcidMushroomSpore, new PickReturnsData { ReturnType = TechType.AcidMushroom, IsLandPlant = false } },
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

        public enum DigitalClockFormat
        {
            TWELVE_HOUR,
            TWENTY_FOUR_HOUR
        }

        public static bool CheckIfInRange(FcsDevice currentDevice, FcsDevice device, float range)
        {
            if (currentDevice == null || device == null || !device.gameObject.activeSelf ||
                !currentDevice.IsConstructed) return false;
            float distance = Vector3.Distance(currentDevice.gameObject.transform.position,
                device.gameObject.transform.position);
            return distance <= range;
        }

        public static float GetDistance(FcsDevice currentDevice, FcsDevice device)
        {
            if (currentDevice == null || device == null) return 0f;
            return Vector3.Distance(currentDevice.gameObject.transform.position, device.gameObject.transform.position);
        }

        public static bool CheckIfInRange(GameObject mainGameObject, GameObject gameObject, float range)
        {
            return Vector3.Distance(mainGameObject.transform.position, gameObject.transform.position) <= range;
        }

        public static PingType CreatePingType(string id, string pingName, Atlas.Sprite pingSprite)
        {
            SpriteHandler.RegisterSprite(SpriteManager.Group.Pings, id, pingSprite);

            var pingType = PingHandler.RegisterNewPingType(pingName, pingSprite);

            if (!FCSAlterraHubService.InternalAPI.PingTypes.ContainsKey(pingType))
            {
                FCSAlterraHubService.InternalAPI.PingTypes.Add(pingType, pingName);
            }

            return pingType;
        }

#if SUBNAUTICA
        public static float GetDepth(GameObject gameObject)
        {
            return gameObject == null ? 0f : Ocean.main.GetDepthOf(gameObject);
        }
#elif BELOWZERO
        public static float GetDepth(GameObject gameObject)
        {
            return gameObject == null ? 0f : Ocean.GetDepthOf(gameObject);
        }
#endif

        public static PingInstance CreateBeacon(GameObject gameObject, PingType pingType, string label,
            bool isEnable = true)
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
    }

    public struct PickReturnsData
    {
        public TechType ReturnType { get; set; }
        public bool IsLandPlant { get; set; }
    }
}
