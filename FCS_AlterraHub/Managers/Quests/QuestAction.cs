using System.Collections.Generic;
using System.IO;
using System.Linq;
using FCS_AlterraHub.Managers.Quests.Enums;
using FCS_AlterraHub.Patches;
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
            if (techType != _questEvent.TechType) return;
            
            //If we shouldn't be working on this event
            // then don't register it as completed
            if (_questEvent.Status != QuestEventStatus.CURRENT || _questEvent.QuestEventType == QuestEventType.BUILD) return;

            if (_questEvent.Requirements.ContainsKey(techType))
            {
                _questEvent.Requirements[techType] -= 1;
            }

            if (!_questEvent.RequirementsMet) return;

            //inject these back  into the Quest Manager to Update the States
            CompleteQuest();
        }

        internal void CheckBuildAction(TechType techType)
        {
            if (techType != _questEvent.TechType || _questEvent.QuestEventType != QuestEventType.BUILD) return;
            QuickLogger.Debug($"[CheckBuildAction] {_questEvent.GetName}: TechType {Language.main.Get(techType)} | Event Type: {_questEvent.QuestEventType} | Status: {_questEvent.Status}", true);
            //If we shouldn't be working on this event
            // then don't register it as completed
            if (_questEvent.Status != QuestEventStatus.CURRENT) return;

            if (_questEvent.Requirements.ContainsKey(techType))
            {
                _questEvent.Requirements[techType] -= 1;
            }

            if (!_questEvent.RequirementsMet) return;

            //inject these back  into the Quest Manager to Update the States
            CompleteQuest();
        }

        internal void CheckScanAction(TechType techType)
        {

            QuickLogger.Debug($"[CheckScanAction] {_questEvent.GetName}: TechType {Language.main.Get(techType)} | Event Type: {_questEvent.QuestEventType}", true);
            if (techType != _questEvent.TechType || _questEvent.QuestEventType != QuestEventType.SCAN) return;

            QuickLogger.Debug($"[CheckScanAction] Status: {_questEvent.Status}", true);

            //If we shouldn't be working on this event
            // then don't register it as completed
            if (_questEvent.Status != QuestEventStatus.CURRENT) return;

            if (_questEvent.Requirements.ContainsKey(techType))
            {
                _questEvent.Requirements[techType] -= 1;
            }

            if (!_questEvent.RequirementsMet) return;

            //inject these back  into the Quest Manager to Update the States
            CompleteQuest();
        }

        public void Setup(QuestManager questManager, QuestEvent questEvent)
        {
            _questManager = questManager;
            _questEvent = questEvent;
        }

        public void CheckDeviceAction(TechType deviceTechType, TechType item, DeviceActionType deviceAction)
        {
            QuickLogger.Debug($"[CheckDeviceAction] {_questEvent.GetName}: TechType {Language.main.Get(deviceTechType)} | Event Type: {_questEvent.QuestEventType}", true);
            if (deviceTechType != _questEvent.TechType || _questEvent.QuestEventType != QuestEventType.DEVICEACTION || _questEvent.DeviceActionType != deviceAction) return;

            QuickLogger.Debug($"[CheckDeviceAction] Status: {_questEvent.Status} | Device Action: {deviceAction}", true);
            
            //If we shouldn't be working on this event
            // then don't register it as completed

            if(_questEvent.Status != QuestEventStatus.CURRENT || _questEvent.Requirements == null) return;

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
            _questEvent.UpdateQuestEvent(QuestEventStatus.DONE);
            _questManager.UpdateQuestOnCompletion(_questEvent);

            var keys = _questEvent.Requirements.Keys.ToList();
            foreach (TechType key in keys)
            {
                _questEvent.Requirements[key] = 0;
            }

            if (!string.IsNullOrEmpty(_questEvent.MissionAudioTrackPath))
            {
                var clipName = Path.GetFileNameWithoutExtension(_questEvent.MissionAudioTrackPath);
                Player_Update_Patch.FCSPDA.MessagesController.PlayAudioTrack(clipName);
                Player_Update_Patch.FCSPDA.MessagesController.AddNewMessage($"New Message from {_questEvent.MissionContactName} - Mission: {_questEvent.GetName}", _questEvent.MissionContactName, clipName,true);
            }
        }

        public QuestEvent GetQuestEvent()
        {
            return _questEvent;
        }
    }
}
