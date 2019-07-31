using FCSCommon.Helpers;
using FCSCommon.Utilities;
using System;
using System.Collections;
using UnityEngine;

namespace ExStorageDepot.Mono.Managers
{
    internal class ExStorageDepotDisplayManager : AIDisplay
    {
        private ExStorageDepotController _mono;

        internal void Initialize(ExStorageDepotController mono)
        {
            _mono = mono;

            if (FindAllComponents())
            {
                PowerOnDisplay();
                DrawPage(1);
            }
        }

        public override void ClearPage()
        {
            throw new NotImplementedException();
        }

        public override void OnButtonClick(string btnName, object tag)
        {
            throw new NotImplementedException();
        }

        public override void ItemModified<T>(T item)
        {
            throw new NotImplementedException();
        }

        public override bool FindAllComponents()
        {
            var canvas = gameObject.GetComponentInChildren<Canvas>();

            if (canvas == null)
            {
                QuickLogger.Error("Canvas could not be found!");
                return false;
            }

            return true;
        }

        public override IEnumerator PowerOff()
        {
            throw new NotImplementedException();
        }

        public override IEnumerator PowerOn()
        {
            yield return new WaitForEndOfFrame();
            _mono.AnimationManager.ToggleScreenState();

        }

        public override IEnumerator ShutDown()
        {
            throw new NotImplementedException();
        }

        public override IEnumerator CompleteSetup()
        {
            throw new NotImplementedException();
        }

        public override void DrawPage(int page)
        {

        }
    }
}
