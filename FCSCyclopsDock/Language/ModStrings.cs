using FCSCyclopsDock.Configuration;
using System;
using System.ComponentModel;
using System.Text;

namespace FCSCyclopsDock.Language
{
    public class ModStrings : FCSCommon.Helpers.ModStrings
    {
        /// <summary>
        /// Load the default English language.
        /// </summary>
        public override void LoadDefault()
        {
            Region = "en-US";
            Description = Information.ModDescription;
        }

        public override string ToString()
        {

            StringBuilder sb = new StringBuilder();

            sb.Append($"ModString {nameof(ModStrings)} Details:");
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
