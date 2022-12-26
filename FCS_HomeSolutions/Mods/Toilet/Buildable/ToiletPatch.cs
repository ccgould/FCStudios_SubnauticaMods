﻿using System;
using System.Collections;
using FCS_AlterraHub.Helpers;
using FCS_HomeSolutions.Mods.Sofas.Buildable;
using FCSCommon.Utilities;
using UnityEngine;

namespace FCS_HomeSolutions.Mods.Toilet.Buildable
{
    internal class ToiletBuildable : SofaBase
    {
        internal const string ToiletClassID = "FCSToilet";
        internal const string ToiletFriendly = "Toilet";
        internal const string ToiletDescription = "A simple toilet for convenient hygiene. (Privacy not included)";
        internal const string ToiletPrefabName = "FCS_Toilet";
        internal const string ToiletKitClassID = "Toilet_Kit";

        public ToiletBuildable() : base(ToiletClassID, ToiletFriendly, ToiletDescription, ToiletKitClassID,
            ToiletPrefabName, 9000)
        {
        }


        public override IEnumerator GetGameObjectAsync(IOut<GameObject> gameObject)
        {
            var task = new TaskResult<GameObject>();
            yield return CreateGameObjectAsync(task, 0.15f);

            try
            {
                var obj = task.Get();
                var center = new Vector3(-0.003933489f, 1.415764f, 0.005244613f);
                var size = new Vector3(1.128494f, 2.60179f, 0.895113f);
                GameObjectHelpers.AddConstructableBounds(obj, size, center);
                gameObject.Set(obj);
            }
            catch (Exception e)
            {
                QuickLogger.Error(e.Message);
                gameObject.Set(null);
            }
        }
    }
}