using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FCS_AlterraHub.Configuration;
using FCS_AlterraHub.Helpers;
using FCS_AlterraHub.Managers.Quests.Enums;
using FCS_AlterraHub.Managers.Quests.Missions;
using FCS_AlterraHub.Mono.FCSPDA.Mono;
using FCS_AlterraHub.Patches;
using FCS_AlterraHub.Systems;
using FCSCommon.Extensions;
using FCSCommon.Utilities;
using UnityEngine;

namespace FCS_AlterraHub.Managers.Quests
{
    public class QuestManager : MonoBehaviour
    {
        public static QuestManager Instance;

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

        private FCSGamePlaySettings settings => Mod.GamePlaySettings;
        public Quest quest;
        private QuestEvent _finalEvent;
        internal Action<Quest> OnMissionAdded;
        private static bool _audioLoaded;

        private void Update()
        {
            if(Mod.GamePlaySettings == null) return;

            if (DayNightCycle.main.timePassedAsFloat > 600f && Mod.GamePlaySettings.PlayStarterMission)
            {
                //Change to add message
                //CreateStarterMission();
                //StartFirstMission();
                //Mod.GamePlaySettings.PlayStarterMission = false;
                //Mod.SaveGamePlaySettings();
            }
        }

        private void StartFirstMission()
        {
            if (quest != null && quest.QuestEvents.Count > 0)
            {
                if (quest.QuestEvents[0].Status == QuestEventStatus.WAITING)
                    quest.QuestEvents[0].SetStatus(QuestEventStatus.CURRENT);
            }
        }

        public void CreateStarterMission()
        {
            if(Mod.GamePlaySettings.PlayStarterMission)
            {
                quest = new StarterMission { CreditReward = 500000, TechTypeReward = "AlterraStorage_kit".ToTechType() };
                _finalEvent = quest.Create();
                StartFirstMission();
                Mod.GamePlaySettings.PlayStarterMission = false;
                Mod.SaveGamePlaySettings();
                uGUI_PowerIndicator_Initialize_Patch.MissionHUD.ShowMessage("ALTERRA MISSION","Alterra Credit System Connection");
                OnMissionAdded?.Invoke(quest);
            }
        }

        public void UpdateQuestOnCompletion(QuestEvent questEvent)
        {
            if (questEvent == _finalEvent)
            {
                //Perform complete
                if (quest.CreditReward > 0)
                {
                    CardSystem.main.AddFinances(quest.CreditReward);
                }

                if (quest.TechTypeReward != TechType.None)
                {
                    PlayerInteractionHelper.GivePlayerItem(quest.TechTypeReward);
                }

                QuickLogger.Debug($"Quest {questEvent.GetName} is complete", true);
                return;
            }

            foreach (QuestEvent currentQuestEvent in quest.QuestEvents)
            {
                //if this event is the next order
                if (currentQuestEvent.Order == questEvent.Order + 1)
                {
                    //make the next in line avaliable for completion
                    currentQuestEvent.UpdateQuestEvent(QuestEventStatus.CURRENT);
                }
            }
        }

        public AudioClip FindAudioClip(string audioName)
        {
            if (!Mod.AudioClips.ContainsKey(audioName)) return null;
            QuickLogger.Debug($"Audio clip found: {audioName}", true);
            return Mod.AudioClips[audioName];
        }

        private GameObject CreateButton(QuestEvent e)
        {
            GameObject button = Instantiate(new GameObject());

            var questButton = button.AddComponent<QuestButton>();
            questButton.Setup(e, new GameObject());

            if (e.Order == 1)
            {
                questButton.UpdateButton(QuestEventStatus.CURRENT);
                e.SetStatus(QuestEventStatus.CURRENT);
            }

            return button;
        }

        public void NotifyTechTypeConstructed(TechType techType)
        {
            if (quest == null) return;
            QuickLogger.Debug($"Notifying TechType Constructed: {Language.main.Get(techType)}", true);
            foreach (QuestAction action in quest.GetQuestActions())
            {
                if (action.GetQuestEvent().Status != QuestEventStatus.CURRENT) continue;
                action.CheckBuildAction(techType);
            }
        }

