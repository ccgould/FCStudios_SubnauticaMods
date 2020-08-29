using System.Collections;
using System.Collections.Generic;
using FCS_DeepDriller.Buildable.MK2;
using FCS_DeepDriller.Configuration;
using FCS_DeepDriller.Managers;
using FCS_DeepDriller.Model;
using FCSCommon.Controllers;
using FCSCommon.Helpers;
using FCSCommon.Utilities;
using FCSTechFabricator.Abstract;
using FCSTechFabricator.Components;
using FCSTechFabricator.Enums;
using FCSTechFabricator.Extensions;
using FCSTechFabricator.Managers;
using UnityEngine;

namespace FCS_DeepDriller.Mono.MK2
{
    [RequireComponent(typeof(LineRenderer))]
    internal class FCSDeepDrillerController : FCSController
    {
        #region Private Members
        private DeepDrillerSaveDataEntry _saveData;
        private Constructable _buildable;
        private PrefabIdentifier _prefabId;
        private List<TechType> _bioData = new List<TechType>();
        private bool _runStartUpOnEnable;
        private bool _fromSave;
        private DeepDrillerSaveDataEntry _data;
        private GameObject _laser;
        private bool _allPlateFormsFound;
        private bool PlatformLegsExtended;
        private const int Segments = 50;
        private LineRenderer _line;
        private const float LerpSpeed = 10f;
        private bool _isRangeVisible;
        private float _currentDistance;


        internal string CurrentBiome { get; set; }

        #endregion

        #region Internal Properties
        internal bool IsBeingDeleted { get; set; }
        internal FCSDeepDrillerAnimationHandler AnimationHandler { get; private set; }
        internal FCSDeepDrillerLavaPitHandler LavaPitHandler { get; private set; }
        internal FCSDeepDrillerContainer DeepDrillerContainer { get; private set; }
        public override bool IsConstructed { get; set; }
        internal AudioManager AudioManager { get; private set; }
        internal FCSDeepDrillerPowerHandler PowerManager { get; private set; }
        internal FCSDeepDrillerDisplay DisplayHandler { get; private set; }
        internal int SolarStateHash { get; private set; }
        internal OreGenerator OreGenerator { get; private set; }
        internal FCSDeepDrillerOilHandler OilHandler { get; set; }
        internal LaserManager LaserManager { get; private set; }
        internal DumpContainer PowercellDumpContainer { get; set; }
        internal DumpContainer OilDumpContainer { get; set; }
        public ColorManager ColorManager { get; private set; }
        public UpgradeManager UpgradeManager { get; private set; }

        #endregion

        #region Unity Methods

        private void OnDestroy()
        {
            IsBeingDeleted = true;
        }

        private void OnEnable()
        {
            if (IsBeingDeleted) return;

            if (_runStartUpOnEnable)
            {

                if (!IsInitialized)
                {
                    Initialize();
                }

                if (_data == null)
                {
                    ReadySaveData();
                }

                if (_fromSave && _data != null)
                {
                    PowerManager.LoadData(_data);

                    DeepDrillerContainer.LoadData(_data.Items);
                    if (_data.IsFocused)
                    {
                        OreGenerator.SetIsFocus(_data.IsFocused);
                        OreGenerator.Load(_data.FocusOres);
                    }

                    ColorManager.SetMaskColorFromSave(_data.BodyColor.Vector4ToColor());
                    CurrentBiome = _data.Biome;
                    OilHandler.SetOilTimeLeft(_data.OilTimeLeft);
                    UpgradeManager.Load(_data?.Upgrades);
                    _isRangeVisible = _data.IsRangeVisible;
                    _fromSave = false;
                }

                StartCoroutine(TryGetLoot());
                _runStartUpOnEnable = false;
            }
        }
        
        #endregion

        #region IConstructable
        public override bool CanDeconstruct(out string reason)
        {
            reason = string.Empty;

            if (IsInitialized == false)
            {
                return true;
            }

            if (DeepDrillerContainer.HasItems())
            {
                reason = FCSDeepDrillerBuildable.RemoveAllItems();
                return false;
            }

            return true;
        }

