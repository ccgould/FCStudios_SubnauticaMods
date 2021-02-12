using System;
using FCS_AlterraHub.Managers.Mission;
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
        private Mission _mission;

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
            _missionCounter.text = MissionManager.Instance.GetMissionCount().ToString();
        }

        private void UpdateDescription()
        {
            _description.text = _mission.Description;
        }

        private void UpdateRewards()
        {
            _creditEarnings.text = $"{_mission.CreditReward} (REWARD)";
            _itemEarnings.text = $"{Language.main.Get(_mission.TechTypeReward)} x1 (REWARD)";
        }

        private void RefreshObjectives()
        {
            for (int i = _missionObjectivesList.transform.childCount - 1; i > 0; i--)
            {
                Destroy(_missionObjectivesList.transform.GetChild(i).gameObject);
            }

            foreach (MissionTask questEvent in _mission.Tasks)
            {
                var prefab = Instantiate(Buildables.AlterraHub.MissionObjectiveItemPrefab);
                var objectiveController = prefab.AddComponent<ObjectiveController>();
                objectiveController.Initialize(questEvent);
                prefab.transform.SetParent(_missionObjectivesList.transform, false);
            }
        }

        private void AddMissionToList(Mission mission)
        {
            for (int i = _missionsList.transform.childCount - 1; i > 0; i--)
            {
                Destroy(_missionsList.transform.GetChild(i).gameObject);
            }
            var prefab = Instantiate(Buildables.AlterraHub.MissionItemPrefab);
            var objectiveController = prefab.AddComponent<MissionItemController>();
            objectiveController.Initialize(mission, this);
            prefab.transform.SetParent(_missionsList.transform, false);
        }

        internal void Refresh(Mission mission)
        {
            _mission = mission;
            Refresh();
        }

        private void Refresh()
        {
            UpdateDescription();
            UpdateRewards();
            RefreshObjectives();
        }

        public void UpdateMissions()
        {
            foreach (Mission mission in MissionManager.Instance.Missions)
            {
                AddMissionToList(mission);
            }
        }
    }

    internal class MissionItemController : MonoBehaviour
    {
        private bool _isInitialized;
        private Mission _mission;
        private PDAMissionController _controller;

        internal void Initialize(Mission quest, PDAMissionController controller)
        {
            if (_isInitialized) return;

            _mission = quest;
            _controller = controller;
            var button = GetComponentInChildren<Button>();
            button.onClick.AddListener(() =>
            {
                _controller.Refresh(_mission);
            });

            var text = GetComponentInChildren<Text>();
            text.text = quest.Name;
            _isInitialized = true;
        }
    }

    internal class ObjectiveController : MonoBehaviour
    {
        private Toggle _toggle;
        private bool _isInitialized;
        private MissionTask _missionTask;

        internal void Initialize(MissionTask missionTask)
        {
            if (_isInitialized) return;
            
            _missionTask = missionTask;
            
            var text = GetComponentInChildren<Text>();
            text.text = missionTask.Description;
            _toggle = GetComponentInChildren<Toggle>();
            InvokeRepeating(nameof(UpdateStatus),1,1);
            _isInitialized = true;
        }

        private void UpdateStatus()
        {
            if(_missionTask != null)
            {
                _toggle.isOn = _missionTask.IsCompleted;
            }
        }
    }
}
