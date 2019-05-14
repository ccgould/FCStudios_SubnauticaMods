using FCSCommon.Helpers;
using System;
using System.ComponentModel;
using System.Text;

namespace FCSAlterraIndustrialSolutions.Handlers.Language
{
    public class JetStreamT242ModStrings : ModStrings
    {
        /// <summary>
        /// The depth text
        /// </summary>
        public string Depth { get; set; }

        /// <summary>
        /// The Speed text
        /// </summary>
        public string Speed { get; set; }

        /// <summary>
        /// The power text
        /// </summary>
        public string Power { get; set; }

        /// <summary>
        /// The health meters  text
        /// </summary>
        public string Health { get; set; }


        /// <summary>
        /// Load the default English language.
        /// </summary>
        public override void LoadDefault()
        {
            Region = "en-US";
            Description = "The Jet Stream T242 provides power by using the water current. The faster the turbine spins the more power output.";
            Depth = "Depth";
            Speed = "Speed";
            Power = "Power";
            Health = "Health";
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
