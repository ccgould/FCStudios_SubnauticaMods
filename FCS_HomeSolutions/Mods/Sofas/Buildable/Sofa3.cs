﻿using System;
using FCS_HomeSolutions.Buildables;
using FCS_HomeSolutions.Configuration;
using FCSCommon.Utilities;
using UnityEngine;


namespace FCS_HomeSolutions.Mods.Sofas.Buildable
{
    internal class Sofa3Buildable : SofaBase
    {
        internal const string Sofa3ClassID = "Sofa3";
        internal const string Sofa3Friendly = "Sofa 3";
        internal const string Sofa3Description = "N/A";
        internal const string Sofa3PrefabName = "Sofia03";
        internal const string Sofa3KitClassID = "Sofa3_Kit";
        public Sofa3Buildable() : base(Sofa3ClassID, Sofa3Friendly, Sofa3Description, Sofa3KitClassID, Sofa3PrefabName, 9000)
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