        public void NotifyItemScanned(TechType techType)
        {
            if (quest == null) return;
            QuickLogger.Debug($"Notifying TechType Scanned: {Language.main.Get(techType)}", true);
            foreach (QuestAction action in quest.GetQuestActions())
            {
                if (action.GetQuestEvent().Status != QuestEventStatus.CURRENT) continue;
                action.CheckScanAction(techType);
            }
        }

        public void NotifyDeviceAction(TechType deviceTechType,TechType item, DeviceActionType deviceAction)
        {
            if (quest == null) return;
            QuickLogger.Debug($"Notifying Device Action: {Language.main.Get(deviceTechType)} | Item: {Language.main.Get(item)} | Device Action: {deviceAction}", true);
            foreach (QuestAction action in quest.GetQuestActions())
            {
                if (action.GetQuestEvent().Status != QuestEventStatus.CURRENT) continue;
                action.CheckDeviceAction(deviceTechType,item,deviceAction);
            }
        }

        public void CompleteCurrentMission()
        {
            if (quest == null || quest.QuestEvents.Count <= 0) return;

            foreach (QuestAction questAction in quest.GetQuestActions())
            {
                if (questAction.GetQuestEvent().Status != QuestEventStatus.CURRENT) continue;
                questAction.CompleteQuest();
                break;
            }
        }

        public IEnumerable<QuestEventData> SaveEvents()
        {
            foreach (QuestEvent questEvent in quest.QuestEvents)
            {
                yield return new QuestEventData
                {
                    Amount = questEvent.Amount,
                    CurrentAmount = questEvent.CurrentAmount,
                    GetID = questEvent.GetID,
                    GetName = questEvent.GetName,
                    GetDescription = questEvent.GetDescription,
                    Order = questEvent.Order,
                    TechType = questEvent.TechType,
                    QuestEventType = questEvent.QuestEventType,
                    Status = questEvent.Status,
                    DeviceActionType = questEvent.DeviceActionType,
                    Requirements = questEvent.Requirements,
                    PathList = GetPathList(questEvent.PathList)
                };
            }
        }

        private IEnumerable<EventPathData> GetPathList(List<QuestPath> questEventPathList)
        {
            foreach (QuestPath path in questEventPathList)
            {
                yield return new EventPathData
                {
                    From = path.StartEvent.GetID,
                    To = path.EndEvent.GetID
                };
            }
        }

        internal Quest GetActiveMission()
        {
            return quest;
        }

        public void Load()
        {
            QuickLogger.Debug($"Loading Saved Mission",true);
            if (Mod.GamePlaySettings?.Event != null)
            {

                quest = new Quest
                {
                    CreditReward = Mod.GamePlaySettings.CreditReward,
                    TechTypeReward = Mod.GamePlaySettings.TechTypeReward
                };

                foreach (QuestEventData data in Mod.GamePlaySettings.Event)
                {
                    QuickLogger.Debug($"Adding Quest event: {data.GetName}", true);
                    var item = quest.AddQuestEvent(data.GetName, data.GetDescription, data.TechType, data.DeviceActionType, data.Requirements, data.QuestEventType, data.GetID);
                    item.Order = data.Order;
                    item.Status = data.Status;
                    item.Amount = data.Amount;
                    item.CurrentAmount = data.CurrentAmount;
                    QuickLogger.Debug($"Filling Quest event path: {data.PathList?.Count()}", true);

                    if (data.PathList != null)
                    {
                        foreach (EventPathData eventPathData in data.PathList)
                        {
                            quest.AddPath(eventPathData.From, eventPathData.To);
                        }
                    }
                    var action = new QuestAction();
                    action.Setup(Instance, item);
                    quest.Description = Mod.GamePlaySettings.MissionDescription;
                    quest.QuestActions.Add(action);

                    quest.BreadthFirstSearch(Mod.GamePlaySettings.Event[0].GetID);

                    OnMissionAdded?.Invoke(quest);
                }

                QuickLogger.Debug($"Quest: {quest}",true);
            }
        }

        public int GetMissionCount()
        {
            return quest != null && !quest.IsComplete()? 1: 0;
        }
    }
}