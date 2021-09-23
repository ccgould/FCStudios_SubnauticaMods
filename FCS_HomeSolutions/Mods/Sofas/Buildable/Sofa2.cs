using System;
using FCS_HomeSolutions.Buildables;
using FCS_HomeSolutions.Configuration;
using FCSCommon.Utilities;
using UnityEngine;

namespace FCS_HomeSolutions.Mods.Sofas.Buildable
{
    internal class Sofa2Buildable : SofaBase
    {
        internal const string Sofa2ClassID = "Sofa2";
        internal const string Sofa2Friendly = "Sofa 2";
        internal const string Sofa2Description = "Don’t put butt on the floor, put it on this comfy people-shelf!";
        internal const string Sofa2PrefabName = "Sofia02";
        internal const string Sofa2KitClassID = "Sofa2_Kit";

        public Sofa2Buildable() : base(Sofa2ClassID, Sofa2Friendly, Sofa2Description, Sofa2KitClassID, Sofa2PrefabName, 9000)
        {
        }

        public override GameObject GetGameObject()
        {
            try
            {
                return CreateGameObject();
            }
            catch (Exception e)
            {
                QuickLogger.Error(e.Message);
                return null;
            }
        }
    }
}
