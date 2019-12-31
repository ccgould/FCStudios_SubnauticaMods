using FCSTechFabricator.Interfaces;
using SMLHelper.V2.Assets;
using System;
using UnityEngine;

namespace FCSTechFabricator.Abstract_Classes
{
    public abstract class FCSTechFabricatorItem : Craftable, IFCSTechFabricatorItem
    {
        protected FCSTechFabricatorItem(string classId, string prefabFileName, TechType techType = TechType.None) : base(classId, prefabFileName, techType)
        {
        }

        public override GameObject GetGameObject()
        {
            throw new NotImplementedException();
        }

        public TechType TechTypeID { get; set; }
        public string ClassID_I { get; set; }
        public string FriendlyName_I { get; set; }

        public bool IsRegistered = false;

        public abstract void Register();
    }
}
