using System.ComponentModel.DataAnnotations;

namespace CRM_Upack_kz.Validation
{
    public class ValueValidationDoubleAttribute : ValidationAttribute
    {
        public override bool IsValid(object? value)
        {
            if (value is double quantity)
            {
                if (quantity != 0) 
                    return true;
                else
                    ErrorMessage = "Укажите значение больше нуля";
            }
            return false;
        }
    }
}