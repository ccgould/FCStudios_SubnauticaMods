using FMOD;

namespace FCS_HomeSolutions.Structs
{
    public struct SoundEntry
    {
        public string Message { get; set; }
        public Sound Sound { get; set; }
        public bool IsRandom { get; set; }
    }
}