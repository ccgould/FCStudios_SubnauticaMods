using System;
using FCS_AlterraHub.Managers.Mission.Enumerators;
using FCS_AlterraHub.Registration;
using FCS_AlterraHub.Systems;
using FCSCommon.Utilities;
using UnityEngine;


namespace FCS_AlterraHub.Managers.Mission
{
    public class MissionTask
    {
        private TaskStatus _status = TaskStatus.InActive;
        private float _progress;
        public delegate void StatusChanged(TaskStatus before, TaskStatus after, MissionTask self);
        public delegate void ProgressChanged(float before, MissionTask task);
        public event ProgressChanged OnProgressChanged;
        public string key;
        private bool _gaveRewards;
        public DateTime? StartTime { get; protected set; }

        public float Progress
        {
            get => _progress;
            set => _progress = value;
        }

        public float ProgressCap { get; set; }
        public bool IsCompleted => _status == TaskStatus.Completed;

        public double TimeSinceStartInSeconds => StartTime == null ? 0d : (DateTime.Now - StartTime).Value.TotalSeconds;

        
        public event StatusChanged OnStatusChanged;

        //public virtual Mission Owner { get; set; }


        public MissionTask(string key, float progressCap)
        {
            this.key = key;
            ProgressCap = progressCap;
        }
        
        public TaskStatus Status
        {
            get => _status;
            protected set
            {
                var before = _status;
                _status = value;

                if (before != _status)
                {
                    OnStatusChanged?.Invoke(before, _status, this);
                }
            }
        }

        public string Description { get; set; }
        public string AudioClip { get; set; }
        public TaskCondition Condition { get; set; }
        public bool SelfValidate { get; set; }

        public void Activate()
        {
            if (Status == TaskStatus.InActive)
            {
                StartTime = DateTime.Now;

                Status = TaskStatus.Active;
            }
            else if (Status == TaskStatus.Completed)
            {
                QuickLogger.Debug("Trying to activate completed task. If you wish to start it again call SetProgress instead.");
            }
        }
        
        public bool ChangeProgress(float amount)
        {
            return SetProgress(Progress + amount);
        }

        public virtual bool SetProgress(float amount)
        {
            var before = _progress;
            _progress = amount;

            if (Mathf.Approximately(before, _progress) == false)
            {
                OnProgressChanged?.Invoke(before, this);
            }

            if (IsProgressSufficientToComplete())
            {
                Complete();
            }

            return true;
        }

        public bool CanComplete()
        {
            if (_gaveRewards == false)
            {
                var s = CanGiveRewards();
                if (s == false)
                {
                    return s;
                }
            }

            if (IsProgressSufficientToComplete() == false)
            {
                return false;
            }

            return true;
        }

        public bool Complete(bool forceComplete = false)
        {
            if (CanComplete() == false && forceComplete == false)
            {
                return false;
            }

            Status = TaskStatus.Completed;

            GiveRewards();

            //QuickLogger.Debug($"Completed task: \"{key}\" on {Owner.GetType().Name} \"{Owner.Name}\"",true);
            return IsCompleted;
        }

        public bool CanGiveRewards()
        {
            if (_gaveRewards)
            {
                return false;
            }

            //var s = Owner.CanGiveRewards();
            //if (s == false)
            //{
            //    return s;
            //}
            
            return true;
        }

        public virtual bool GiveRewards(bool force = false)
        {
            if (IsCompleted == false && force == false)
            {
                return false;
            }

            _gaveRewards = true;
            return true;
        }

        public bool IsProgressSufficientToComplete()
        {
            return Progress >= ProgressCap;
        }

        public void ConditionValid(TaskCondition condition)
        {
            if (condition.TechType == Condition.TechType && condition.MissionType == Condition.MissionType)
            {
                ChangeProgress(1);
            }
        }

        public void CheckCondition()
        {
            if (Condition.MissionType == MissionType.Scan)
            {
                if(KnownTech.knownTech.Contains(Condition.TechType));
                {
                    ChangeProgress(1);
                }
            }

            if (Condition.MissionType == MissionType.Construct)
            {
                SetProgress(FCSAlterraHubService.PublicAPI.GetTechBuiltCount(Condition.TechType));
            }

            if (Condition.MissionType == MissionType.AccountCreation)
            {
                if (CardSystem.main.HasBeenRegistered())
                {
                    ChangeProgress(1);
                }
            }
        }
    }
}
