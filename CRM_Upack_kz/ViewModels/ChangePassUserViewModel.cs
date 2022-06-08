using System.ComponentModel.DataAnnotations;

namespace CRM_Upack_kz.ViewModels
{
    public class ChangePassUserViewModel
    {
        public string Id { get; set; }
        public string Email { get; set; }
        
        [Required(ErrorMessage = "Это поле обязательно для заполнения")]
        [Display(Name = "Введите новый пароль")]
        [MinLength(5, ErrorMessage = "Пароль должен содержать минимум 5 символов")]
        [DataType(DataType.Password)]
        public string NewPassword { get; set; }
        
        
        [Compare("NewPassword", ErrorMessage = "Пароли не совпадают")]
        [Required(ErrorMessage = "Это поле обязательно для заполнения")]
        [Display(Name = "Подтверждение пароля")]
        [DataType(DataType.Password)]
        public string ConfirmPassword { get; set; }
        

        [Required(ErrorMessage = "Это поле обязательно для заполнения")]
        [Display(Name = "Введите старый пароль")]
        [MinLength(5, ErrorMessage = "Пароль должен содержать минимум 5 символов")]
        [DataType(DataType.Password)]
        public string OldPassword { get; set; }
    }
}