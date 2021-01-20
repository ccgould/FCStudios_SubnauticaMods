using System;
using System.Collections.Generic;
using UnityEngine;

namespace FCS_AlterraHub.Managers.Quests
{
    public class QuestEvent
    {
        public enum EventStatus{WAITING,CURRENT,DONE};  
        //WAITING - not yet completed but can't be worked on cause there's a prerequisite event
        //CURRENT - the one the player should be trying to achieve
        //DONE - has been achieved

        private string _name;
        private string _description;
        private string _id;
        private EventStatus _status;
        public int Order = -1;
        public GameObject Location { get; set; }
        public List<QuestPath> PathList = new List<QuestPath>();

        public QuestEvent(string name, string description, GameObject location)
        {
            _id = Guid.NewGuid().ToString();
            _name = name;
            _description = description;
            Location = location;
            _status = EventStatus.WAITING;
        }

        public void UpdateQuestEvent(EventStatus eventStatus)
        {
            _status = eventStatus;
        }

        public string GetID()
        {
            return _id;
        }

        public string GetName()
        {
            return _name;
        }

        public EventStatus GetStatus()
        {
            return _status;
        }
    }
}
