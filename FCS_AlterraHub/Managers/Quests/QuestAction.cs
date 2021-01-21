using FCSCommon.Utilities;
using UnityEngine;

namespace FCS_AlterraHub.Managers.Quests
{
    public class QuestAction
    {
        /*
         * This code goes on a gameobject that represents the task to be performed by the player at the location
         * of the object, This code can contain any logic as long as when the task is complete it injects the three statuses
         * back into the Quest System (as per in the OnCollisionEnter) currently here
         */
        private QuestManager _questManager;
        private QuestEvent _questEvent;
        
        internal void CheckCollectionAction(TechType techType, int amount = 1)
        {
            if (techType != _questEvent.GetTechType) return;
            
            //If we shouldn't be working on this event
            // then don't register it as completed
            if (_questEvent.GetStatus != QuestEvent.EventStatus.CURRENT || _questEvent.GetEventType == QuestEvent.EventType.BUILD) return;

            _questEvent.AddAmount(amount);

            if(!_questEvent.AmountMet) return;
            
            //inject these back  into the Quest Manager to Update the States
            _questEvent.UpdateQuestEvent(QuestEvent.EventStatus.DONE);
            _questManager.UpdateQuestOnCompletion(_questEvent);
        }

        internal void CheckBuildAction(TechType techType)
        {
            if (techType != _questEvent.GetTechType || _questEvent.GetEventType != QuestEvent.EventType.BUILD) return;
            QuickLogger.Debug($"[CheckBuildAction] {_questEvent.GetName}: TechType {Language.main.Get(techType)} | Event Type: {_questEvent.GetEventType} | Status: {_questEvent.GetStatus}", true);
            //If we shouldn't be working on this event
            // then don't register it as completed
            if (_questEvent.GetStatus != QuestEvent.EventStatus.CURRENT) return;

            _questEvent.AddAmount(1);

            if (!_questEvent.AmountMet) return;

            //inject these back  into the Quest Manager to Update the States
            _questEvent.UpdateQuestEvent(QuestEvent.EventStatus.DONE);
            _questManager.UpdateQuestOnCompletion(_questEvent);
        }

        internal void CheckScanAction(TechType techType)
        {

            QuickLogger.Debug($"[CheckScanAction] {_questEvent.GetName}: TechType {Language.main.Get(techType)} | Event Type: {_questEvent.GetEventType}", true);
            if (techType != _questEvent.GetTechType || _questEvent.GetEventType != QuestEvent.EventType.SCAN) return;

            QuickLogger.Debug($"[CheckScanAction] Status: {_questEvent.GetStatus}", true);

            //If we shouldn't be working on this event
            // then don't register it as completed
            if (_questEvent.GetStatus != QuestEvent.EventStatus.CURRENT) return;

            //inject these back  into the Quest Manager to Update the States
            _questEvent.UpdateQuestEvent(QuestEvent.EventStatus.DONE);
            _questManager.UpdateQuestOnCompletion(_questEvent);
        }

        public void Setup(QuestManager questManager, QuestEvent questEvent)
        {
            _questManager = questManager;
            _questEvent = questEvent;
        }

        public void CheckDeviceAction(TechType deviceTechType, TechType item, QuestEvent.DeviceActionType deviceAction)
        {
            QuickLogger.Debug($"[CheckDeviceAction] {_questEvent.GetName}: TechType {Language.main.Get(deviceTechType)} | Event Type: {_questEvent.GetEventType}", true);
            if (deviceTechType != _questEvent.GetTechType || _questEvent.GetEventType != QuestEvent.EventType.DEVICEACTION || _questEvent.GetDeviceActionType != deviceAction) return;

            QuickLogger.Debug($"[CheckDeviceAction] Status: {_questEvent.GetStatus} | Device Action: {deviceAction}", true);


            

            //If we shouldn't be working on this event
            // then don't register it as completed

            if(_questEvent.GetStatus != QuestEvent.EventStatus.CURRENT || _questEvent.Requirements == null) return;

            if (_questEvent.Requirements.ContainsKey(item))
            {
                _questEvent.Requirements[item] -= 1;
            }
            
            if (!_questEvent.RequirementsMet) return;

            CompleteQuest();
        }

        public void CompleteQuest()
        {
            //inject these back  into the Quest Manager to Update the States
            _questEvent.UpdateQuestEvent(QuestEvent.EventStatus.DONE);
            _questManager.UpdateQuestOnCompletion(_questEvent);
        }

        public QuestEvent GetQuestEvent()
        {
            return _questEvent;
        }
    }
}