        public override void OnConstructedChanged(bool constructed)
        { 
            QuickLogger.Info("In Constructed Changed");

            IsConstructed = constructed;

            if (constructed)
            {
                if (isActiveAndEnabled)
                {
                    if (!IsInitialized)
                    {
                        Initialize();
                    }

                    CurrentBiome = BiomeManager.GetBiome();
                    StartCoroutine(TryGetLoot());
                }
                else
                {
                    _runStartUpOnEnable = true;
                }
            }
        }
        #endregion

        #region IProtoEventListener

        internal void Save(DeepDrillerSaveData saveDataList)
        {
            var prefabIdentifier = GetComponent<PrefabIdentifier>();
            var id = prefabIdentifier.Id;

            if (_saveData == null)
            {
                _saveData = new DeepDrillerSaveDataEntry();
            }

            _saveData.Id = id;
            _saveData.BodyColor = ColorManager.GetMaskColor().ColorToVector4();
            _saveData.PowerState = PowerManager.GetPowerState();
            _saveData.PullFromRelay = PowerManager.GetPullFromPowerRelay();
            _saveData.Items = DeepDrillerContainer.SaveData();
            _saveData.PowerData = PowerManager.SaveData();
            _saveData.FocusOres = OreGenerator.GetFocuses();
            _saveData.IsFocused = OreGenerator.GetIsFocused();
            _saveData.Biome = CurrentBiome;
            _saveData.OilTimeLeft = OilHandler.GetOilTimeLeft();
            _saveData.SolarExtended = PowerManager.IsSolarExtended();
            _saveData.Upgrades = UpgradeManager.Save();
            _saveData.IsRangeVisible = _isRangeVisible;
            _saveData.AllowedToExport = TransferManager.IsAllowedToExport();
            saveDataList.Entries.Add(_saveData);
        }

        public override void OnProtoSerialize(ProtobufSerializer serializer)
        {
            if (!Mod.IsSaving())
            {
                QuickLogger.Info("Saving Drills");
                Mod.SaveDeepDriller();
            }
        }

        public override void OnProtoDeserialize(ProtobufSerializer serializer)
        {
            _fromSave = true;
        }

        #endregion

        #region Private Methods

        private void Update()
        {
            if(_line == null) return;
            if(_isRangeVisible)
            {
                CreatePoints(Mathf.Clamp(_currentDistance + DayNightCycle.main.deltaTime * LerpSpeed * 1, 0, QPatch.Configuration.DrillExStorageRange));
            }
            else
            {
                for (int i = 0; i < _line.positionCount; i++)
                {
                    if(_line.GetPosition(i) != Vector3.zero)
                        _line.SetPosition(i,Vector3.zero);
                    _currentDistance = 0f;
                }
            }
        }

