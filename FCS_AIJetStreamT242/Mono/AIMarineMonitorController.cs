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

        #endregion

        #region Unity Methods

        private void Awake()
        {
            //Lets get all the turbines at this base
            //InvokeRepeating("GetTurbines", 0, 1);

            //Get the SeaBase
            //_seabase = gameObject.GetComponentInParent<SubRoot>();
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

                    Turbines.Add(turbine.GetPrefabID(), turbine);
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
            if (obj != null)
            {
                QuickLogger.Debug("Removed Sub");
                Turbines.Remove(obj.GetPrefabID());
                QuickLogger.Debug("Removed Turbine");

                _aiMarineMonitorDisplay.ItemModified<string>(null);
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
            yield return new WaitForEndOfFrame();
            GameObject newSeaBase = obj?.gameObject?.transform?.parent?.gameObject;
            if (newSeaBase != null && newSeaBase == _seaBase)
            {
                Turbines.Add(obj.GetPrefabID(), obj);
                _aiMarineMonitorDisplay.ItemModified<string>(null);
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
                if (_isEnabled)
                {
                    TurnDisplayOff();
                }
            }
        }
        #endregion
    }
}
