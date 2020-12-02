using System;
using FCSCommon.Utilities;
using UnityEngine;

namespace FCS_HomeSolutions.HoverLiftPad.Mono
{
    internal class PrawnSuitTrigger : MonoBehaviour
    {
        public Action<Exosuit> OnPrawnEntered;
        public Action<Exosuit> OnPrawnExit;
        //public Collider CurrentCollider { get; set; }
        public bool IsPrawnOnPad { get; set; }
        //When the Primitive collides with the walls, it will reverse direction
        private void OnTriggerEnter(Collider collision)
        {
            QuickLogger.Debug($"[PrawnSuitTrigger] Collision with {collision.name} detected", true);
            var exosuit = collision.GetComponentInParent<Exosuit>();
            if (exosuit != null)
            {
                OnPrawnEntered?.Invoke(exosuit);
                IsPrawnOnPad = true;
            }
        }

        //private void OnTriggerStay(Collider collision)
        //{
        //    CurrentCollider = collision;
        //}

        private void OnTriggerExit(Collider collision)
        {
            QuickLogger.Debug($"[PrawnSuitTrigger] Collision with {collision.name} detected", true);
            var exosuit = collision.GetComponentInParent<Exosuit>();
            if (exosuit != null)
            {
                OnPrawnExit?.Invoke(exosuit);
                IsPrawnOnPad = false;
            }
        }
    }
}