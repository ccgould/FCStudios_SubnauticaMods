using System;
using DataStorageSolutions.Mono;
using FCSCommon.Components;
using FCSCommon.Helpers;
using FCSCommon.Utilities;
using UnityEngine;
using UnityEngine.EventSystems;

namespace DataStorageSolutions.Display
{
    internal class ServerHitController: InterfaceButton
    {
        private bool _isInitialized;
        private GameObject _slot;
        public DSSServerController Controller { get; set; }

        //public override void OnEnable()
        //{
        //    base.OnEnable();

        //    if (_isInitialized) return;
            
        //    Disabled = false;

        //    _slot = GameObjectHelpers.FindGameObject(gameObject, "Slot");

        //    _isInitialized = true;
        //}

    }
}
