using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DataStorageSolutions.Buildables;
using DataStorageSolutions.Configuration;
using DataStorageSolutions.Mono;
using FCSCommon.Components;
using FCSCommon.Enums;
using FCSCommon.Helpers;
using FCSCommon.Utilities;
using UnityEngine;
using UnityEngine.UI;

namespace DataStorageSolutions.Model
{
    internal class RackSlot
    {
        private GameObject _dummy;
        private Text _counter;
        private bool _isInstantiated;
        private StringBuilder sb = new StringBuilder();
        internal readonly int Id;
        internal readonly Transform Slot;
        private DSSRackController _mono;
        private bool _isOccupied;
        private List<ObjectData> _server;

        internal bool IsOccupied
        {
            get => _isOccupied;
            set
            {
                _isOccupied = value;
                ChangeDummyState(value);
            }
        }

        internal List<ObjectData> Server
        {
            get => _server;
            set
            {
                _server = value;
                UpdateNetwork();
            }
        }

        private void ChangeDummyState(bool b = true)
        {
            if(_dummy == null)
            {
                InstantiateDummy();
            }

            if (_dummy.activeSelf != b)
            {
                _dummy.SetActive(b);
            }
        }

        private void UpdateScreen()
        {
            _counter.text = $"{Server?.Count}/{QPatch.Configuration.Config.ServerStorageLimit}";
        }

        private void OnInterfaceButton(bool obj)
        {
            //Not sure if needed anymore 
            //TODO Remove
        }

        private string FormatData()
        {
            sb.Clear();

            var lookup = Server?.Where(x => x != null).ToLookup(x => x.TechType);

            if (lookup == null) return sb.ToString();
            {
                foreach (var data in lookup)
                {
                    if (data.All(objectData => objectData.TechType != data.Key)) continue;
                    sb.Append($"{Language.main.Get(data.Key)} x{data.Count()}");
                    sb.Append(Environment.NewLine);
                }
            }

            return sb.ToString();
        }

        internal RackSlot(DSSRackController controller, int id, Transform slot)
        {
            _mono = controller;
            Id = id;
            Slot = slot;
        }
        
        internal void Clear()
        {
            Server = null;
        }

        internal void Add(ObjectData data)
        {
            Server.Add(data);
            UpdateNetwork();
        }

        internal void Remove(ObjectData data)
        {
            Server.Remove(data);
            UpdateNetwork();
        }
        
        internal bool FindAllComponents()
        {
            try
            {
                #region Canvas  
                var canvasGameObject = _dummy.gameObject.GetComponentInChildren<Canvas>()?.gameObject;

                if (canvasGameObject == null)
                {
                    QuickLogger.Error("Canvas cannot be found");
                    return false;
                }
                #endregion

                #region Counter

                _counter = canvasGameObject.GetComponentInChildren<Text>();
                #endregion

                #region Hit

                var interactionFace = InterfaceHelpers.FindGameObject(canvasGameObject, "Hit");
                var catcher = interactionFace.AddComponent<InterfaceButton>();
                catcher.ButtonMode = InterfaceButtonMode.TextColor;
                catcher.OnInterfaceButton += OnInterfaceButton;
                catcher.TextLineOne = string.Format(AuxPatchers.TakeServer(),Mod.ServerFriendlyName);
                catcher.TextLineTwo = "Data: {0}";
                catcher.GetAdditionalDataFromString = true;
                catcher.GetAdditionalString = FormatData;
                catcher.BtnName = "ServerClick";
                catcher.OnButtonClick += OnButtonClick;

                #endregion

                return true;
            }
            catch (Exception e)
            {
                QuickLogger.Error($"{e.Message}: {e.StackTrace}");
                return false;
            }
        }

        private void OnButtonClick(string arg1, object arg2)
        {
            if (_mono.GetRackCageState())
            {
                var result = _mono.GivePlayerItem(QPatch.Server.TechType,new ObjectDataTransferData{data = Server,IsServer = true});
                QuickLogger.Debug($"Give Player ITem Result: {result}",true);
                if(result)
                    DisconnectFromRack();
            }
        }
        
        internal void InstantiateDummy()
        {
            if (_isInstantiated) return;

            _dummy = Slot.Find("Server")?.gameObject;
                
            if (FindAllComponents())
            {
                UpdateScreen();
            }

            _isInstantiated = true;
        }
        
        internal bool IsFull()
        {
            return Server != null && Server.Count >= QPatch.Configuration.Config.ServerStorageLimit;
        }

        internal void DisconnectFromRack()
        {
            IsOccupied = false;
            Clear();
            _mono.DisplayManager.UpdateContainerAmount();
            Mod.OnBaseUpdate?.Invoke();
        }
        
        internal void UpdateNetwork()
        {
            UpdateScreen();
            Mod.OnContainerUpdate?.Invoke();
        }
    }
}