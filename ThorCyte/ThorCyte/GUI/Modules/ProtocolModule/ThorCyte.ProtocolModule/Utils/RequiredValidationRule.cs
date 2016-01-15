using System.Windows.Controls;

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
}
