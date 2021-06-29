using UnityEngine;

namespace FCS_ProductionSolutions.Mods.DeepDriller.Models
{
    public class LaserScript : MonoBehaviour
    {
        public Transform StartPoint;

        public Transform EndPoint;

        private LineRenderer laserLine;
        // Start is called before the first frame update
        void Start()
        {
            laserLine = GetComponent<LineRenderer>();
            laserLine.startWidth = .2f;
            laserLine.endWidth = .2f;
        }

        // Update is called once per frame
        void Update()
        {
            if (laserLine != null)
            {
                laserLine.SetPosition(0, StartPoint.position);
                laserLine.SetPosition(1, EndPoint.position);
            }
        }
    }
}
