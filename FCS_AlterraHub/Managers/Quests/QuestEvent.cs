using System;
using System.Collections.Generic;
using FCSCommon.Utilities;
using UnityEngine;

namespace FCS_AlterraHub.Managers.Quests
{
    public class QuestEvent
    {
        public enum EventStatus{WAITING,CURRENT,DONE};  
        public enum EventType{COLLECT,BUILD,SCAN,FIND};
        //WAITING - not yet completed but can't be worked on cause there's a prerequisite event
        //CURRENT - the one the player should be trying to achieve
        //DONE - has been achieved

        private TechType _techType;
        private int _amount;
        private EventType _eventType;
        private EventStatus _status;
        public string GetID { get; }
        public string GetName { get; }
        public string GetDescription { get; }
        public EventStatus GetStatus => _status;
        public int Order = -1;
        public GameObject Location { get; set; }
        public TechType GetTechType => _techType;

        public List<QuestPath> PathList = new List<QuestPath>();
        private int _currentAmount;
        public bool AmountMet => _currentAmount >= _amount;
        public EventType GetEventType => _eventType;

        public QuestEvent(string name, string description, GameObject location,EventType eventType)
        {
            GetID = Guid.NewGuid().ToString();
            GetName = name;
            GetDescription = description;
            Location = location;
            _eventType = eventType;
            _status = EventStatus.WAITING;
        }

        public QuestEvent(string name, string description, TechType techType, EventType eventType)
        {
            GetID = Guid.NewGuid().ToString();
            GetName = name;
            GetDescription = description;
            _techType = techType;
            _eventType = eventType;
            _status = EventStatus.WAITING;
        }

        public QuestEvent(string name, string description, TechType techType,int amount, EventType eventType)
        {
            GetID = Guid.NewGuid().ToString();
            GetName = name;
            GetDescription = description;
            _techType = techType;
            _amount = amount;
            _eventType = eventType;
            _status = EventStatus.WAITING;
        }

        public QuestEvent(string name, string description, TechType techType,GameObject location, EventType eventType)
        {
            GetID = Guid.NewGuid().ToString();
            GetName = name;
            GetDescription = description;
            _techType = techType;
            Location = location;
            _eventType = eventType;
            _status = EventStatus.WAITING;
        }

        public void ActivateEvent()
        {
            SetStatus(EventStatus.CURRENT);
        }

        public void UpdateQuestEvent(EventStatus eventStatus)
        {
            SetStatus(eventStatus);
        }

        public void SetStatus(EventStatus current)
        {
            _status = current;
            QuickLogger.Debug($"Setting Quest: {GetName} status to {current}",true);
        }

        public void AddAmount(int amount)
        {
            _currentAmount += amount;
            QuickLogger.Debug($"Added amount to Quest: {GetName} : New Value = {_currentAmount}",true);
        }
    }
}