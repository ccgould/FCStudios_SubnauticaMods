namespace FCS_AlterraHub.Mono.FCSPDA.Mono
{
    internal class AudioMessage    
    {
        public string Description { get; set; }
        public string AudioClipName { get; set; }
        public bool HasBeenPlayed { get; set; }
        public AudioMessage(string description,string audioClipName)
        {
            AudioClipName = audioClipName;
            Description = description;
        }
    }
}