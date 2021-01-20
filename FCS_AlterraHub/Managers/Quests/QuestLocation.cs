using UnityEngine;

namespace FCS_AlterraHub.Managers.Quests
{
    public class QuestLocation : MonoBehaviour
    {
        /*
         * This code goes on a gameobject that represents the task to be performed by the player at the location
         * of the object, This code can contain any logic as long as when the task is complete it injects the three statuses
         * back into the Quest System (as per in the OnCollisionEnter) currently here
         */
        private QuestManager _questManager;
        private QuestEvent _questEvent;

        private void OnCollisionEnter(Collision collision)
        {
            if (collision.gameObject.tag != "Player") return;

            //If we shouldn't be working on this event
            // then don't register it as completed
            if(_questEvent.GetStatus !=  QuestEvent.EventStatus.CURRENT) return;

            //inject these back  into the Quest Manager to Update the States
            _questEvent.UpdateQuestEvent(QuestEvent.EventStatus.DONE);
            _questManager.UpdateQuestOnCompletion(_questEvent);
        }

        public void Setup(QuestManager questManager, QuestEvent questEvent)
        {
            _questManager = questManager;
            _questEvent = questEvent;
        }
    }
}
