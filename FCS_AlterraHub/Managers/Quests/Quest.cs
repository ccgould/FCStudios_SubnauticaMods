
using System;
using System.Collections.Generic;
using FCS_AlterraHub.Managers.Quests.Enums;
using FCSCommon.Utilities;
using UnityEngine;

namespace FCS_AlterraHub.Managers.Quests
{
    public class Quest
    {
        //ADD Description for the mission
        //public string MissionDescription { get; set; }
        public List<QuestEvent> QuestEvents { get; set; } = new List<QuestEvent>();
        public List<QuestAction> QuestActions { get; set; } = new List<QuestAction>();
        public TechType TechTypeReward { get; set; }
        public decimal CreditReward { get; set; }
        public string Description { get; set; }

        public QuestEvent AddQuestEvent(string name, string description, GameObject location, QuestEventType questEventType)
        {
            QuestEvent questEvent = new QuestEvent(name,description,location,questEventType);
            QuestEvents.Add(questEvent);
            return questEvent;
        }

        public QuestEvent AddQuestEvent(QuestEvent newEvent)
        {
            QuestEvents.Add(newEvent);
            return newEvent;
        }

        public QuestEvent AddQuestEvent(string name, string description, TechType techType, QuestEventType questEventType)
        {
            QuestEvent questEvent = new QuestEvent(name, description, techType,questEventType);
            QuestEvents.Add(questEvent);
            return questEvent;
        }

        internal QuestEvent AddQuestEvent(string name, string description, TechType techType,
            DeviceActionType actionType, Dictionary<TechType, int> requirements, QuestEventType questEventType,
            string id = null)
        {
            QuestEvent questEvent = new QuestEvent(name, description, techType, actionType, requirements,questEventType, id);
            QuestEvents.Add(questEvent);
            return questEvent;
        }

        public void AddPath(string fromQuestEvent, string toQuestEvent)
        {
            QuestEvent from = FindQuestEvent(fromQuestEvent);
            QuestEvent to = FindQuestEvent(toQuestEvent);

            if (from != null && to != null)
            {
                QuestPath path = new QuestPath(from,to);
                from.PathList.Add(path);
            }
        }

        private QuestEvent FindQuestEvent(string id)
        {
            foreach (QuestEvent questEvent in QuestEvents)
            {
                if (questEvent.GetID == id)
                    return questEvent;
            }

            return null;
        }

        public void BreadthFirstSearch(string id, int orderNumber = 1)
        {
            QuestEvent thisEvent = FindQuestEvent(id);
            thisEvent.Order = orderNumber;
            foreach (QuestPath questPath in thisEvent.PathList)
            {
                if(questPath.EndEvent.Order == -1)
                    BreadthFirstSearch(questPath.EndEvent.GetID,orderNumber + 1);
            }
        }

        public void PrintPath()
        {
            foreach (QuestEvent questEvent in QuestEvents)
            {
                QuickLogger.Debug($"Name: {questEvent.GetName} | Order: {questEvent.Order}");
            }
        }

        public virtual QuestEvent Create()
        {
            return null;
        }

        public virtual List<QuestAction> GetQuestActions()
        {
            return QuestActions;
        }

        public bool IsComplete()
        {
            foreach (QuestEvent questEvent in QuestEvents)
            {
                if (questEvent.Status != QuestEventStatus.DONE)
                {
                    return false;
                }
            }

            return true;
        }
    }
}
