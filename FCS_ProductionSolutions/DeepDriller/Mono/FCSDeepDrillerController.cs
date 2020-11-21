using FCS_AlterraHub.Enumerators;
using FCS_AlterraHub.Extensions;
using FCS_AlterraHub.Mono;
using FCS_AlterraHub.Registration;
using FCS_ProductionSolutions.Configuration;
using FCS_ProductionSolutions.DeepDriller.Buildable;
using FCS_ProductionSolutions.DeepDriller.Managers;
using FCS_ProductionSolutions.DeepDriller.Models;
using FCSCommon.Controllers;
using FCSCommon.Helpers;
using FCSCommon.Utilities;
using System.Collections;
using System.Collections.Generic;
using FCS_ProductionSolutions.Buildable;
using SMLHelper.V2.Assets;
using UnityEngine;


namespace FCS_ProductionSolutions.DeepDriller.Mono
{
    [RequireComponent(typeof(LineRenderer))]
    internal class FCSDeepDrillerController : FcsDevice, IFCSSave<SaveData>
    {
        #region Private Members

        private DeepDrillerMk2SaveDataEntry _mk2SaveData;
        private Constructable _buildable;
        private List<TechType> _bioData = new List<TechType>();
        private bool _runStartUpOnEnable;
        private GameObject _laser;
        private bool _allPlateFormsFound;
        private bool PlatformLegsExtended;
        private const int Segments = 50;
        private LineRenderer _line;
        private const float LerpSpeed = 10f;
        private bool _isRangeVisible;
        private float _currentDistance;
        private bool _noBiomeMessageSent;
        private bool _biomeFoundMessageSent;


        internal string CurrentBiome { get; set; }
        internal bool IsFromSave { get; set; }

        #endregion

        #region Internal Properties
        internal bool IsBeingDeleted { get; set; }
        internal FCSDeepDrillerAnimationHandler AnimationHandler { get; private set; }
        internal FCSDeepDrillerLavaPitHandler LavaPitHandler { get; private set; }
        internal FCSDeepDrillerContainer DeepDrillerContainer { get; private set; }
        public override bool IsConstructed { get; set; }
        internal AudioManager AudioManager { get; private set; }
        public  FCSDeepDrillerPowerHandler DeepDrillerPowerManager { get;  set; }
        internal FCSDeepDrillerDisplay DisplayHandler { get; private set; }
        internal int SolarStateHash { get; private set; }
        internal FCSDeepDrillerOreGenerator OreGenerator { get; private set; }
        internal FCSDeepDrillerOilHandler OilHandler { get; set; }
        internal LaserManager LaserManager { get; private set; }
        internal DumpContainer PowercellDumpContainer { get; set; }
        internal DumpContainer OilDumpContainer { get; set; }
        public ColorManager ColorManager { get; private set; }
        public FCSDeepDrillerUpgradeManager UpgradeManager { get; private set; }
        public DeepDrillerMk2Config Config => QPatch.DeepDrillerMk2Configuration;
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

                if (_mk2SaveData == null)
                {
                    ReadySaveData();
                }

                if (IsFromSave && _mk2SaveData != null)
                {
                    DeepDrillerPowerManager.LoadData(_mk2SaveData);

                    DeepDrillerContainer.LoadData(_mk2SaveData.Items);
                    if (_mk2SaveData.IsFocused)
                    {
                        OreGenerator.SetIsFocus(_mk2SaveData.IsFocused);
                        OreGenerator.Load(_mk2SaveData.FocusOres);
                    }

                    ColorManager.ChangeColor(_mk2SaveData.BodyColor.Vector4ToColor());
                    CurrentBiome = _mk2SaveData.Biome;
                    OilHandler.SetOilTimeLeft(_mk2SaveData.OilTimeLeft);
                    UpgradeManager.Load(_mk2SaveData?.Upgrades);
                    OreGenerator.SetBlackListMode(_mk2SaveData.IsBlackListMode);
                    _isRangeVisible = _mk2SaveData.IsRangeVisible;
                }

