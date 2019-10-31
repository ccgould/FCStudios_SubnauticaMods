using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace FCSModdingUtility
{
    public class RequiredRule : ValidationRule
    {
        public RequiredRule()
        {
        }

        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            if (((string)value).Length > 8)
                return new ValidationResult(false, "This field is limited to 8 characters.");

            return new ValidationResult(true, null);
        }
    }
}
