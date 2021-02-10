using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using FCS_AlterraHub.Managers.Mission.Enumerators;
using FCSCommon.Utilities;
using UnityEngine;

namespace FCS_AlterraHub.Managers.Mission
{
   public class Mission
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public bool IsComplete { get; private set; }
        public string AudioTrackName { get; set; }
        public List<MissionTask> Tasks { get; set; } = new List<MissionTask>();
        public Action<Mission> OnMissionComplete { get; set; }
        public Action<Mission> OnMissionStart { get; set; }
        public Action OnStatusChanged { get; set; }

        private MissionStatus _status = MissionStatus.InActive;
        public MissionStatus Status
        {
            get => _status;
            protected set
            {
                var before = _status;
                _status = value;

                if (before != _status)
                {
                    OnStatusChanged?.Invoke();
                }
            }
        }

        public float Percentage { get; private set; }

        private void CalculatePercentage()
        {
            var completedCount = Tasks.Count(x => x.IsCompleted);
            QuickLogger.Debug($"Amount Completed: {completedCount}",true);
            QuickLogger.Debug($"Task Count: {Tasks.Count}",true);
            QuickLogger.Debug($"Percentage: {(float)completedCount / Tasks.Count * 100.0f}",true);
            Percentage = (float)completedCount / Tasks.Count * 100.0f;
        }

        internal void Load()
        {
            if (Status != MissionStatus.InActive) return;

            foreach (MissionTask task in Tasks)
            {
                task.Owner = this;
                task.Activate();
                task.OnProgressChanged += Task_OnProgressChanged;
                task.OnStatusChanged += Task_OnStatusChanged;
            }

            Status = MissionStatus.Active;
            OnMissionStart?.Invoke(this);
        }

        private void Task_OnStatusChanged(TaskStatus before, TaskStatus after, MissionTask self)
        {
            if (after == TaskStatus.Completed)
            {
                self.OnProgressChanged -= Task_OnProgressChanged;
                self.OnStatusChanged -= Task_OnStatusChanged;
                IsComplete = true;
                Status = MissionStatus.Completed;
                OnMissionComplete?.Invoke(this);
            }
        }

        private void Task_OnProgressChanged(float before, MissionTask task)
        {
            CalculatePercentage();
        }

        internal void AddTask(string description, TaskCondition condition,string audioClip = "")
        {
            var task = new MissionTask(Guid.NewGuid().ToString(), 1)
            {
                Description = description, 
                AudioClip = audioClip, 
                Condition = condition, 
                Owner = this
            };

            task.Activate();
            task.OnProgressChanged += Task_OnProgressChanged;
            task.OnStatusChanged += Task_OnStatusChanged;
            Tasks.Add(task);
        }

        public virtual bool CanSetTaskProgress(string key, float value)
        {
            if (Status != MissionStatus.Active)
            {
                QuickLogger.Debug("Trying to set progress on quest task but quest is not active. Setting progress ignored status is: " + Status);
                return false;
            }
            
            return true;
        }

        public virtual bool SetTaskProgress(string key, float value)
        {
            if (CanSetTaskProgress(key, value) == false)
            {
                return false;
            }

            var task = GetTask(key);
            if (task != null)
            {
                return task.SetProgress(value);
            }

            return false;
        }

        public bool ChangeTaskProgress(string key, float value)
        {
            var task = GetTask(key);
            if (task != null)
            {
                return SetTaskProgress(key, task.Progress + value);
            }

            return false;
        }

        public MissionTask GetTask(string key)
        {
            return Tasks.FirstOrDefault(task => task.key == key);
        }

        public bool CanGiveRewards()
        {
            return true;
        }

        public void GiveRewards()
        {
            
        }

        public void CheckTasks(TaskCondition condition)
        {
            foreach (MissionTask missionTask in Tasks)
            {
                if(missionTask.IsCompleted) continue;
                missionTask.ConditionValid(condition);
            }
        }

        public void CheckSelfValidateTasks()
        {
            foreach (MissionTask missionTask in Tasks)
            {
                if (missionTask.SelfValidate && !missionTask.IsCompleted)
                {
                    missionTask.CheckCondition();
                }
            }
        }

        public void CheckDeviceActionTasks(TechType deviceTechType, TechType techType, DeviceAction deviceAction)
        {
            foreach (MissionTask missionTask in Tasks)
            {
                if (missionTask.IsCompleted) continue;
                {
                    if(missionTask.Condition.DeviceAction == deviceAction && missionTask.Condition.TechType == deviceTechType && missionTask.Condition.TargetTechType == techType)
                    {
                        missionTask.ChangeProgress(1);
                    }
                }
            }
        }
    }

   public struct TaskCondition
   {
       public TechType TechType { get; set; }
       public MissionType MissionType { get; set; }
       public DeviceAction DeviceAction { get; set; }
       public TechType TargetTechType { get; set; }
   }

   public enum MissionType
   {
       None,
       Collection,
       Kill,
       Scan,
       Creation,
       Construct,
       AccountCreation,
       DeviceAction
   }

   public enum MissionStatus
   {
       Active = 0,
       Completed = 1,
       InActive = 2,
       Cancelled = 3
   }
}
