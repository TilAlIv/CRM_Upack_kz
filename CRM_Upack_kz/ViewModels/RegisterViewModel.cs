using System;
using System.ComponentModel.DataAnnotations;
using CRM_Upack_kz.Validation;
using Microsoft.AspNetCore.Http;

namespace CRM_Upack_kz.ViewModels
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
        [AgeValidation]
        public DateTime DateOfBirth { get; set; }

        [Required(ErrorMessage = "Это поле обязательно для заполнения")]
        [Display(Name = "Электронная почта")]
        [DataType(DataType.EmailAddress)]
        public string Email { get; set; }

        [Required(ErrorMessage = "Это поле обязательно для заполнения")]
        [Display(Name = "Номер телефона")]
        [DataType(DataType.PhoneNumber, ErrorMessage = "Вы вводите не номер телефона")]
        [MaxLength (12, ErrorMessage = "Максимальная длина номера не может быть больше 12 символов")]
        [MinLength(3, ErrorMessage = "Минимальная длина номера должена быть не меньше 3 символов")]
        public string PhoneNumber { get; set; }
        public string AvatarPath { get; set; }

        [Display(Name = "Укажите роль пользователя")]
        [Required(ErrorMessage = "Укажите роль это поле обязательно для заполнения")]
        public string Role { get; set; }
        
        
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