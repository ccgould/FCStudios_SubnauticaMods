using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace FCS_AlterraHub.Managers.Quests
{
    public class QuestButton : MonoBehaviour
    {
        private Button _button;
        private QuestEvent _thisEvent;
        private QuestEvent.EventStatus _status;
        private Text _eventName;
        public void Setup(QuestEvent e, GameObject scrollList)
        {
            _thisEvent = e;
            _button.transform.SetParent(scrollList.transform,false);
            _eventName.text = $"<b>{_thisEvent.GetName}</b>\n{_thisEvent.GetDescription}";
            _status = _thisEvent.GetStatus;

        }

        public void UpdateButton(QuestEvent.EventStatus status)
        {
            _status = status;

            switch (status)
            {
                case QuestEvent.EventStatus.DONE:
                case QuestEvent.EventStatus.WAITING:
                    _button.interactable = false;
                    break;
                case QuestEvent.EventStatus.CURRENT:
                    _button.interactable = true;
                    break;
            }
        }
    }
}
