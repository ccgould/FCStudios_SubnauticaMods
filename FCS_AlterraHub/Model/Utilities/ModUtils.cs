#if SUBNAUTICA_STABLE
using Oculus.Newtonsoft.Json;
using ErrorEventArgs = Oculus.Newtonsoft.Json.Serialization.ErrorEventArgs;
#else
using Newtonsoft.Json;
using ErrorEventArgs = Newtonsoft.Json.Serialization.ErrorEventArgs;
#endif

using UnityEngine;
using System;
using System.IO;
using System;
using System.Collections;
using System.IO;



namespace FCS_AlterraHub.Model.Utilities
{
    public static class ModUtils
    {

        private static MonoBehaviour _coroutineObject;

        
        public static void Save<SaveDataT>(SaveDataT newSaveData, string fileName, string saveDirectory, Action onSaveComplete = null)
        {
            if (newSaveData != null)
            {
                var settings = new JsonSerializerSettings {
                    NullValueHandling = NullValueHandling.Ignore,
                    //TypeNameHandling = TypeNameHandling.Auto
                };
                var saveDataJson = JsonConvert.SerializeObject(newSaveData, Formatting.Indented,settings);
                

                if (!Directory.Exists(saveDirectory))
                {
                    Directory.CreateDirectory(saveDirectory);
                }

                File.WriteAllText(Path.Combine(saveDirectory, fileName), saveDataJson);

                onSaveComplete?.Invoke();
            }
        }

        private static void HandleDeserializationError(object sender, ErrorEventArgs errorArgs)
        {
            var currentError = errorArgs.ErrorContext.Error.Message;
            errorArgs.ErrorContext.Handled = true;
        }

        public static void LoadSaveData<TSaveData>(string fileName, string saveDirectory, Action<TSaveData> onSuccess) where TSaveData : new()
        {
            var path = Path.Combine(saveDirectory, fileName);
            
            if (!File.Exists(path))
            {
                return;
            }
            var save = File.ReadAllText(path);

            var jsonSerializerSettings = new JsonSerializerSettings
            {
                MissingMemberHandling = MissingMemberHandling.Ignore,
                NullValueHandling = NullValueHandling.Ignore,
                Error = HandleDeserializationError

                //TypeNameHandling = TypeNameHandling.Auto
            };

            var json = JsonConvert.DeserializeObject<TSaveData>(save, jsonSerializerSettings);
            onSuccess?.Invoke(json);
        }

        private static Coroutine StartCoroutine(IEnumerator coroutine)
        {
            if (_coroutineObject == null)
            {
                var go = new GameObject();
                _coroutineObject = go.AddComponent<ModSaver>();
            }

            return _coroutineObject.StartCoroutine(coroutine);
        }
    }
}
