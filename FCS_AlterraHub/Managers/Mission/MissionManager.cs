using System;
using System.Collections.Generic;
using System.Linq;
using FCS_AlterraHub.Configuration;
using FCS_AlterraHub.Patches;
using FCSCommon.Extensions;
using FCSCommon.Utilities;
using UnityEngine;

namespace FCS_AlterraHub.Managers.Mission
{
    public class MissionManager : MonoBehaviour
    {
        public List<Mission> Missions { get; set; } = new List<Mission>();

        public static MissionManager Instance;

        private FCSGamePlaySettings GamePlaySettings => Mod.GamePlaySettings;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else if (Instance != this)
            {
                Destroy(gameObject);
            }
        }

        internal void Load()
        {
            if (Mod.GamePlaySettings.Missions.Count > 0)
            {
                Missions = Mod.GamePlaySettings.Missions;
            }
        }

        public int GetMissionCount()
        {
            return Missions.Count;
        }

        private void Update()
        {
           //QuickLogger.Debug($"Sun Positon Vector: X:{uSkyManager.main.SunDir.x} | Y:{uSkyManager.main.SunDir.y} | Z:{uSkyManager.main.SunDir.z}",true);

            foreach (Mission mission in Missions)
            {
                if(mission.IsComplete) continue;
                mission.CheckSelfValidateTasks();
            }
        }

        public void CompleteCurrentMission(string missionKey)
        {
            foreach (Mission mission in Missions)
            {
                if (mission.HasMissionKey(missionKey))
                {
                    mission.GetTask(missionKey).Complete(true);
                    break;
                }
            }

        }

        public void CreateStarterMission()
        {
            if (!Mod.GamePlaySettings.PlayStarterMission) return;
            AddMission("Alterra Credit System Test","Help Alterra test the connection between you and the system at Alterra Corp",new List<MissionTask>
            {
                new MissionTask("Ore Consumer Scan",1)
                {
                    Condition = new TaskCondition
                    {
                        MissionType = MissionType.Scan,
                        TechType = Mod.OreConsumerTechType,
                    },
                    Description = "Scan the Ore Consumer in the Jelly Caves biome by the abandoned Degasi Base.",
                    SelfValidate = true
                },
                new MissionTask("Build AlterraHub",1)
                {
                    Condition = new TaskCondition
                    {
                        MissionType = MissionType.Construct,
                        TechType = Mod.AlterraHubTechType
                    },
                    Description = "Build an AlterraHub at your base.",
                    SelfValidate = true
                },
                new MissionTask("Create Alterra Account",1)
                {
                    Condition = new TaskCondition
                    {
                        MissionType = MissionType.AccountCreation,
                    },
                    Description = "Create an Alterra Account",
                    SelfValidate = true
                },
                new MissionTask("Build two Ore Consumers",2)
                {
                    Condition = new TaskCondition
                    {
                        MissionType = MissionType.Construct,
                        TechType = Mod.OreConsumerTechType
                    },
                    Description = "Build two ore consumers",
                    SelfValidate = true
                },
                new MissionTask("Process 10 Diamonds",10)
                {
                    Condition = new TaskCondition
                    {
                        MissionType = MissionType.DeviceAction,
                        TechType = Mod.OreConsumerTechType,
                        DeviceAction = DeviceAction.Consume,
                        TargetTechType = TechType.Diamond   
                    },
                    Description = "Process 10 Diamonds in the Ore Consumer",
                },
                new MissionTask("Process 10 Gold",10)
                {
                    Condition = new TaskCondition
                    {
                        MissionType = MissionType.DeviceAction,
                        TechType = Mod.OreConsumerTechType,
                        DeviceAction = DeviceAction.Consume,
                        TargetTechType = TechType.Gold
                    },
                    Description = "Process 10 Gold in the Ore Consumer",
                },
            }, "AlterraStorage_Kit".ToTechType(),500000);
            QuickLogger.Debug("Adding Missions");
            Player_Update_Patch.FCSPDA.MessagesController.AddNewMessage("Message From: Jack Winton (Chief Engineer)", "Jack Winton", "AH-Mission01-Pt1");
            Mod.GamePlaySettings.PlayStarterMission = false;
        }

        public Mission AddMission(string missionTitle, string missionDescription, List<MissionTask> tasks,TechType techReward = TechType.None, decimal creditReward = 0)
        {
            var mission = new Mission
            {
                Name = missionTitle,
                Description = missionDescription,
                Tasks = tasks
            };

            mission.Load();

            mission.TechTypeReward = techReward;
            mission.CreditReward = creditReward;

            //TODO Deal with mission complete

            Missions.Add(mission);

            return mission;
        }

        public void NotifyDeviceAction(TechType deviceTechType, TechType techType, DeviceAction deviceAction)
        {
            foreach (var mission in Missions)
            {
                if(mission.IsComplete) continue;
                mission.CheckDeviceActionTasks(deviceTechType, techType, deviceAction);
            }
        }
    }

    public enum DeviceAction
    {
        Consume,
        Used,
        Destroy,
        TurnOff,
        TurnOn,
        Rename,
    }
}
