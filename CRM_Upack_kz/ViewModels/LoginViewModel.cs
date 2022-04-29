using System.ComponentModel.DataAnnotations;

namespace CRM_Upack_kz.ViewModel
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Это поле обязательно для заполнения")]
        [Display(Name = "Электронная почта")]
        [DataType(DataType.EmailAddress)]
        public string Email { get; set; }
        
        [Required(ErrorMessage = "Это поле обязательно для заполнения")]
        [Display(Name = "Пароль")]
        [DataType(DataType.Password)]
        public string Password { get; set; }
        
        [Display(Name = "Запомнить")] 
        public bool RememberMe { get; set; }

        public string ReturnUrl { get; set; }

        public string RepeatLinkForConfirmEmail { get; set; }
    }
}