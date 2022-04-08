using System;
using System.Collections;
using FCSCommon.Utilities;
using UnityEngine;


namespace FCS_HomeSolutions.Mods.Sofas.Buildable
{
    internal class NeonBarStoolBuildable : SofaBase
    {
        internal const string NeonBarStoolClassID = "NeonBarStool";
        internal const string NeonBarStoolFriendly = "Neon Bar Stool";
        internal const string NeonBarStoolDescription = "Companion seating for the Peeper Lounge. Seatbelt not included.";
        internal const string NeonBarStoolPrefabName = "FCS_NeonBarStool";
        internal const string NeonBarStoolKitClassID = "NeonBarStool_Kit";

        public NeonBarStoolBuildable() : base(NeonBarStoolClassID, NeonBarStoolFriendly, NeonBarStoolDescription, NeonBarStoolKitClassID, NeonBarStoolPrefabName, 9000)
        {
        }

#if SUBNAUTICA_STABLE
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
#endif
        public override IEnumerator GetGameObjectAsync(IOut<GameObject> gameObject)
        {
            yield return CreateGameObjectAsync(gameObject, 0.38f);
        }
    }
}
