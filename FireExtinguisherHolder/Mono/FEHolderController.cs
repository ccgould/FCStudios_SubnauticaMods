using MAC.FireExtinguisherHolder.Buildable;
using MAC.FireExtinguisherHolder.Config;
using System.Collections;
using FCSCommon.Helpers;
using FCSCommon.Utilities;
using UnityEngine;


namespace MAC.FireExtinguisherHolder.Mono
{
    internal class FEHolderController : HandTarget, IHandTarget, IConstructable, IProtoEventListener
    {
        #region Private Members   
        private GameObject _tankMesh;
        private bool _initialized;
        private bool _hasTank = false;
        private float _fuel = 100f;
        private FEHolderSaveDataEntry _saveData;
        private bool _fromSave;
        private bool _runStartUpOnEnable;

        #endregion

        #region Internal Properties
        internal bool IsConstructed { get; private set; }

        #endregion


        private void OnEnable()
        {

            if (!_runStartUpOnEnable) return;

            Setup();

            if (_fromSave)
            {
                QuickLogger.Info("Loading FEExtinguishers");
                var prefabIdentifier = GetComponent<PrefabIdentifier>();
                var id = prefabIdentifier?.Id ?? string.Empty;
                var data = Mod.GetSaveData(id);

                if (data == null) return;

                _fuel = data.Fuel;
                _hasTank = data.HasTank;

                if (_tankMesh != null)
                {
                    StartCoroutine(ShowTank(data));
                }
                else
                {
                    QuickLogger.Error("Tank Mesh Not Found");
                }

                _fromSave = false;

                QuickLogger.Info("Loaded FEExtinguishers");
            }
        }

        private void Setup()
        {
            if (!_initialized)
            {
                if (!FindComponents())
                {
                    _initialized = false;
                    return;
                }
                _tankMesh.SetActive(_hasTank);
                _initialized = true;
            }
        }

        #region IHandTarget
        public void OnHandHover(GUIHand hand)
        {
            HandReticle main = HandReticle.main;

            if (!_hasTank)
            {
                main.SetInteractText(LanguageHelpers.GetLanguage(FEHolderBuildable.OnHandOverEmpty()));
            }
            else
            {
                main.SetInteractText(LanguageHelpers.GetLanguage(FEHolderBuildable.OnHandOverNotEmpty()), $"Fire Extinguisher: {Mathf.RoundToInt(_fuel)}%");
            }

            main.SetIcon(HandReticle.IconType.Hand, 1f);
        }

        public void OnHandClick(GUIHand hand)
        {
            if (_hasTank)
            {
                TakeTank();
            }
            else
            {
                TryStoreTank();
            }
        }
        #endregion

        #region Private Methods 
        /// <summary>
        /// A method that takes the tank from the player and stores it in the holder.
        /// </summary>
        private void TryStoreTank()
        {
            Pickupable pickupable = Inventory.main.container.RemoveItem(TechType.FireExtinguisher);

            if (pickupable != null)
            {
                FireExtinguisher component = pickupable.GetComponent<FireExtinguisher>();
                if (component != null)
                {
                    _fuel = component.fuel;
                }
                _hasTank = true;
                _tankMesh.SetActive(true);
            }
        }

        /// <summary>
        /// A method that takes the tank from holder and gives it to the player.
        /// </summary>
        private void TakeTank()
        {
            var size =  CraftData.GetItemSize(TechType.FireExtinguisher);
            
            if (!Inventory.main.HasRoomFor(size.x, size.y))
            {
                QuickLogger.Message(Language.main.Get("InventoryFull"));
                return;
            }

            CraftData.AddToInventory(TechType.FireExtinguisher, 1, false, false);

            _hasTank = false;
            _tankMesh.SetActive(false);
            gameObject.GetComponent<FireExtinguisher>().fuel = _fuel;
        }

        /// <summary>
        /// Finds all the gameObjects in the prefab for use
        /// </summary>
        /// <returns></returns>
        private bool FindComponents()
        {
            var objectName = "fire_extinguisher_tube_01";
            var tank = gameObject.FindChild("model").FindChild(objectName);

            if (tank == null)
            {
                QuickLogger.Error($"Cant find {objectName} on the prefab");
                return false;
            }

            _tankMesh = tank;
            return true;
        }

        /// <summary>
        /// Tries to display tank on load.
        /// </summary>
        /// <param name="data">Save data</param>
        /// <returns></returns>
        private IEnumerator ShowTank(FEHolderSaveDataEntry data)
        {
            if (!data.HasTank) yield break;

            while (!_tankMesh.activeSelf)
            {
                _tankMesh.SetActive(true);
                yield return null;
            }
        }
        #endregion

        #region IConstructable
        public bool CanDeconstruct(out string reason)
        {
            if (_hasTank)
            {
                reason = FEHolderBuildable.HolderNotEmptyMessage();
                return false;
            }

            reason = string.Empty;
            return true;
        }

        public void OnConstructedChanged(bool constructed)
        {
            IsConstructed = constructed;

            if (constructed)
            {
                if (isActiveAndEnabled)
                {
                    Setup();
                }
                else
                {
                    _runStartUpOnEnable = true;
                }
            }
        }
        #endregion

        #region IProtoEventListener
        internal void Save(FEHolderSaveData saveDataList)
        {
            var prefabIdentifier = GetComponent<PrefabIdentifier>();
            var id = prefabIdentifier.Id;

            if (_saveData == null)
            {
                _saveData = new FEHolderSaveDataEntry();
            }

            _saveData.ID = id;
            _saveData.Fuel = _fuel;
            _saveData.HasTank = _hasTank;
            saveDataList.Entries.Add(_saveData);
        }

        public void OnProtoSerialize(ProtobufSerializer serializer)
        {
            if (!Mod.IsSaving())
            {
                QuickLogger.Info("Saving FEExtinguishers");
                Mod.Save();
                QuickLogger.Info("Saved FEExtinguishers");
            }
        }

        public void OnProtoDeserialize(ProtobufSerializer serializer)
        {
            _fromSave = true;
        }
        #endregion
    }
}
