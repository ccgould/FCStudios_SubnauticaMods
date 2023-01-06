using SMLHelper.Options;
using System.Collections.Generic;
using UnityEngine.UIElements;

namespace FCS_ProductionSolutions.Mods.IonCubeGenerator.Configuration
{
    internal class MenuConfiguration : ModOptions
    {
        private readonly ModToggleOption _allowsfx;

        //private ModModes _modMode;
        private const string EnableAudioID = "IONEnableAudio";
        //private const string AllowFoodToggle = "DSSAllowFood";


        internal MenuConfiguration() : base("ION Cube Generator Settings")
        {
            this.OnChanged += OnToggleChanged;
            _allowsfx = ModToggleOption.Factory(EnableAudioID, "Enable SFX", ModConfiguration.Singleton.AllowSFX);
            _allowsfx.OnChanged += OnToggleChanged;
            AddItem(_allowsfx);
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

        public override void BuildModOptions(uGUI_TabbedControlsPanel panel, int modsTabIndex, List<OptionItem> options)
        {
            panel.AddHeading(modsTabIndex,"FCS Mods");
            
            if(true) _allowsfx.AddToPanel(panel,modsTabIndex);
           // options.ForEach(x=>x.AddToPanel(panel,modsTabIndex));
            //base.BuildModOptions(panel, modsTabIndex, options);
        }
    }
}