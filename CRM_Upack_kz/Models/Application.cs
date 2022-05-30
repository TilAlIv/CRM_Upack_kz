using System;
using CRM_Upack_kz.Enums;

namespace CRM_Upack_kz.Models
{
    public class Application
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string ManagerId { get; set; }
        public virtual User Manager { get; set; }
        public string ClientId { get; set; }
        public virtual Client Client { get; set; }
        public DateTime CreateDate { get; set; }
        public DateTime WaitingDate { get; set; }
        public string ArticleNumber { get; set; }
        public decimal Quantity { get; set; }
        public double Price { get; set; }
        public double Amount { get; set; }
        public string Comment { get; set; }
        public decimal NumberApplication { get; set; }
        public AppState AppState { get; set; } = AppState.Новая;
    }
}