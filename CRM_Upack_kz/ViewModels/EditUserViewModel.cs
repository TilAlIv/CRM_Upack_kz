using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace CRM_Upack_kz.ViewModels
{
    public class EditUserViewModel
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
        [DataType(DataType.PhoneNumber, ErrorMessage = "Вы вводите не номер телефона")]
        [MaxLength (12, ErrorMessage = "Максимальная длина номера не может быть больше 12 символов")]
        [MinLength(3, ErrorMessage = "Минимальная длина номера должена быть не меньше 3 символов")]
        public string PhoneNumber { get; set; }
        
        [Display(Name = "Укажите роль пользователя")]
        [Required(ErrorMessage = "Укажите роль это поле обязательно для заполнения")]
        public string Role { get; set; }

        public string AvatarPath { get; set; }
        public IFormFile File { get; set; }
    }
}