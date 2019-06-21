using FCSCommon.Enums;
using FCSCommon.Utilities;
using FCSTechWorkBench.Models;
using Oculus.Newtonsoft.Json;
using System.IO;

namespace FCSTechWorkBench.Mono
{
    public class LongTermFilterController : Filter, IProtoTreeEventListener
    {
        public override PrefabIdentifier PrefabId { get; set; }
        public override FilterTypes FilterType { get; set; } = FilterTypes.LongTermFilter;

        public override float GetMaxTime()
        {
            return MaxTime;
        }

        public override void StartTimer()
        {
            if (RunTimer) return;
            RunTimer = true;
            QuickLogger.Debug($"RunTimer was Set to {RunTimer}", true);
        }

        public override void StopTimer()
        {
            if (!RunTimer) return;
            RunTimer = false;
            QuickLogger.Debug($"Runtimer was Set to {RunTimer}", true);
        }

        public override void Initialize(bool fromSave = false)
        {
            PrefabId = gameObject.GetComponent<PrefabIdentifier>();
            FromSave = fromSave;
            SetMaxTime();
        }

        public override string GetRemainingTime()
        {
            return RemainingTime;
        }

        #region IPhotoTreeEventListener
        public void OnProtoSerializeObjectTree(ProtobufSerializer serializer)
        {
            QuickLogger.Debug($"Saving {PrefabId.Id} Data");

            if (!Directory.Exists(_saveDirectory))
                Directory.CreateDirectory(_saveDirectory);

            var saveData = new FilterSaveData
            {
                FilterType = FilterType,
                RemainingTime = MaxTime,
                FilterState = FilterState
            };

            var output = JsonConvert.SerializeObject(saveData, Formatting.Indented);
            File.WriteAllText(SaveFile, output);

            QuickLogger.Debug($"Saved {PrefabId.Id} Data");
        }

        public void OnProtoDeserializeObjectTree(ProtobufSerializer serializer)
        {
            QuickLogger.Debug("// ****************************** Load Data *********************************** //");

            if (PrefabId != null)
            {
                QuickLogger.Info($"Loading Long Term Filter: {PrefabId.Id}");

                if (File.Exists(SaveFile))
                {
                    string savedDataJson = File.ReadAllText(SaveFile).Trim();

                    //LoadData
                    var savedData = JsonConvert.DeserializeObject<FilterSaveData>(savedDataJson);
                    FilterType = savedData.FilterType;
                    MaxTime = savedData.RemainingTime;
                    FilterState = savedData.FilterState;
                }
            }
            else
            {
                QuickLogger.Error("PrefabIdentifier is null");
            }
            QuickLogger.Debug("// ****************************** Loaded Data *********************************** //");
        }
        #endregion
    }
}
