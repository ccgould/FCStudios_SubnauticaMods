using System;
using System.Collections.Generic;
using FCS_AlterraHub.Configuration;
using FCSCommon.Utilities;
using UnityEngine;

namespace FCS_AlterraHub.Managers.Quests
{
    public class QuestManager : MonoBehaviour
    {
        public Quest quest = new Quest();
        private QuestEvent final;
        private List<QuestAction> _questActions = new List<QuestAction>();

        private void Start()
        {
            //Create each event
            //QuestEvent scanOreConsumer = quest.AddQuestEvent("Scan Ore Consumer", "Alterra needs you to scan the ore consumer located in the Aurora. Make sure you have a scanner and any other tool you need to gain access", Mod.OreConsumerTechType,QuestEvent.EventType.SCAN);
            QuestEvent buildAlterraHub = quest.AddQuestEvent("Build AlterraHub", "Alterra needs you to build an Alterra Hub so you can make an account so we can begin testing the account system.",Mod.AlterraHubTechType,1,QuestEvent.EventType.BUILD);
            QuestEvent buildOreConsumers = quest.AddQuestEvent("Build Ore Consumer", "Ok, lastly we now need you to build 2 Ore Consumers and start a ore process by adding ores to the machine so you can now generate cash.", Mod.OreConsumerTechType, 2, QuestEvent.EventType.BUILD);


            //Define the paths between the events - e.g the order they must be completed
            //quest.AddPath(scanOreConsumer.GetID, buildAlterraHub.GetID);
            quest.AddPath(buildAlterraHub.GetID,buildOreConsumers.GetID);

            //Create tree
            quest.BreadthFirstSearch(buildAlterraHub.GetID);

            var buildAlterraHubAction = new QuestAction();
            buildAlterraHubAction.Setup(this, buildAlterraHub);

            var buildOreConsumersAction = new QuestAction();
            buildOreConsumersAction.Setup(this, buildOreConsumers);

            _questActions.Add(buildAlterraHubAction);
            _questActions.Add(buildOreConsumersAction);

            final = buildOreConsumers;

            var button = CreateButton(buildAlterraHub).GetComponent<QuestButton>();


            quest.PrintPath();
        }

        public void UpdateQuestOnCompletion(QuestEvent questEvent)
        {
            if (questEvent == final)
            {
                //Perform complete
                QuickLogger.Debug($"Quest {questEvent.GetName} is complete",true);
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
            QuickLogger.Debug($"Notifying TechType Constructed: {Language.main.Get(techType)}",true);
            foreach (QuestAction action in _questActions)
            {
                action.CheckBuildAction(techType);
            }
        }

        public void NotifyItemScanned(TechType techType)
        {
            foreach (QuestAction action in _questActions)
            {
                action.CheckCollectionAction(techType);
            }
        }
    }
}