        public override void Initialize()
        {
            QuickLogger.Debug($"Initializing");

            var listSolar = new List<GameObject>
            {
                GameObjectHelpers.FindGameObject(gameObject, "Cube_2_2"),
                GameObjectHelpers.FindGameObject(gameObject, "Cube_1_2"),
                GameObjectHelpers.FindGameObject(gameObject, "Cube_1"),
                GameObjectHelpers.FindGameObject(gameObject, "Cube_2"),
                GameObjectHelpers.FindGameObject(gameObject, "Cube_2_3"),
                GameObjectHelpers.FindGameObject(gameObject, "Cube_1_4"),
                GameObjectHelpers.FindGameObject(gameObject, "Cube_2_4")
            };

            foreach (GameObject solarObj in listSolar)
            {
                var sController = solarObj.AddComponent<FCSDeepDrillerSolarController>();
                sController.Setup(this);
            }
            
            SolarStateHash = Animator.StringToHash("SolarState");

            _prefabId = GetComponentInParent<PrefabIdentifier>() ?? GetComponent<PrefabIdentifier>();

            InvokeRepeating(nameof(UpdateDrillShaftSate), 1, 1);

            if (OilHandler == null)
            {
                OilHandler = gameObject.AddComponent<FCSDeepDrillerOilHandler>();
                OilHandler.Initialize(this);
            }

            if (_prefabId == null)
            {
                QuickLogger.Error("Prefab Identifier Component was not found");
            }

            if (_buildable == null)
            {
                _buildable = GetComponentInParent<Constructable>();
            }

            OreGenerator = gameObject.AddComponent<OreGenerator>();
            OreGenerator.Initialize(this);
            OreGenerator.OnAddCreated += OreGeneratorOnAddCreated;

            if (PowerManager == null)
            {
                PowerManager = gameObject.AddComponent<FCSDeepDrillerPowerHandler>();
                PowerManager.Initialize(this);
                var powerRelay = gameObject.AddComponent<PowerRelay>();
                PowerManager.SetPowerRelay(powerRelay);
            }
            
            if (LaserManager == null)
            {
                LaserManager = new LaserManager();
                LaserManager.Setup(this);
            }

            if (ColorManager == null)
            {
                ColorManager = gameObject.AddComponent<ColorManager>();
                ColorManager.Initialize(gameObject, FCSDeepDrillerBuildable.BodyMaterial);
            }

            AudioManager = new AudioManager(gameObject.GetComponent<FMOD_CustomLoopingEmitter>());

            DeepDrillerContainer = new FCSDeepDrillerContainer();
            DeepDrillerContainer.Setup(this);


            AnimationHandler = gameObject.AddComponent<FCSDeepDrillerAnimationHandler>();
            AnimationHandler.Initialize(this);

            LavaPitHandler = gameObject.AddComponent<FCSDeepDrillerLavaPitHandler>();
            LavaPitHandler.Initialize(this);

            if (OilDumpContainer == null)
            {
                OilDumpContainer = gameObject.AddComponent<DumpContainer>();
                OilDumpContainer.Initialize(transform,
                    FCSDeepDrillerBuildable.OilDropContainerTitle(),
                    FCSDeepDrillerBuildable.NotAllowedItem(),
                    FCSDeepDrillerBuildable.StorageFull(),
                    OilHandler, 4, 4);
            }

            if (PowercellDumpContainer == null)
            {
                PowercellDumpContainer = gameObject.AddComponent<DumpContainer>();
                PowercellDumpContainer.Initialize(transform,
                    FCSDeepDrillerBuildable.PowercellDumpContainerTitle(),
                    FCSDeepDrillerBuildable.NotAllowedItem(),
                    FCSDeepDrillerBuildable.StorageFull(),
                    PowerManager, 1, 1);
            }

            if (TransferManager == null)
            {
                TransferManager = gameObject.AddComponent<FCSDeepDrillerTransferManager>();
                TransferManager.Initialize(this);
            }

            if (UpgradeManager == null)
            {
                UpgradeManager = gameObject.AddComponent<UpgradeManager>();
                UpgradeManager.Initialize(this);
            }

            _line = gameObject.GetComponent<LineRenderer>();
            _line.SetVertexCount(Segments + 1);
            _line.useWorldSpace = false;

            OnGenerate();

            IsInitialized = true;

            QuickLogger.Debug($"Initializing Completed");
        }

        public FCSDeepDrillerTransferManager TransferManager { get; set; }

        private void UpdateDrillShaftSate()
        {
            if (IsOperational())
            {
                LaserManager.ChangeLasersState();

                if (IsUnderWater())
                {
                    LaserManager.ChangeBubblesState();
                }

                AnimationHandler.DrillState(true);
                AudioManager.PlayAudio();
            }
            else
            {
                LaserManager.ChangeLasersState(false);
                LaserManager.ChangeBubblesState(false);
                AnimationHandler.DrillState(false);
                AudioManager.StopAudio();
            }
        }

        private void OreGeneratorOnAddCreated(TechType type)
        {
            if (TransferManager.IsAllowedToExport())
            {
                var result = TransferManager.TransferToExStorage(type);
                if (result)
                {
                    return;
                }
            }
            DeepDrillerContainer.AddItemToContainer(type);
        }

        private void ReadySaveData()
        {
            QuickLogger.Debug("In OnProtoDeserialize");
            var prefabIdentifier = GetComponentInParent<PrefabIdentifier>() ?? GetComponent<PrefabIdentifier>();
            var id = prefabIdentifier?.Id ?? string.Empty;
            _data = Mod.GetDeepDrillerSaveData(id);
        }
        
