using FCS_DeepDriller.Configuration;
using FCS_DeepDriller.Managers;
using FCS_DeepDriller.Mono.Handlers;
using FCSAlterraIndustrialSolutions.Models.Controllers.Logic;
using FCSCommon.Extensions;
using FCSCommon.Models;
using FCSCommon.Models.Components;
using FCSCommon.Objects;
using FCSCommon.Utilities;
using UnityEngine;

namespace FCS_DeepDriller.Mono
{
    [RequireComponent(typeof(WeldablePoint))]
    internal class FCSDeepDrillerController : MonoBehaviour, IConstructable, IProtoEventListener
    {

        private DeepDrillerSaveDataEntry saveData;
        private string _currentBiome;
        private PowerController _powerSystem;
        private HealthController _healthSystem;
        private OreGenerator _oreGenerator;
        private Constructable _buildable;
        private PrefabIdentifier _prefabId;

        internal bool IsBeingDeleted { get; set; }

        #region Unity Methods
        private void Awake()
        {
            _prefabId = GetComponentInParent<PrefabIdentifier>();

            if (_prefabId == null)
            {
                QuickLogger.Error("Prefab Identifier Component was not found");
            }

            if (_buildable == null)
            {
                _buildable = GetComponentInParent<Constructable>();
            }

            var unqiueLiveMixingData = new CustomLiveMixinData.UniqueLiveMixinData();
            var liveMixinData = unqiueLiveMixingData.Create(100, true, true, true);

            _healthSystem = gameObject.AddComponent<HealthController>();

            _oreGenerator = gameObject.AddComponent<OreGenerator>();
            _oreGenerator.Start(1, 2);
            _oreGenerator.OnAddCreated += OreGeneratorOnOnAddCreated;

            var liveMixin = gameObject.AddComponent<LiveMixin>();

            _healthSystem.Startup(liveMixin, liveMixinData);
            _healthSystem.FullHealth();

            DeepDrillerContainer = new FCSDeepDrillerContainer();
            DeepDrillerContainer.Setup(this);

            _powerSystem = gameObject.AddComponent<PowerController>();
            _powerSystem.CreateBattery(100, 2000);
        }

        internal FCSDeepDrillerContainer DeepDrillerContainer { get; private set; }
        internal bool IsConstructed => _buildable != null && _buildable.constructed;
        private void OreGeneratorOnOnAddCreated(string type)
        {
            QuickLogger.Info($"In OreGeneratorOnOnAddCreated {type}");
            DeepDrillerContainer.AddItem(type.ToPickupable());
        }

        private void Start()
        {

        }

        private void Update()
        {

        }

        private void OnDestroy()
        {

        }
        #endregion

        public void Save(DeepDrillerSaveData saveDataList)
        {
            var prefabIdentifier = GetComponent<PrefabIdentifier>();
            var id = prefabIdentifier.Id;

            if (saveData == null)
            {
                saveData = new DeepDrillerSaveDataEntry() { Id = id };
            }

            saveData.Id = id;
            saveDataList.Entries.Add(saveData);
        }

        #region IConstructable
        public bool CanDeconstruct(out string reason)
        {
            reason = string.Empty;
            return true;
        }

        public void OnConstructedChanged(bool constructed)
        {
            QuickLogger.Info("In Constructed Changed");

            if (IsBeingDeleted) return;

            if (constructed)
            {
                _currentBiome = BiomeManager.GetBiome();
            }
        }
        #endregion

        #region IProtoEventListener
        public void OnProtoSerialize(ProtobufSerializer serializer)
        {
            if (!Mod.IsSaving())
            {
                QuickLogger.Info("Saving Drills");
                Mod.SaveDeepDriller();
            }
        }

        public void OnProtoDeserialize(ProtobufSerializer serializer)
        {
            var prefabIdentifier = GetComponent<PrefabIdentifier>();
            var id = prefabIdentifier?.Id ?? string.Empty;
            var data = Mod.GetDeepDrillerSaveData(id);
        }
        #endregion
    }
}
