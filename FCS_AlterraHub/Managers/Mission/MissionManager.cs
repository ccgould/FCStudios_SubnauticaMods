using System;
using System.Collections.Generic;
using FCS_AlterraHub.Configuration;
using UnityEngine;

namespace FCS_AlterraHub.Managers.Mission
{
    public class MissionManager : MonoBehaviour
    {
        public List<Mission> Missions { get; set; } = new List<Mission>();

        public static MissionManager Instance;

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

        }

        public int GetMissionCount()
        {
            return Missions.Count;
        }

        private void Update()
        {
            foreach (Mission mission in Missions)
            {
                if(mission.IsComplete) continue;
                mission.CheckSelfValidateTasks();
            }
        }

        public void CompleteCurrentMission()
        {
            
        }

        private void CreateStarterMission()
        {
            AddMission("Alterra Credit System Test","Help Alterra test the connection between you and the system at Alterra Corp",new List<MissionTask>
            {
                new MissionTask(Guid.NewGuid().ToString(),1)
                {
                    Condition = new TaskCondition
                    {
                        MissionType = MissionType.Scan,
                        TechType = Mod.OreConsumerTechType,
                    },
                    Description = "Scan the Ore Consumer in the Jelly Caves biome by the abandoned Degasi Base.",
                    SelfValidate = true
                },
                new MissionTask(Guid.NewGuid().ToString(),1)
                {
                    Condition = new TaskCondition
                    {
                        MissionType = MissionType.Construct,
                        TechType = Mod.AlterraHubTechType
                    },
                    Description = "Build an AlterraHub at your base.",
                    SelfValidate = true
                },
                new MissionTask(Guid.NewGuid().ToString(),1)
                {
                    Condition = new TaskCondition
                    {
                        MissionType = MissionType.AccountCreation,
                    },
                    Description = "Create an Alterra Account",
                    SelfValidate = true
                },
                new MissionTask(Guid.NewGuid().ToString(),2)
                {
                    Condition = new TaskCondition
                    {
                        MissionType = MissionType.Construct,
                        TechType = Mod.OreConsumerTechType
                    },
                    Description = "Build two ore consumers",
                    SelfValidate = true
                },
                new MissionTask(Guid.NewGuid().ToString(),10)
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
                new MissionTask(Guid.NewGuid().ToString(),10)
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
            });
        }

        public void AddMission(string missionTitle, string missionDescription, List<MissionTask> tasks)
        {
            var mission = new Mission
            {
                Name = missionTitle,
                Description = missionDescription,
                Tasks = tasks
            };

            mission.Load();

            //TODO Deal with mission complete

            Missions.Add(mission);
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
