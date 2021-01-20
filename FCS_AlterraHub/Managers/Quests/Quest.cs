using System.Collections.Generic;
using FCSCommon.Utilities;
using UnityEngine;

namespace FCS_AlterraHub.Managers.Quests
{
    public class Quest
    {
        public List<QuestEvent> QuestEvents  = new List<QuestEvent>();
        
        public QuestEvent AddQuestEvent(string name, string description, GameObject location, QuestEvent.EventType eventType)
        {
            QuestEvent questEvent = new QuestEvent(name,description,location,eventType);
            QuestEvents.Add(questEvent);
            return questEvent;
        }

        public QuestEvent AddQuestEvent(string name, string description, TechType techType, QuestEvent.EventType eventType)
        {
            QuestEvent questEvent = new QuestEvent(name, description, techType,eventType);
            QuestEvents.Add(questEvent);
            return questEvent;
        }

        public QuestEvent AddQuestEvent(string name, string description, TechType techType,int amount, QuestEvent.EventType eventType)
        {
            QuestEvent questEvent = new QuestEvent(name, description, techType,amount, eventType);
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
    }
}
