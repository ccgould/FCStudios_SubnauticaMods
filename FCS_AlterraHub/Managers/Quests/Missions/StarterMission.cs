using System.Collections.Generic;
using System.IO;
using FCS_AlterraHub.Configuration;
using FCS_AlterraHub.Managers.Quests.Enums;

namespace FCS_AlterraHub.Managers.Quests.Missions
{
    internal class StarterMission : Quest
    {
        public override QuestEvent Create()
        {
            //Create each event
            QuestEvent scanOreConsumer = AddQuestEvent("Scan Ore Consumer",
                "Alterra needs you to scan the ore consumer located in the Jellyshroom caves. Make sure you have a scanner and any other tool you need to gain access",
                Mod.OreConsumerFragmentTechType, DeviceActionType.NONE, 
                new Dictionary<TechType, int>
                {
                    { Mod.OreConsumerFragmentTechType, 1 }
                }, 
                QuestEventType.SCAN);
            scanOreConsumer.MissionContactName = "Jack Winton";

            QuestEvent buildAlterraHub = AddQuestEvent("Build AlterraHub",
                "Alterra needs you to build an Alterra Hub so you can make an account so we can begin testing the account system.",
                Mod.AlterraHubTechType, DeviceActionType.NONE,
                new Dictionary<TechType,int>
                {
                    { Mod.AlterraHubTechType, 1}
                }, 
               QuestEventType.BUILD);
            buildAlterraHub.MissionContactName = "Jack Winton";

            QuestEvent createAccount = AddQuestEvent("Create an Alterra Account",
                "Alterra needs you to make an account so we can begin testing the account system.",
                Mod.AlterraHubTechType, DeviceActionType.CREATEITEM, new Dictionary<TechType, int> { { Mod.DebitCardTechType, 1 } }, QuestEventType.DEVICEACTION);
            createAccount.MissionContactName = "Jack Winton";
            createAccount.MissionAudioTrackPath = Path.Combine(Mod.GetAssetPath(), "Audio", "AH-Mission01-Pt2.wav");

            QuestEvent buildOreConsumers = AddQuestEvent("Build Ore Consumer",
                "Ok, lastly we now need you to build 2 Ore Consumers and start a ore process by adding ores to the machine so you can now generate cash.",
                Mod.OreConsumerTechType,DeviceActionType.NONE,
                new Dictionary<TechType, int>
                {
                    { Mod.OreConsumerTechType, 2 }
                }, 
                QuestEventType.BUILD);
            buildOreConsumers.MissionContactName = "Jack Winton";

            QuestEvent processOres = AddQuestEvent("Process Ores in Ore Consumer",
                "Alterra needs you to process 10 diamonds and 10 gold",
                Mod.OreConsumerTechType, DeviceActionType.PROCESSITEM,
                new Dictionary<TechType, int>
                {
                    {TechType.Diamond, 10},
                    { TechType.Gold, 10}
                },
                QuestEventType.DEVICEACTION);
            processOres.MissionContactName = "Jack Winton";
            processOres.MissionAudioTrackPath = Path.Combine(Mod.GetAssetPath(), "Audio", "AH-Mission01-Pt3.wav");


            //Define the paths between the events - e.g the order they must be completed
            AddPath(scanOreConsumer.GetID, buildAlterraHub.GetID);
            AddPath(buildAlterraHub.GetID, createAccount.GetID);
            AddPath(createAccount.GetID, buildOreConsumers.GetID);
            AddPath(buildOreConsumers.GetID, processOres.GetID);

            //Create tree
            BreadthFirstSearch(scanOreConsumer.GetID);

            var scanOreConsumerAction = new QuestAction();
            scanOreConsumerAction.Setup(QuestManager.Instance, scanOreConsumer);

            var buildAlterraHubAction = new QuestAction();
            buildAlterraHubAction.Setup(QuestManager.Instance, buildAlterraHub);

            var createAccountAction = new QuestAction();
            createAccountAction.Setup(QuestManager.Instance, createAccount);

            var buildOreConsumersAction = new QuestAction();
            buildOreConsumersAction.Setup(QuestManager.Instance, buildOreConsumers);

            var processOresAction = new QuestAction();
            processOresAction.Setup(QuestManager.Instance, processOres);

            QuestActions.Add(scanOreConsumerAction);
            QuestActions.Add(buildAlterraHubAction);
            QuestActions.Add(createAccountAction);
            QuestActions.Add(buildOreConsumersAction);
            QuestActions.Add(processOresAction);

            Description = "Alterra needs your help to test the credit system connection from you to Alterra Corp.";

            PrintPath();

            return processOres;
        }
    }
}
