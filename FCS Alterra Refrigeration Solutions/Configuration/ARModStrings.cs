using System;
using System.Text;
using FCSCommon.Helpers;

namespace FCS_Alterra_Refrigeration_Solutions.Configuration
{
    public class ARModStrings: ModStrings
    {
        /// <summary>
        /// The description of the mod.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// The Region of the object
        /// </summary>
        public string Region { get; set; }

        /// <summary>
        /// The booting text
        /// </summary>
        public string Booting { get; set; }

        /// <summary>
        /// The power off text
        /// </summary>
        public string PowerOff { get; set; }

        /// <summary>
        /// The battery text
        /// </summary>
        public string Battery { get; set; }

        /// <summary>
        /// The battery meters  text
        /// </summary>
        public string BatteryMeters { get; set; }

        /// <summary>
        /// The color picker text
        /// </summary>
        public string ColorPicker { get; set; }

        /// <summary>
        /// The storage mode text
        /// </summary>
        public string StorageMode { get; set; }

        /// <summary>
        /// The trickle mode text
        /// </summary>
        public string TrickleMode { get; set; }

        /// <summary>
        /// The charge mode text
        /// </summary>
        public string ChargeMode { get; set; }

        /// <summary>
        /// the settings text
        /// </summary>
        public string Settings { get; set; }

        /// <summary>
        /// Load the default English language.
        /// </summary>
        public override void LoadDefault()
        {
            Region = "en-US";
            Description = "This is a wall mounted battery storage for base backup power.";
            Booting = "BOOTING";
            PowerOff = "POWER OFF";
            Battery = "Battery";
            BatteryMeters = "BATTERY METERS";
            ColorPicker = "COLOR PICKLER";
            StorageMode = "Storage Mode";
            TrickleMode = "TRICKLE MODE";
            ChargeMode = "CHARGE MODE";
            Settings = "SETTINGS";
        }

        public override string ToString()
        {

            StringBuilder sb = new StringBuilder();

            sb.Append("ModString Language Details:");
            sb.Append(Environment.NewLine);

            sb.Append($"* Description: {Description}");
            sb.Append(Environment.NewLine);

            sb.Append($"* Region: {Region}");
            sb.Append(Environment.NewLine);

            sb.Append($"* Booting: {Booting}");
            sb.Append(Environment.NewLine);

            sb.Append($"* PowerOff: {PowerOff}");
            sb.Append(Environment.NewLine);

            sb.Append($"* Battery: {Battery}");
            sb.Append(Environment.NewLine);

            sb.Append($"* BatteryMeters: {BatteryMeters}");
            sb.Append(Environment.NewLine);

            sb.Append($"* ColorPicker: {ColorPicker}");
            sb.Append(Environment.NewLine);

            sb.Append($"* StorageMode: {StorageMode}");
            sb.Append(Environment.NewLine);

            sb.Append($"* TrickleMode: {TrickleMode}");
            sb.Append(Environment.NewLine);

            sb.Append($"* ChargeMode: {ChargeMode}");
            sb.Append(Environment.NewLine);


            sb.Append($"* Settings: {Settings}");
            sb.Append(Environment.NewLine);


            return sb.ToString();
        }
    }
}
