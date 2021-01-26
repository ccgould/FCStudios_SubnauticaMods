using System;
using System.Collections.Generic;
using FCS_AlterraHub.Managers.Quests.Enums;
using FCSCommon.Utilities;
using Oculus.Newtonsoft.Json;
using UnityEngine;

namespace FCS_AlterraHub.Managers.Quests
{
    public class QuestEvent
    {
        public int Amount { get; set; }
        public int CurrentAmount { get; set; }
        public string GetID { get; set; }
        public string GetName { get; set; }
        public string GetDescription { get; set; }
        public int Order = -1;
        public TechType TechType { get; set; }
        public QuestEventType QuestEventType { get; set; }
        public QuestEventStatus Status { get; set; }
        public DeviceActionType DeviceActionType { get; set; }
        public Dictionary<TechType, int> Requirements { get; set; }
        [JsonIgnore] public GameObject Location { get; set; }

        [JsonIgnore] public List<QuestPath> PathList = new List<QuestPath>();
        [JsonIgnore] public bool RequirementsMet => IsRequirementsMet();

        private bool IsRequirementsMet()
        {
            foreach (KeyValuePair<TechType, int> pair in Requirements)
            {
                if (pair.Value > 0)
                {
                    return false;
                }
            }
            return true;
        }

        public QuestEvent(string name, string description, GameObject location,QuestEventType questEventType)
        {
            GetID = Guid.NewGuid().ToString();
            GetName = name;
            GetDescription = description;
            Location = location;
            QuestEventType = questEventType;
            Status = QuestEventStatus.WAITING;
        }

        public QuestEvent(string name, string description, TechType techType, QuestEventType questEventType)
        {
            GetID = Guid.NewGuid().ToString();
            GetName = name;
            GetDescription = description;
            TechType = techType;
            QuestEventType = questEventType;
            Status = QuestEventStatus.WAITING;
        }

        public QuestEvent(string name, string description, TechType techType,GameObject location, QuestEventType questEventType)
        {
            GetID = Guid.NewGuid().ToString();
            GetName = name;
            GetDescription = description;
            TechType = techType;
            Location = location;
            QuestEventType = questEventType;
            Status = QuestEventStatus.WAITING;
        }

        public QuestEvent(string name, string description, TechType techType, DeviceActionType actionType, Dictionary<TechType, int> requirements, QuestEventType questEventType, string id = null)
        {
            GetID = id ?? Guid.NewGuid().ToString();
            GetName = name;
            GetDescription = description;
            TechType = techType;
            QuestEventType = questEventType;
            Status = QuestEventStatus.WAITING;
            DeviceActionType = actionType;
            Requirements = requirements;
        }

        public void ActivateEvent()
        {
            SetStatus(QuestEventStatus.CURRENT);
        }

        public void UpdateQuestEvent(QuestEventStatus questEventStatus)
        {
            SetStatus(questEventStatus);
        }

        public void SetStatus(QuestEventStatus current)
        {
            Status = current;
            QuickLogger.Debug($"Setting Quest: {GetName} status to {current}",true);
        }
    }
}