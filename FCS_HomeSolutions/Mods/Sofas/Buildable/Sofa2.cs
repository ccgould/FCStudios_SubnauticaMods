using System;
using FCS_HomeSolutions.Buildables;
using FCS_HomeSolutions.Configuration;
using FCSCommon.Utilities;
using UnityEngine;

namespace FCS_HomeSolutions.Mods.Sofas.Buildable
{
    internal class Sofa2Buildable : SofaBase
    {
        public Sofa2Buildable() : base(Mod.Sofa2ClassID, Mod.Sofa2Friendly, Mod.Sofa2Description, Mod.Sofa2KitClassID,
            ModelPrefab.Sofa2Prefab)
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
