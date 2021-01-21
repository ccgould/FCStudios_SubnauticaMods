using System.Collections.Generic;
using UnityEngine;

namespace FCS_AlterraHub.Managers.Quests.Missions
{
    internal class StarterMission : QuestEvent
    {
        public StarterMission(string name, string description, GameObject location, EventType eventType) : base(name, description, location, eventType)
        {
        }

        public StarterMission(string name, string description, TechType techType, EventType eventType) : base(name, description, techType, eventType)
        {
        }

        public StarterMission(string name, string description, TechType techType, int amount, EventType eventType) : base(name, description, techType, amount, eventType)
        {
        }

        public StarterMission(string name, string description, TechType techType, GameObject location, EventType eventType) : base(name, description, techType, location, eventType)
        {
        }

        public StarterMission(string name, string description, TechType techType, DeviceActionType actionType, Dictionary<TechType, int> requirements, EventType eventType) : base(name, description, techType, actionType, requirements, eventType)
        {
        }
    }
}
