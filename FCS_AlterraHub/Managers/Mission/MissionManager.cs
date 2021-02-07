using System.Collections.Generic;
using UnityEngine;

namespace FCS_AlterraHub.Managers.Mission
{
    public class MissionManager : MonoBehaviour
    {
        public List<Mission> Missions { get; set; } = new List<Mission>();

        public static MissionManager Instance;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else if (Instance != this)
            {
                Destroy(gameObject);
            }
        }

        public int GetMissionCount()
        {
            return Missions.Count;
        }

        public void CompleteCurrentMission()
        {
            
        }
    }
}
