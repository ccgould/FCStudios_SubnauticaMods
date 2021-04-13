using System;
using System.Collections.Generic;
using System.Linq;
using FCS_AlterraHub.Helpers;
using FCS_AlterraHub.Managers.Mission.Enumerators;
using FCS_AlterraHub.Systems;
using FCSCommon.Utilities;

namespace FCS_AlterraHub.Managers.Mission
{
   public class Mission
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public bool IsComplete => CheckIfComplete();
        public bool RewardHasBeenClaimed { get; set; }
        private bool CheckIfComplete()
        {
            foreach (var missionTask in Tasks)
            {
                if (!missionTask.IsCompleted)
                {
                    return false;
                }
            }

            return true;
        }

        public float Percentage { get; private set; }
        public decimal CreditReward { get;  set; }
        public TechType TechTypeReward { get;  set; }
        public string AudioTrackName { get; set; }
        public List<MissionTask> Tasks { get; set; } = new List<MissionTask>();
        public Action<Mission> OnMissionComplete { get; set; }
        public Action<Mission> OnMissionStart { get; set; }
        public Action OnStatusChanged { get; set; }

        private MissionStatus _status = MissionStatus.InActive;
        private int _repeatedTimes;

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
                //task.Owner = this;
                task.Activate();
                task.OnProgressChanged += Task_OnProgressChanged;
                task.OnStatusChanged += Task_OnStatusChanged;
            }

            Status = MissionStatus.Active;
            OnMissionStart?.Invoke(this);
            ID = Guid.NewGuid().ToString();
        }

        private void Task_OnStatusChanged(TaskStatus before, TaskStatus after, MissionTask self)
        {
            CalculatePercentage();

            if (Percentage >= 100)
            {
                self.OnProgressChanged -= Task_OnProgressChanged;
                self.OnStatusChanged -= Task_OnStatusChanged;
                OnMissionComplete?.Invoke(this);
                //CompleteAndGiveRewards();

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
                //Owner = this
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
            if (TechTypeReward != TechType.None)
            {
                PlayerInteractionHelper.GivePlayerItem(TechTypeReward);
            }

            if (CreditReward > 0)
            {
                CardSystem.main.AddFinances(CreditReward);
            }
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

        public bool HasMissionKey(string missionKey)
        {
            return Tasks.Any(x => x.key == missionKey);
        }

        public bool CompleteAndGiveRewards(bool forceComplete = false)
        {
            
            _repeatedTimes++;
            CompleteCompletableTasks(forceComplete);
            GiveRewards();
            Status = MissionStatus.Completed;
            QuickLogger.Debug("Completed quest/achievement with ID: " + ID + " and gave rewards. Repeated quest #" + _repeatedTimes + " times. Quest status is: " + Status);
            return true;
        }

        public string ID { get; set; }

        private void CompleteCompletableTasks(bool forceComplete)
        {
            foreach (var task in Tasks)
            {
                if ((task.IsCompleted == false && task.IsProgressSufficientToComplete()) || forceComplete)
                {
                    task.Complete(forceComplete);
                }
            }
        }

        public void ClaimReward()
        {
            QuickLogger.Debug($"Is Completed: {IsComplete}",true);

            if (!RewardHasBeenClaimed)
            {
                CompleteAndGiveRewards();
                RewardHasBeenClaimed = true;
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
