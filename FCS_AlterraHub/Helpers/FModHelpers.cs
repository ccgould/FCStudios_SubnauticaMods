using UnityEngine;

namespace FCS_AlterraHub.Helpers
{
    public static class FModHelpers
    {
        public static FMOD_CustomEmitter CreateCustomEmitter(GameObject go, string name, string eventPath, bool followParent = true, bool restartOnPlay = true, bool playOnAwake = false)
        {
            var customEmitter = go.AddComponent<FMOD_CustomEmitter>();
            if (customEmitter.asset == null)
            {
                var fmodAsset = ScriptableObject.CreateInstance<FMODAsset>();
                fmodAsset.id = name;
                fmodAsset.path = eventPath;
                customEmitter.restartOnPlay = restartOnPlay;
                customEmitter.playOnAwake = playOnAwake;
                customEmitter.followParent = followParent;
                customEmitter.asset = fmodAsset;
            }
            
            return customEmitter;
        }

        public static FMOD_CustomLoopingEmitter CreateCustomLoopingEmitter(GameObject go, string name,string eventPath, bool followParent = true, bool restartOnPlay = true, bool playOnAwake = false)
        {
            var customEmitter = go.AddComponent<FMOD_CustomLoopingEmitter>();

            if (customEmitter.asset == null)
            {
                var fmodAsset = ScriptableObject.CreateInstance<FMODAsset>();
                fmodAsset.id = name;
                fmodAsset.path = eventPath;
                customEmitter.restartOnPlay = restartOnPlay;
                customEmitter.playOnAwake = playOnAwake;
                customEmitter.followParent = followParent;
                customEmitter.asset = fmodAsset;
            }
            return customEmitter;
        }

        public static FMODAsset CreateFmodAsset(string name, string eventPath,string id = "")
        {
            var result = ScriptableObject.CreateInstance<FMODAsset>();
            result.id = id;
            result.name = name;
            result.path = eventPath;
            return result;
        }
    }
}
