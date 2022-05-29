using System;
using System.Collections;
using FCSCommon.Utilities;
using UnityEngine;


namespace FCS_HomeSolutions.Mods.Sofas.Buildable
{
    internal class Sofa3Buildable : SofaBase
    {
        internal const string Sofa3ClassID = "Sofa3";
        internal const string Sofa3Friendly = "Sofa 3";
        internal const string Sofa3Description = "Don’t put butt on the floor, put it on this comfy people-shelf!";
        internal const string Sofa3PrefabName = "Sofia03";
        internal const string Sofa3KitClassID = "Sofa3_Kit";
        public Sofa3Buildable() : base(Sofa3ClassID, Sofa3Friendly, Sofa3Description, Sofa3KitClassID, Sofa3PrefabName, 9000)
        {
        }
#if SUBNAUTICA_STABLE
        public override GameObject GetGameObject()
        {
            try
            {
                return CreateGameObject(0.15f);
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
            yield return CreateGameObjectAsync(gameObject, 0.15f);
        }
    }
}
