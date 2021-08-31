using System;
using FCS_HomeSolutions.Buildables;
using FCS_HomeSolutions.Configuration;
using FCSCommon.Utilities;
using UnityEngine;


namespace FCS_HomeSolutions.Mods.Sofas.Buildable
{
    internal class Sofa3Buildable : SofaBase
    {
        public Sofa3Buildable() : base(Mod.Sofa3ClassID, Mod.Sofa3Friendly, Mod.Sofa3Description, Mod.Sofa3KitClassID, ModelPrefab.Sofa3Prefab)
        {
        }

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
    }
}
