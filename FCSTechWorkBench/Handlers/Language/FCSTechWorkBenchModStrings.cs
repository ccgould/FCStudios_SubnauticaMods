using FCSCommon.Helpers;
using System;
using System.ComponentModel;
using System.Text;

namespace FCSTechWorkBench.Handlers.Language
{
    public class FCSTechWorkBenchModStrings : ModStrings
    {
        public string AIDeepDrillerBatteryDescription = "Provide your Deep Driller with this very efficient battery supply";

        public string AIDeepDrillerSolarDescription = "Provide your Deep Driller with this very efficient solar panel to get energy from the sun";

        /// <summary>
        /// Load the default English language.
        /// </summary>
        public override void LoadDefault()
        {
            Region = "en-US";
            Description = "Zip Zap use me to fabricate all you FCS object needs";
            AIDeepDrillerBatteryDescription = "Provide your Deep Driller with this very efficient battery supply";
            AIDeepDrillerSolarDescription = "Provide your Deep Driller with this very efficient solar panel to get energy from the sun";
        }

        public override string ToString()
        {

            StringBuilder sb = new StringBuilder();

            sb.Append($"ModString {nameof(FCSTechWorkBenchModStrings)} Details:");
            sb.Append(Environment.NewLine);

            foreach (PropertyDescriptor prop in TypeDescriptor.GetProperties(this))
            {
                sb.Append($"* {prop.Name}: {prop.GetValue(this)}");
                sb.Append(Environment.NewLine);
            }
            return sb.ToString();
        }
    }
}
