using FCSCommon.Helpers;
using System;
using System.ComponentModel;
using System.Text;

namespace FCSAlterraIndustrialSolutions.Handlers.Language
{
    public class MarineMonitorModStrings : ModStrings
    {
        /// <summary>
        /// The depth text
        /// </summary>
        public string Depth { get; set; }

        /// <summary>
        /// The Damaged text
        /// </summary>
        public string Damaged { get; set; }

        /// <summary>
        /// The powered off text
        /// </summary>
        public string PoweredOff { get; set; }

        /// <summary>
        /// The health meters  text
        /// </summary>
        public string Health { get; set; }

        /// <summary>
        /// The Working  text
        /// </summary>
        public string Working { get; set; }

        /// <summary>
        /// The status overview  text
        /// </summary>
        public string StatusOverview { get; set; }

        /// <summary>
        /// The legend text
        /// </summary>
        public string Legend { get; set; }

        /// <summary>
        /// The ping text
        /// </summary>
        public string PING { get; set; }

        /// <summary>
        /// The pinging text
        /// </summary>
        public string PINGING { get; set; }

        /// <summary>
        /// The off text
        /// </summary>
        public string OFF { get; set; }


        /// <summary>
        /// The on text
        /// </summary>
        public string ON { get; set; }

        /// <summary>
        /// The text for the damaged legend area
        /// </summary>
        public string DamagedLegend { get; set; }

        /// <summary>
        /// The text for the mildly damaged legend area
        /// </summary>
        public string MidlyDamagedLegend { get; set; }

        /// <summary>
        /// The text for the healthy legend area
        /// </summary>
        public string HealthyLegend { get; set; }


        /// <summary>
        /// Load the default English language.
        /// </summary>
        public override void LoadDefault()
        {
            Region = "en-US";
            Description = "Why go outside and get wet? Get your turbine status and control your turbine from inside!";
            Depth = "Depth";
            Health = "Health";
            PoweredOff = "Powered Off";
            Working = "Working";
            StatusOverview = "Status Overview";
            Legend = "Legend";
            HealthyLegend = "Healthy\n51 % => 100%";
            MidlyDamagedLegend = "Midly Damaged\n20% => 50%";
            DamagedLegend = "Damaged\n0% => 49%% ";
            Damaged = "DAMAGED";
            ON = "ON";
            OFF = "OFF";
            PING = "PING";
            PINGING = "PINGING";
        }




        public override string ToString()
        {

            StringBuilder sb = new StringBuilder();

            sb.Append($"ModString {nameof(JetStreamT242ModStrings)} Details:");
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
