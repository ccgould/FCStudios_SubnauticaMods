using System.Collections.Generic;
using FCS_AlterraHub.Configuration;
using FCS_AlterraHub.Helpers;
using FCS_AlterraHub.Systems;
using FCSCommon.Extensions;
using FCSCommon.Utilities;
using UnityEngine;

namespace FCS_AlterraHub.Managers.Quests
{
    public class QuestManager : MonoBehaviour
    {
        public Quest quest = new Quest();
        private QuestEvent final;
        private List<QuestAction> _questActions = new List<QuestAction>();
        private bool _createStarterMission = true;

        private void Update()
        {
            if (_createStarterMission)
            {
                if (DayNightCycle.main.timePassedAsFloat > 600f)
                {
                    StartFirstMission();
                    _createStarterMission = false;
                }
            }
        }

        private void StartFirstMission()
        {
            if (quest != null && quest.QuestEvents.Count > 0)
            {
                if (quest.QuestEvents[0].GetStatus == QuestEvent.EventStatus.WAITING)
                    quest.QuestEvents[0].SetStatus(QuestEvent.EventStatus.CURRENT);
            }

        }

        private void Start()
        {
            if (_createStarterMission)
            {
                CreateStarterMission();
            }
        }

        private void CreateStarterMission()
        {


            quest.AddQuestEvent()


            quest.CreditReward = 500000;
            quest.TechTypeReward = "AlterraStorage_kit".ToTechType();

            //Create each event
            QuestEvent scanOreConsumer = quest.AddQuestEvent("Scan Ore Consumer",
                "Alterra needs you to scan the ore consumer located in the Aurora. Make sure you have a scanner and any other tool you need to gain access",
                Mod.OreConsumerFragmentTechType, QuestEvent.EventType.SCAN);

            QuestEvent buildAlterraHub = quest.AddQuestEvent("Build AlterraHub",
                "Alterra needs you to build an Alterra Hub so you can make an account so we can begin testing the account system.",
                Mod.AlterraHubTechType, 1, QuestEvent.EventType.BUILD);

            QuestEvent createAccount = quest.AddQuestEvent("Create an Alterra Account",
                "Alterra needs you to make an account so we can begin testing the account system.",
                Mod.AlterraHubTechType, QuestEvent.DeviceActionType.CREATEITEM, new Dictionary<TechType, int> { { Mod.DebitCardTechType, 1 } }, QuestEvent.EventType.DEVICEACTION);

            QuestEvent buildOreConsumers = quest.AddQuestEvent("Build Ore Consumer",
                "Ok, lastly we now need you to build 2 Ore Consumers and start a ore process by adding ores to the machine so you can now generate cash.",
                Mod.OreConsumerTechType, 2, QuestEvent.EventType.BUILD);

            QuestEvent processOres = quest.AddQuestEvent("Process Ores in Ore Consumer",
                "Alterra needs you to process 10 diamonds and 10 gold",
                Mod.OreConsumerTechType, QuestEvent.DeviceActionType.PROCESSITEM, 
                new Dictionary<TechType, int>
                {
                    {TechType.Diamond, 10},
                    { TechType.Gold, 10}
                }, 
                QuestEvent.EventType.DEVICEACTION);


            //Define the paths between the events - e.g the order they must be completed
            quest.AddPath(scanOreConsumer.GetID, buildAlterraHub.GetID);
            quest.AddPath(buildAlterraHub.GetID, createAccount.GetID);
            quest.AddPath(createAccount.GetID, buildOreConsumers.GetID);
            quest.AddPath(buildOreConsumers.GetID, processOres.GetID);

            //Create tree
            quest.BreadthFirstSearch(scanOreConsumer.GetID);

            var scanOreConsumerAction = new QuestAction();
            scanOreConsumerAction.Setup(this, scanOreConsumer);

            var buildAlterraHubAction = new QuestAction();
            buildAlterraHubAction.Setup(this, buildAlterraHub);

            var createAccountAction = new QuestAction();
            createAccountAction.Setup(this, createAccount);

            var buildOreConsumersAction = new QuestAction();
            buildOreConsumersAction.Setup(this, buildOreConsumers);

            var processOresAction = new QuestAction();
            processOresAction.Setup(this, processOres);

            _questActions.Add(scanOreConsumerAction);
            _questActions.Add(buildAlterraHubAction);
            _questActions.Add(createAccountAction);
            _questActions.Add(buildOreConsumersAction);
            _questActions.Add(processOresAction);

            final = processOres;

            //var button = CreateButton(buildAlterraHub).GetComponent<QuestButton>();
            quest.PrintPath();
        }

        public void UpdateQuestOnCompletion(QuestEvent questEvent)
        {
            if (questEvent == final)
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
                    currentQuestEvent.UpdateQuestEvent(QuestEvent.EventStatus.CURRENT);
                }
            }
        }

        private GameObject CreateButton(QuestEvent e)
        {
            GameObject button = Instantiate(new GameObject());

            var questButton = button.AddComponent<QuestButton>();
            questButton.Setup(e, new GameObject());

            if (e.Order == 1)
            {
                questButton.UpdateButton(QuestEvent.EventStatus.CURRENT);
                e.SetStatus(QuestEvent.EventStatus.CURRENT);
            }

            return button;
        }

        public void NotifyTechTypeConstructed(TechType techType)
        {
            QuickLogger.Debug($"Notifying TechType Constructed: {Language.main.Get(techType)}", true);
            foreach (QuestAction action in _questActions)
            {
                if (action.GetQuestEvent().GetStatus != QuestEvent.EventStatus.CURRENT) continue;
                action.CheckBuildAction(techType);
            }
        }

        public void NotifyItemScanned(TechType techType)
        {
            QuickLogger.Debug($"Notifying TechType Scanned: {Language.main.Get(techType)}", true);
            foreach (QuestAction action in _questActions)
            {
                if (action.GetQuestEvent().GetStatus != QuestEvent.EventStatus.CURRENT) continue;
                action.CheckScanAction(techType);
            }
        }

        public void NotifyDeviceAction(TechType deviceTechType,TechType item, QuestEvent.DeviceActionType deviceAction)
        {
            QuickLogger.Debug($"Notifying Device Action: {Language.main.Get(deviceTechType)} | Item: {Language.main.Get(item)} | Device Action: {deviceAction}", true);
            foreach (QuestAction action in _questActions)
            {
                if (action.GetQuestEvent().GetStatus != QuestEvent.EventStatus.CURRENT) continue;
                action.CheckDeviceAction(deviceTechType,item,deviceAction);
            }
        }

        public void CompleteCurrentMission()
        {
            if (quest == null || quest.QuestEvents.Count <= 0) return;

            foreach (QuestAction questAction in _questActions)
            {
                if (questAction.GetQuestEvent().GetStatus != QuestEvent.EventStatus.CURRENT) continue;
                questAction.CompleteQuest();
                break;
            }
        }
    }
}
