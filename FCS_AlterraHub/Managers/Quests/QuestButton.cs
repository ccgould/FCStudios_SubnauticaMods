﻿using System.Collections.Generic;
using FCS_AlterraHub.Managers.Quests.Enums;
using UnityEngine;
using UnityEngine.UI;

namespace FCS_AlterraHub.Managers.Quests
{
    public class QuestButton : MonoBehaviour
    {
        private Button _button;
        private QuestEvent _thisEvent;
        private QuestEventStatus _status;
        private Text _eventName;
        public void Setup(QuestEvent e, GameObject scrollList)
        {
            _thisEvent = e;
            _button.transform.SetParent(scrollList.transform,false);
            _eventName.text = $"<b>{_thisEvent.GetName}</b>\n{_thisEvent.GetDescription}";
            _status = _thisEvent.Status;

        }

        public void UpdateButton(QuestEventStatus status)
        {
            _status = status;

            switch (status)
            {
                case QuestEventStatus.DONE:
                case QuestEventStatus.WAITING:
                    _button.interactable = false;
                    break;
                case QuestEventStatus.CURRENT:
                    _button.interactable = true;
                    break;
            }
        }
    }
}