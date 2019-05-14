using FCSCommon.Helpers;
using System;
using System.ComponentModel;
using System.Text;

namespace FCSAlterraIndustrialSolutions.Handlers.Language
{
    public class DeepDrillerModStrings : ModStrings
    {
        /// <summary>
        /// Load the default English language.
        /// </summary>
        public override void LoadDefault()
        {
            Region = "en-US";
            Description = "Let's dig down to the deep down deep dark!";
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

        public string ModulesItemNotAllowed { get; set; } = "Deep Drill modules allowed only";
        public string ItemNotAllowed { get; set; } = "Cannot place this item in here.";
    }
}
