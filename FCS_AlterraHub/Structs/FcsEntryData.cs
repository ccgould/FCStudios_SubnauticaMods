using UnityEngine;

namespace FCS_AlterraHub.Structs
{
    public struct FcsEntryData
    {
        public string key;
        public string path;
        public string[] nodes;
        public bool timeCapsule;
        public bool unlocked;
        public Sprite popup;
        public Texture2D image;
        public FMODAsset sound;
        public FMODAsset audio;
        public string Description;
        public string Title;
        public bool Verbose;
    }
}