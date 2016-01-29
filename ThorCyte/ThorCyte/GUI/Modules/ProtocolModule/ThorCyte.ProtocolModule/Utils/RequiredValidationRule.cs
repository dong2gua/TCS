using System.Windows.Controls;
using System.Text.RegularExpressions;

namespace ThorCyte.ProtocolModule.Utils
{
    public class RequiredValidationRule : ValidationRule
    {
        public override ValidationResult Validate(object value, System.Globalization.CultureInfo cultureInfo)
        {
            if (value == null || string.IsNullOrWhiteSpace(value.ToString()))
            {
                return new ValidationResult(false, "Content can not be empty!");
            }
            return new ValidationResult(true, null);
        }
    }

    public class RequirIntegerValidationRule : ValidationRule
    {
        public override ValidationResult Validate(object value, System.Globalization.CultureInfo cultureInfo)
        {

            //determine if this is a Integer
            int val;
            return !int.TryParse(value.ToString(),out val) ? new ValidationResult(false, "Please input a number!") : new ValidationResult(true, null);
        }

    }
}
