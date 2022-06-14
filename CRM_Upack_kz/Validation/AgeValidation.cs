using System;
using System.ComponentModel.DataAnnotations;

namespace CRM_Upack_kz.Validation
{
    public class AgeValidation : ValidationAttribute
    {
        public override bool IsValid(object? value)
        {
            var maxDate = new DateTime(DateTime.Now.Year - 18, DateTime.Now.Month, DateTime.Now.Day);
            var minDate = new DateTime(DateTime.Now.Year - 65, DateTime.Now.Month, DateTime.Now.Day);

            if (value is DateTime age)
            {
                if (maxDate < age)
                {
                    ErrorMessage = "Ваш возраст меньше 18 лет";
                }
                else if (minDate > age)
                {
                    ErrorMessage = "Вам не может быть больше 65 лет";
                }
                else
                {
                    return true;
                }

                return false;
            }
            return false;
        }
    }
}