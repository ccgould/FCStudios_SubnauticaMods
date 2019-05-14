using UnityEngine;

namespace FCSCommon.Objects
{
    public class TargetRotation
    {

        #region Constructor
        /// <summary>
        /// Default Constructor 
        /// </summary>
        public TargetRotation(Quaternion quaternion)
        {
            X = quaternion.x;
            Y = quaternion.y;
            Z = quaternion.z;
            W = quaternion.w;
        }
        #endregion
        public float X { get; set; }
        public float Y { get; set; }
        public float Z { get; set; }
        public float W { get; set; }
    }
}
