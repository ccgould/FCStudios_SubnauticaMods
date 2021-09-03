using System;
using FCSCommon.Utilities;
using UnityEngine;


namespace FCS_HomeSolutions.Mods.Sofas.Buildable
{
    internal class NeonBarStoolBuildable : SofaBase
    {
        internal const string NeonBarStoolClassID = "NeonBarStool";
        internal const string NeonBarStoolFriendly = "NeonBarStool";
        internal const string NeonBarStoolDescription = "N/A";
        internal const string NeonBarStoolPrefabName = "FCS_NeonBarStool";
        internal const string NeonBarStoolKitClassID = "NeonBarStool_Kit";

        public NeonBarStoolBuildable() : base(NeonBarStoolClassID, NeonBarStoolFriendly, NeonBarStoolDescription, NeonBarStoolKitClassID, NeonBarStoolPrefabName, 9000)
        {
        }

        public override GameObject GetGameObject()
        {
            try
            {
                return CreateGameObject(0.38f);
            }
            catch (Exception e)
            {
                QuickLogger.Error(e.Message);
                return null;
            }
        }
    }
}