        private IEnumerator TryGetLoot()
        {
            QuickLogger.Debug("In TryGetLoot");

            if (OreGenerator == null)
            {
                QuickLogger.Error("OreGenerator is null");
                yield return null;
            }

            while (OreGenerator.AllowedOres.Count <= 0)
            {
                var loot = GetBiomeData(CurrentBiome, transform);

                if (loot != null)
                {
                    OreGenerator.AllowedOres = loot;
                    ConnectDisplay();
                }

                yield return null;
            }
        }

        private void ConnectDisplay()
        {
            if (DisplayHandler != null) return;
            QuickLogger.Debug($"Creating Display");
            DisplayHandler = gameObject.AddComponent<FCSDeepDrillerDisplay>();
            DisplayHandler.Setup(this);
            DisplayHandler.UpdateBiome(CurrentBiome);
            DisplayHandler.OnIsFocusedChanged(_data.IsFocused);
            DisplayHandler.UpdateListItemsState(_data?.FocusOres ?? new HashSet<TechType>());
            if (_data.AllowedToExport)
            {
                TransferManager.Toggle();
            }
        }

        private void OnGenerate()
        {
            if (PlatformLegsExtended) return;

            QuickLogger.Debug("OnGenerate", true);
            if (!_allPlateFormsFound)
            {
                var pillar = GameObjectHelpers.FindGameObject(gameObject, "PlatfromLeg")?.AddComponent<Pillar>();
                pillar?.Instantiate(this);

                var pillar1 = GameObjectHelpers.FindGameObject(gameObject, "PlatfromLeg_2")?.AddComponent<Pillar>();
                pillar1?.Instantiate(this);

                var pillar2 = GameObjectHelpers.FindGameObject(gameObject, "PlatfromLeg_3")?.AddComponent<Pillar>();
                pillar2?.Instantiate(this);

                var pillar3 = GameObjectHelpers.FindGameObject(gameObject, "PlatfromLeg_4")?.AddComponent<Pillar>();
                pillar3?.Instantiate(this);

                var pillar4 = GameObjectHelpers.FindGameObject(gameObject, "PlatfromLeg_5")?.AddComponent<Pillar>();
                pillar4?.Instantiate(this);

                var pillar5 = GameObjectHelpers.FindGameObject(gameObject, "PlatfromLeg_6")?.AddComponent<Pillar>();
                pillar5?.Instantiate(this);

                var pillar6 = GameObjectHelpers.FindGameObject(gameObject, "DrillTunnelBase")?.AddComponent<Pillar>();
                pillar6?.Instantiate(this, true);
                _allPlateFormsFound = true;
            }
        }
        
        private void CreatePoints(float radius)
        {
            _currentDistance = radius;
            float x;
            float y;
            float z;

            float angle = 20;

            for (int i = 0; i < (Segments + 1); i++)
            {
                x = Mathf.Cos(Mathf.Deg2Rad * angle) * radius;
                z = Mathf.Sin(Mathf.Deg2Rad * angle) * radius;

                _line.SetPosition(i, new Vector3(x, 0, z));


                angle += (360f / Segments);
            }
        }

        #endregion

        #region Internal Methods

        internal bool IsUnderWater()
        {
            return GetDepth() >= 0.63f;
        }

        internal float GetDepth()
        {
#if SUBNAUTICA
            return gameObject == null ? 0f : Ocean.main.GetDepthOf(gameObject);
#elif BELOWZERO
            return gameObject == null ? 0f : Ocean.GetDepthOf(gameObject);
#endif
        }

        internal List<TechType> GetBiomeData(string biome = null, Transform tr = null)
        {
            if (_bioData?.Count <= 0 && tr != null || biome != null)
            {
                var data = BiomeManager.FindBiomeLoot(tr, biome);

                if (data != null)
                {
                    _bioData = data;
                    _bioData.Add(Mod.GetSandBagTechType());
                }
            }

            return _bioData;
        }
        
        internal bool IsOperational()
        {
            return PowerManager.HasEnoughPowerToOperate() &&
                   PowerManager.GetPowerState() == FCSPowerStates.Powered &&
                   OilHandler.HasOil() && !DeepDrillerContainer.IsFull;
        }

        internal bool GetIsRangeVisible()
        {
            return _isRangeVisible;
        }

        internal void ToggleRangeView()
        {
            _isRangeVisible = !_isRangeVisible;
        }

        #endregion
    }
}