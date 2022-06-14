using System;
using System.ComponentModel.DataAnnotations;
using CRM_Upack_kz.Validation;

namespace CRM_Upack_kz.ViewModels
{
    public class ApplViewModel
    {
        [Required(ErrorMessage = "Это поле обязательно")]
        [DataType(DataType.Text, ErrorMessage = "Вы вводите не текст")]
        [Display(Name = "Клиент")]
        [MaxLength(50, ErrorMessage = "Укажите меньше 10 символов")]
        [MinLength(2, ErrorMessage = "Укажите больше 3 символов")]
        public string NameClient { get; set; }
            
        [Required(ErrorMessage = "Это поле обязательно")]
        [DataType(DataType.Text, ErrorMessage = "Вы вводите не текст")]
        [Display(Name = "Код клиента")]
        [MaxLength(10, ErrorMessage = "Укажите меньше 10 символов")]
        [MinLength(4, ErrorMessage = "Укажите больше 3 символов")]
        public string CodeClient { get; set; }

        [Required(ErrorMessage = "Это поле обязательно")]
        [DataType(DataType.Text, ErrorMessage = "Вы вводите не текст")]
        [MaxLength(10, ErrorMessage = "Укажите меньше 10 символов")]
        [MinLength(3, ErrorMessage = "Укажите больше 3 символов")]
        [Display(Name = "Артикул товара")]
        public string ArticleNumber { get; set; }
        
        [Required(ErrorMessage = "Это поле обязательно")]
        [Range(Int32.MinValue, Int32.MaxValue)]
        
        [Display(Name = "Количество")]
        [ValueValidationInt]
        public int Quantity { get; set; }
        
        [Display(Name = "Цена за единицу")]
        [ValueValidationDouble]
        public double Price { get; set; }
        
        [MaxLength(250, ErrorMessage = "Укажите меньше 10 символов")]
        [MinLength(3, ErrorMessage = "Укажите больше 3 символов")]
        [Display(Name = "Добавьте комментарии")]
        public string Comment { get; set; }

        
    }
}