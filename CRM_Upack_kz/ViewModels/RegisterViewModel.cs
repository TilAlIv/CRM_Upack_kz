using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace CRM_Upack_kz.ViewModel
{
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "Это поле обязательно для заполнения")]
        [Display(Name = "Имя пользователя")]
        [DataType(DataType.Text)]
        public string Name { get; set; }

        [Required(ErrorMessage = "Это поле обязательно для заполнения")]
        [Display(Name = "Фамилия пользователя")]
        [DataType(DataType.Text)]
        public string Surname { get; set; }

        [Required(ErrorMessage = "Это поле обязательно для заполнения")]
        [Display(Name = "Дата рождения")]
        public DateTime DateOfBirth { get; set; }

        [Required(ErrorMessage = "Это поле обязательно для заполнения")]
        [Display(Name = "Электронная почта")]
        [DataType(DataType.EmailAddress)]
        public string Email { get; set; }

        [Required(ErrorMessage = "Это поле обязательно для заполнения")]
        [Display(Name = "Номер телефона")]
        [Phone(ErrorMessage = "Данный номер не является номером телефона")]
        [Range(3,12, ErrorMessage = "Укажите номер от 3 до 12 символов")]
        public string PhoneNumber { get; set; }
        public string AvatarPath { get; set; }
        
        
        public IFormFile File { get; set; }

        [Required(ErrorMessage = "Это поле обязательно для заполнения")]
        [Display(Name = "Пароль")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Compare("Password", ErrorMessage = "Пароли не совпадают")]
        [Required(ErrorMessage = "Это поле обязательно для заполнения")]
        [Display(Name = "Подтверждение пароля")]
        [DataType(DataType.Password)]
        public string ConfirmPassword { get; set; }

    }
}