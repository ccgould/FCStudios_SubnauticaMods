using UnityEngine;

namespace FCS_AlterraHub.Models.Structs
{
    public struct FCSModItemSettings
    {
        public string Description;
        public LargeWorldEntity.CellLevel CellLevel;
        public Vector3 ConstructableSize;
        public Vector3 ConstructableCenter;
        public bool HasGlass;
        public bool AllowedOutside;
        public bool AllowedInBase;
        public bool AllowedOnGround;
        public bool AllowedOnWall;
        public bool RotationEnabled;
        public bool AllowedOnCeiling;
        public bool AllowedInSub;
        public bool AllowedOnConstructables;
        public decimal ItemCost;

        public string MODNAME;
        internal bool AllowedOnBase;
        internal bool AllowOnRigidBody;
        internal bool HasAnimations;
        internal bool HasBashAnimation;
        internal float DrawTime;
        internal float DropTime;
        internal float HolsterTime;
        internal bool HasFirstAnimation;

        public FCSModItemSettings(string modName)
        {
            MODNAME = modName;            
        }
    }
}
