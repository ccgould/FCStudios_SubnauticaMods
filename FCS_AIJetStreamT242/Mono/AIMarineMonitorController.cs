using FCS_AIMarineTurbine.Display;
using FCS_AIMarineTurbine.Mono;
using FCS_AIMarineTurbine.Patches;
using FCSCommon.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace FCSAlterraIndustrialSolutions.Models.Controllers
{
    internal class AIMarineMonitorController : MonoBehaviour, IConstructable
    {
        #region Public Properties

        public Dictionary<string, AIJetStreamT242Controller> Turbines { get; set; } = new Dictionary<string, AIJetStreamT242Controller>();
        public bool IsBeingDeleted { get; set; }

        #endregion

        #region Private Members

        private GameObject _seaBase;
        private bool _constructed;
        private bool _isEnabled;
        private AIMarineMoniterDisplay _aiMarineMonitorDisplay;
        private bool _runStartUpOnEnable;

        #endregion

        #region Unity Methods

        private void OnEnable()
        {
            if (!_runStartUpOnEnable) return;

            if (_isEnabled == false)
            {
                _isEnabled = true;
                StartCoroutine(Startup());
            }
            else
            {
                TurnDisplayOn();
            }
        }

        private void Update()
        {

        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Gets all the turbines in the base
        /// </summary>
        private void GetTurbines()
        {
            //Clear the list
            Turbines.Clear();

            //Check if there is a base connected
            if (_seaBase != null)
            {
                var jetStreamT242S = _seaBase.GetComponentsInChildren<AIJetStreamT242Controller>().ToList();

                foreach (var turbine in jetStreamT242S)
                {

                    Turbines.Add(turbine.GetPrefabId(), turbine);
                }
            }
        }

        private void TurnDisplayOn()
        {
            try
            {
                if (IsBeingDeleted) return;

                if (_aiMarineMonitorDisplay != null)
                {
                    QuickLogger.Debug("Turnoff");

                    TurnDisplayOff();
                }

                _aiMarineMonitorDisplay = gameObject.AddComponent<AIMarineMoniterDisplay>();
                _aiMarineMonitorDisplay.Setup(this);

            }
            catch (Exception e)
            {
                QuickLogger.Error($"Error in TurnDisplayOn Method: {e.Message} || {e.InnerException} || {e.Source}");
            }
        }

        private void TurnDisplayOff()
        {
            if (IsBeingDeleted) return;

            if (_aiMarineMonitorDisplay != null)
            {
                _aiMarineMonitorDisplay.ShutDownDisplay();
                Destroy(_aiMarineMonitorDisplay);
                _aiMarineMonitorDisplay = null;
            }
        }

        private void AlertedNewTurbinePlaced(AIJetStreamT242Controller obj)
        {
            if (obj != null)
            {
                StartCoroutine(TrackNewTurbineCoroutine(obj));
            }
        }

        private void AlertedNewTurbineDestroyed(AIJetStreamT242Controller obj)
        {
            QuickLogger.Debug("Attempting to remove turbine", true);
            if (obj != null)
            {
                QuickLogger.Debug("OBJ Not NULL", true);
                Turbines.Remove(obj.GetPrefabId());
                QuickLogger.Debug("Past Turbine", true);
                _aiMarineMonitorDisplay.ItemModified(TechType.None);
                QuickLogger.Debug("Removed Turbine");
            }
        }

        #endregion

        #region IEnumerator

        private IEnumerator Startup()
        {
            if (IsBeingDeleted) yield break;
            yield return new WaitForEndOfFrame();
            if (IsBeingDeleted) yield break;

            //Get the SeaBase
            _seaBase = gameObject?.transform?.parent?.gameObject;
            if (_seaBase == null)
            {
                ErrorMessage.AddMessage($"[MarineMonitor] ERROR: Can not work out what base it was placed inside.");
                QuickLogger.Error("ERROR: Can not work out what base it was placed inside.");
                yield break;
            }
            AIMarineTurbine_Patcher.AddEventHandlerIfMissing(AlertedNewTurbinePlaced);
            JetStreamDestroy_Patcher.AddEventHandlerIfMissing(AlertedNewTurbineDestroyed);
            GetTurbines();
            TurnDisplayOn();
        }

        private IEnumerator TrackNewTurbineCoroutine(AIJetStreamT242Controller obj)
        {

            // We yield to the end of the frame as we need the parent/children tree to update.
            yield return new WaitForEndOfFrame();
            GameObject newSeaBase = obj?.gameObject?.transform?.parent?.gameObject;
            QuickLogger.Debug("Attempting to add turbine", true);
            if (newSeaBase != null && newSeaBase == _seaBase)
            {
                Turbines.Add(obj.GetPrefabId(), obj);
                QuickLogger.Debug($"Turbine Count: {Turbines.Count}", true);
                _aiMarineMonitorDisplay.ItemModified(TechType.None);
            }
        }

        #endregion

        #region Interface Methods
        public bool CanDeconstruct(out string reason)
        {
            reason = String.Empty;
            return true;
        }

        public void OnConstructedChanged(bool constructed)
        {
            QuickLogger.Debug($"Constructed - {constructed}");

            _constructed = constructed;

            if (IsBeingDeleted) return;

            if (constructed)
            {
                if (isActiveAndEnabled)
                {

                    if (_isEnabled == false)
                    {
                        _isEnabled = true;
                        StartCoroutine(Startup());
                    }
                    else
                    {
                        TurnDisplayOn();
                    }
                }
                else
                {
                    _runStartUpOnEnable = true;
                }
            }



            //else
            //{
            //    if (_isEnabled)
            //    {
            //        TurnDisplayOff();
            //    }
            //}
        }
        #endregion

        private void OnDestroy()
        {
            AIMarineTurbine_Patcher.RemoveEventHandler(AlertedNewTurbinePlaced);
            JetStreamDestroy_Patcher.RemoveEventHandler(AlertedNewTurbineDestroyed);
        }
    }
}
