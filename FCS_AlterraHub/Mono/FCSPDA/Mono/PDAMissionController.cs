using FCS_AlterraHub.Managers.Quests;
using FCSCommon.Helpers;
using UnityEngine;
using UnityEngine.UI;

namespace FCS_AlterraHub.Mono.FCSPDA.Mono
{
    internal class PDAMissionController : MonoBehaviour
    {
        private Text _missionCounter;
        private GameObject _missions;
        private Text _description;
        private Text _itemEarnings;
        private Text _creditEarnings;
        private GameObject _missionObjectivesList;
        private GameObject _missionsList;
        private bool _isInitialized;
        private Quest _quest;

        internal void Initialize()
        {
            if (_isInitialized) return;
            //Find All the components
            _missionCounter = GameObjectHelpers.FindGameObject(gameObject, "MissionCounter").GetComponent<Text>();
            _missions = GameObjectHelpers.FindGameObject(gameObject, "Missions");
            _description = GameObjectHelpers.FindGameObject(_missions, "Description")?.GetComponentInChildren<Text>();
            _creditEarnings = GameObjectHelpers.FindGameObject(_missions, "CreditEarnings")?.GetComponent<Text>();
            _itemEarnings = GameObjectHelpers.FindGameObject(_missions, "ItemEarnings")?.GetComponent<Text>();
            _missionObjectivesList = GameObjectHelpers.FindGameObject(_missions, "MissionObjectivesList");
            _missionsList = GameObjectHelpers.FindGameObject(_missions, "MissionsList");

            InvokeRepeating(nameof(UpdateCounter),1,1);

            _isInitialized = true;
        }

        private void UpdateCounter()
        {
            _missionCounter.text = QuestManager.Instance.GetMissionCount().ToString();
        }

        private void UpdateDescription()
        {
            _description.text = _quest.Description;
        }

        private void UpdateRewards()
        {
            _creditEarnings.text = $"{_quest.CreditReward} (REWARD)";
            _itemEarnings.text = $"{Language.main.Get(_quest.TechTypeReward)} x1 (REWARD)";
        }

        private void RefreshObjectives()
        {
            for (int i = _missionObjectivesList.transform.childCount - 1; i > 0; i--)
            {
                Destroy(_missionObjectivesList.transform.GetChild(i).gameObject);
            }
            
            foreach (QuestEvent questEvent in _quest.QuestEvents)
            {
                var prefab = Instantiate(Buildables.AlterraHub.MissionObjectiveItemPrefab);
                var objectiveController = prefab.AddComponent<ObjectiveController>();
                objectiveController.Initialize(questEvent);
                prefab.transform.SetParent(_missionObjectivesList.transform,false);
            }
        }

        internal void UpdateQuest(Quest quest)
        {
            if(quest == null) return;
            _quest = quest;
            AddMissionToList();
            Refresh();
        }

        private void AddMissionToList()
        {
            for (int i = _missionsList.transform.childCount - 1; i > 0; i--)
            {
                Destroy(_missionsList.transform.GetChild(i).gameObject);
            }
            var prefab = Instantiate(Buildables.AlterraHub.MissionItemPrefab);
            var objectiveController = prefab.AddComponent<MissionItemController>();
            objectiveController.Initialize(_quest,this);
            prefab.transform.SetParent(_missionsList.transform, false);
        }

        internal void Refresh(Quest questEvent)
        {
            _quest = questEvent;
            Refresh();
        }

        private void Refresh()
        {
            UpdateDescription();
            UpdateRewards();
            RefreshObjectives();
        }
    }

    internal class MissionItemController : MonoBehaviour
    {
        private bool _isInitialized;
        private Quest _quest;
        private PDAMissionController _controller;

        internal void Initialize(Quest quest, PDAMissionController controller)
        {
            if (_isInitialized) return;

            _quest = quest;
            _controller = controller;
            var button = GetComponentInChildren<Button>();
            button.onClick.AddListener(() =>
            {
                _controller.Refresh(_quest);
            });

            var text = GetComponentInChildren<Text>();
            text.text = quest.Description;
            _isInitialized = true;
        }
    }

    internal class ObjectiveController : MonoBehaviour
    {
        private Toggle _toggle;
        private bool _isInitialized;
        private QuestEvent _qEvent;

        internal void Initialize(QuestEvent qEvent)
        {
            if (_isInitialized) return;
            
            _qEvent = qEvent;
            
            var text = GetComponentInChildren<Text>();
            text.text = qEvent.GetDescription;
            _toggle = GetComponentInChildren<Toggle>();
            InvokeRepeating(nameof(UpdateStatus),1,1);
            _isInitialized = true;
        }

        private void UpdateStatus()
        {
            if(_qEvent != null)
            {
                _toggle.isOn = _qEvent.RequirementsMet;
            }
        }
    }
}
