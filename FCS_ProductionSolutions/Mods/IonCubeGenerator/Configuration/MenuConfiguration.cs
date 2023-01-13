using SMLHelper.Options;


namespace FCS_ProductionSolutions.Mods.IonCubeGenerator.Configuration
{
    internal class MenuConfiguration : ModOptions
    {
        private const string EnableAudioID = "IONEnableAudio";


        internal MenuConfiguration() : base("ION Cube Generator Settings")
        {
            this.OnChanged += OnToggleChanged;
            AddItem(ModToggleOption.Factory(EnableAudioID, "Enable SFX", ModConfiguration.Singleton.AllowSFX));
        }

        private void OnToggleChanged(object sender, OptionEventArgs e)
        {
            switch (e.Id)
            {
                case EnableAudioID when e is ToggleChangedEventArgs arg:
                    ModConfiguration.Singleton.AllowSFX = arg.Value;
                    break;
            }

            ModConfiguration.Singleton.SaveModConfiguration();
        }
    }
}