                StartCoroutine(TryGetLoot());
                _runStartUpOnEnable = false;
            }
        }

        private void Update()
        {
            //if (_line == null) return;
            //if (_isRangeVisible)
            //{
            //    CreatePoints(Mathf.Clamp(_currentDistance + DayNightCycle.main.deltaTime * LerpSpeed * 1, 0, QPatch.DeepDrillerMk2Configuration.DrillExStorageRange));
            //}
            //else
            //{
            //    for (int i = 0; i < _line.positionCount; i++)
            //    {
            //        if (_line.GetPosition(i) != Vector3.zero)
            //            _line.SetPosition(i, Vector3.zero);
            //        _currentDistance = 0f;
            //    }
            //}
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

        public void Save(SaveData saveDataList, ProtobufSerializer serializer = null)
        {
            if (!IsInitialized || _mk2SaveData == null || ColorManager == null || DeepDrillerPowerManager == null ||
                DeepDrillerContainer == null || OreGenerator == null || OilHandler == null || 
                UpgradeManager == null || TransferManager == null)
            {
                QuickLogger.Error($"Failed to save driller {GetPrefabID()}");
                return;
            }

            QuickLogger.Message($"SaveData = {_mk2SaveData}", true);

            _mk2SaveData.Id = GetPrefabID();
            _mk2SaveData.BodyColor = ColorManager.GetColor().ColorToVector4();

            _mk2SaveData.PowerState = DeepDrillerPowerManager.GetPowerState();
            _mk2SaveData.PullFromRelay = DeepDrillerPowerManager.GetPullFromPowerRelay();
            _mk2SaveData.SolarExtended = DeepDrillerPowerManager.IsSolarExtended();
            _mk2SaveData.PowerData = DeepDrillerPowerManager.SaveData();

            _mk2SaveData.Items = DeepDrillerContainer.SaveData();

            _mk2SaveData.FocusOres = OreGenerator.GetFocuses();
            _mk2SaveData.IsFocused = OreGenerator.GetIsFocused();
            _mk2SaveData.IsBlackListMode = OreGenerator.GetInBlackListMode();

            _mk2SaveData.Biome = CurrentBiome;

            _mk2SaveData.OilTimeLeft = OilHandler.GetOilTimeLeft();

            _mk2SaveData.Upgrades = UpgradeManager.Save();

            _mk2SaveData.IsRangeVisible = _isRangeVisible;

            _mk2SaveData.AllowedToExport = TransferManager.IsAllowedToExport();
            saveDataList.DeepDrillerMk2Entries.Add(_mk2SaveData);

        }

        public override void OnProtoSerialize(ProtobufSerializer serializer)
        {
            if (!Mod.IsSaving())
            {
                QuickLogger.Info("Saving Drills");
                Mod.Save();
            }
        }

        public override void OnProtoDeserialize(ProtobufSerializer serializer)
        {
            IsFromSave = true;
        }

        #endregion

        #region Private Methods
        
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
            
            InvokeRepeating(nameof(UpdateDrillShaftSate), 1, 1);

            if (OilHandler == null)
            {
                OilHandler = gameObject.AddComponent<FCSDeepDrillerOilHandler>();
                OilHandler.Initialize(this);
            }

            if (_buildable == null)
            {
                _buildable = GetComponentInParent<Constructable>();
            }

            OreGenerator = gameObject.AddComponent<FCSDeepDrillerOreGenerator>();
            OreGenerator.Initialize(this);
            OreGenerator.OnAddCreated += OreGeneratorOnAddCreated;

            if (DeepDrillerPowerManager == null)
            {
                DeepDrillerPowerManager = gameObject.AddComponent<FCSDeepDrillerPowerHandler>();
                DeepDrillerPowerManager.Initialize(this);
                var powerRelay = gameObject.AddComponent<PowerRelay>();
                DeepDrillerPowerManager.SetPowerRelay(powerRelay);
            }
            
            if (LaserManager == null)
            {
                LaserManager = new LaserManager();
                LaserManager.Setup(this);
            }

            if (ColorManager == null)
            {
                ColorManager = gameObject.AddComponent<ColorManager>();
                ColorManager.Initialize(gameObject, ModelPrefab.BodyMaterial);
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
                    OilHandler, 4, 4);
            }

            if (PowercellDumpContainer == null)
            {
                PowercellDumpContainer = gameObject.AddComponent<DumpContainer>();
                PowercellDumpContainer.Initialize(transform,
                    FCSDeepDrillerBuildable.PowercellDumpContainerTitle(),
                    DeepDrillerPowerManager, 1, 1);
            }

            if (TransferManager == null)
            {
                TransferManager = gameObject.AddComponent<FCSDeepDrillerTransferManager>();
                TransferManager.Initialize(this);
            }

            if (UpgradeManager == null)
            {
                UpgradeManager = gameObject.AddComponent<FCSDeepDrillerUpgradeManager>();
                UpgradeManager.Initialize(this);
            }

            _line = gameObject.GetComponent<LineRenderer>();
            _line.SetVertexCount(Segments + 1);
            _line.useWorldSpace = false;

            //if (FCSConnectableDevice == null)
            //{
            //    FCSConnectableDevice = gameObject.AddComponent<FcsDevice>();
            //    FCSConnectableDevice.Initialize(this,DeepDrillerContainer,DeepDrillerPowerManager);
            //    FCSTechFabricator.FcTechFabricatorService.PublicAPI.RegisterDevice(FCSConnectableDevice, GetPrefabID(),Mod.DeepDrillerTabID);
            //}

            FCSAlterraHubService.PublicAPI.RegisterDevice(this,Mod.DeepDrillerMk2TabID);

            OnGenerate();

            IsInitialized = true;

            QuickLogger.Debug($"Initializing Completed");
        }

        public FCSDeepDrillerTransferManager TransferManager { get; set; }

        private void UpdateDrillShaftSate()
        {
            if (IsOperational)
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
            //if (TransferManager.IsAllowedToExport())
            //{
            //    var result = TransferManager.TransferToExStorage(type);
            //    if (result)
            //    {
            //        return;
            //    }
            //}
            DeepDrillerContainer.AddItemToContainer(type);
        }

        private void ReadySaveData()
        {
            QuickLogger.Debug("In OnProtoDeserialize");
            var prefabIdentifier = GetComponentInParent<PrefabIdentifier>() ?? GetComponent<PrefabIdentifier>();
            var id = prefabIdentifier?.Id ?? string.Empty;
            _mk2SaveData = Mod.GetDeepDrillerMK2SaveData(id);
        }
        
        private IEnumerator TryGetLoot()
        {
            QuickLogger.Debug("In TryGetLoot");

            if (OreGenerator == null)
            {
                QuickLogger.Error("OreGenerator is null");
                yield return null;
            }


            while (OreGenerator?.AllowedOres.Count <= 0)
            {
                if (string.IsNullOrEmpty(CurrentBiome))
                {
                    if (!_noBiomeMessageSent)
                    {
                        QuickLogger.Info($"No biome Found trying to find biome");
                        _noBiomeMessageSent = true;
                    }

                    CurrentBiome = BiomeManager.GetBiome(gameObject.transform);
                }
                else
                {
                    if (!_biomeFoundMessageSent)
                    {
                        QuickLogger.Info($"biome Found: {CurrentBiome}");
                        _biomeFoundMessageSent = true;
                    }

                    var loot = GetBiomeData(CurrentBiome, transform);

                    if (loot != null)
                    {
                        OreGenerator.AllowedOres = loot;
                        ConnectDisplay();
                    }
                }
                yield return null;
            }
            yield return 0;
        }

        private void ConnectDisplay()
        {
            if (DisplayHandler != null) return;
            QuickLogger.Debug($"Creating Display");
            DisplayHandler = gameObject.AddComponent<FCSDeepDrillerDisplay>();
            DisplayHandler.Setup(this);
            DisplayHandler.UpdateBiome(CurrentBiome);
            DisplayHandler.OnIsFocusedChanged(_mk2SaveData.IsFocused);
            DisplayHandler.UpdateListItemsState(_mk2SaveData?.FocusOres ?? new HashSet<TechType>());
            if (_mk2SaveData.AllowedToExport)
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

        public override bool IsOperational => DeepDrillerPowerManager.HasEnoughPowerToOperate() &&
                   DeepDrillerPowerManager.GetPowerState() == FCSPowerStates.Powered &&
                   OilHandler.HasOil() && !DeepDrillerContainer.IsFull;
        

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