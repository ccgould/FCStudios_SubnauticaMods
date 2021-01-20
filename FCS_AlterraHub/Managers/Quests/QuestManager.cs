using System;
using System.Collections.Generic;
using UnityEngine;

namespace FCS_AlterraHub.Managers.Quests
{
    public class QuestManager : MonoBehaviour
    {
        public Quest quest = new Quest();
        private List<GameObject> _locations = new List<GameObject>
        {
            new GameObject("Test"),
            new GameObject("Test1"),
            new GameObject("Test2"),
            new GameObject("Test3"),
            new GameObject("Test4"),
            new GameObject("Test5"),
        };
        private QuestEvent final;

        private void Start()
        {
            //Create each event
            QuestEvent a = quest.AddQuestEvent("test1", "description 1",_locations[0]);
            QuestEvent b = quest.AddQuestEvent("test2", "description 2",_locations[1]);
            QuestEvent c = quest.AddQuestEvent("test3", "description 3",_locations[2]);
            QuestEvent d = quest.AddQuestEvent("test4", "description 4",_locations[3]);
            QuestEvent e = quest.AddQuestEvent("test5", "description 5",_locations[4]);

            //Define the paths between the events - e.g the order they must be completed
            quest.AddPath(a.GetID(),b.GetID());
            quest.AddPath(b.GetID(),c.GetID());
            quest.AddPath(b.GetID(),d.GetID());
            quest.AddPath(c.GetID(),e.GetID());
            quest.AddPath(d.GetID(),e.GetID());

            quest.BreadthFirstSearch(a.GetID());

            _locations[0].GetComponent<QuestLocation>().Setup(this,a);

            final = e;

            quest.PrintPath();
        }

        public void UpdateQuestOnCompletion(QuestEvent questEvent)
        {
            if (questEvent == final)
            {
                //Perform complete
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
    }
}
