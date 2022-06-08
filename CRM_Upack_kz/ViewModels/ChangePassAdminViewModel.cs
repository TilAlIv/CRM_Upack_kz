using System.ComponentModel.DataAnnotations;

namespace CRM_Upack_kz.ViewModels
{
    public class ChangePassAdminViewModel
    {
        public string Id { get; set; }
        public string Email { get; set; }
        
        [Required(ErrorMessage = "Это поле обязательно для заполнения")]
        [Display(Name = "Введите новый пароль")]
        [MinLength(5, ErrorMessage = "Пароль должен содержать минимум 5 символов")]
        [DataType(DataType.Password)]
        public string NewPassword { get; set; }
    }
}