using System;
using System.Collections.Generic;

namespace FCS_AlterraHub.Managers.Mission
{
   public class Mission
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public bool IsComplete { get; }
        public string AudioTrackName { get; set; }
        public List<MissionTask> Tasks { get; set; } = new List<MissionTask>();
        public Action<Mission> OnMissionComplete { get; set; }
        public Action<Mission> OnMissionStart { get; set; }
        public Action OnStatusChanged { get; set; }

        public bool CanGiveRewards()
        {
            return true;
        }

        public void GiveRewards()
        {
            
        }
    }
}
