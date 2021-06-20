using UnityEngine;

namespace FCS_AlterraHub.Helpers
{
    public static class FModHelpers
    {
        public static FMOD_CustomEmitter CreateCustomEmitter(GameObject go, string name, string eventPath, bool followParent = true, bool restartOnPlay = true, bool playOnAwake = false)
        {
            var customEmitter = go.EnsureComponent<FMOD_CustomEmitter>();
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

        public static FMOD_CustomEmitter CreateCustomLoopingEmitter(GameObject go, string name,string eventPath, bool followParent = true, bool restartOnPlay = true, bool playOnAwake = false)
        {
            var customEmitter = go.EnsureComponent<FMOD_CustomLoopingEmitter>();

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
    }
}
