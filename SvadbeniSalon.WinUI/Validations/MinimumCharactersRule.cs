using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Windows.Controls;

namespace SvadbeniSalon.WinUI.Validations
{
    public class MinimumCharactersRule : ValidationRule
    {
        public int MinimumCharacters { get; set; }
        public MinimumCharactersRule()
        {
            //ValidatesOnTargetUpdated = true;
        }
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            var parsedValue = value as string;

            if (parsedValue == null || parsedValue.Length < MinimumCharacters)
                return new ValidationResult(false, "Field is required.");

            return new ValidationResult(true, null);
        }
    }
}
