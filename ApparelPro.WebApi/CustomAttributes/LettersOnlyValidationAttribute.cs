using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace ApparelPro.WebApi.CustomAttributes
{
    public class LettersOnlyValidationAttribute:ValidationAttribute
    {
        private readonly int? _numberOfChars;
        public LettersOnlyValidationAttribute(int numberOfChars=0)
        {
            _numberOfChars = numberOfChars;
        }
      
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {           
            string expression = "^[A-Za-z]" + "{" + _numberOfChars + "}$";
            var lettersOnlyRegex = _numberOfChars!.Value>0 ? new Regex(expression) : new Regex(@"^[A-Za-z]+$");            
            var strval = value?.ToString();
            var valid = lettersOnlyRegex.IsMatch(strval!);
            if (valid) { return ValidationResult.Success; }
            
            return new ValidationResult(string.Format("Value must contain only {0} letters(no spaces, digits, or other chars)",_numberOfChars>0?_numberOfChars:null));
        }
    }
}
