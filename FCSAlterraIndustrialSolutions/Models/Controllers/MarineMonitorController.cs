using FCSAlterraIndustrialSolutions.Configuration;
using FCSAlterraIndustrialSolutions.Logging;
using FCSAlterraIndustrialSolutions.Patches;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace FCSAlterraIndustrialSolutions.Models.Controllers
{
    public class MarineMonitorController : MonoBehaviour, IConstructable
    {
        #region Public Properties

        public Dictionary<string, JetStreamT242Controller> Turbines { get; set; } = new Dictionary<string, JetStreamT242Controller>();
        public bool IsBeingDeleted { get; set; }

        #endregion

        #region Private Members

        private GameObject _seaBase;
        private bool _constructed;
        private bool _isEnabled;
        private MarineMoniterDisplay _marineMonitorDisplay;

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
                var jetStreamT242S = _seaBase.GetComponentsInChildren<JetStreamT242Controller>().ToList();

                foreach (var turbine in jetStreamT242S)
                {

                    Turbines.Add(turbine.ID, turbine);
                }
            }
        }

        private void TurnDisplayOn()
        {
            try
            {
                if (IsBeingDeleted) return;

                if (_marineMonitorDisplay != null)
                {
                    Log.Info("Turnoff");

                    TurnDisplayOff();
                }

                _marineMonitorDisplay = gameObject.AddComponent<MarineMoniterDisplay>();
                _marineMonitorDisplay.Setup(this);

            }
            catch (Exception e)
            {
                Log.Error($"Error in TurnDisplayOn Method: {e.Message} || {e.InnerException} || {e.Source}");
            }
        }

        private void TurnDisplayOff()
        {
            if (IsBeingDeleted) return;

            if (_marineMonitorDisplay != null)
            {
                _marineMonitorDisplay.ShutDownDisplay();
                Destroy(_marineMonitorDisplay);
                _marineMonitorDisplay = null;
            }
        }

        private void AlertedNewTurbinePlaced(JetStreamT242Controller obj)
        {
            if (obj != null)
            {
                StartCoroutine(TrackNewTurbineCoroutine(obj));
            }
        }

        private void AlertedNewTurbineDestroyed(JetStreamT242Controller obj)
        {
            if (obj != null)
            {
                Log.Info("Removed Sub");
                Turbines.Remove(obj.ID);
                Log.Info("Removed Turbine");

                _marineMonitorDisplay.ItemModified<string>(null);
            }
        }

        #endregion

        #region Public Methods



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
                ErrorMessage.AddMessage($"[{Information.ModName}] ERROR: Can not work out what base it was placed inside.");
                Log.Error("ERROR: Can not work out what base it was placed inside.");
                yield break;
            }
            AISolutions_Patcher.AddEventHandlerIfMissing(AlertedNewTurbinePlaced);
            JetStreamDestroy_Patcher.AddEventHandlerIfMissing(AlertedNewTurbineDestroyed);
            GetTurbines();
            TurnDisplayOn();
        }

        private IEnumerator TrackNewTurbineCoroutine(JetStreamT242Controller obj)
        {
            yield return new WaitForEndOfFrame();
            GameObject newSeaBase = obj?.gameObject?.transform?.parent?.gameObject;
            if (newSeaBase != null && newSeaBase == _seaBase)
            {
                Turbines.Add(obj.ID, obj);
                _marineMonitorDisplay.ItemModified<string>(null);
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
            Log.Info($"Constructed - {constructed}");

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
