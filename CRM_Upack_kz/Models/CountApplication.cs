using System;

namespace CRM_Upack_kz.Models
{
    public class CountApplication
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public DateTime DateTime { get; set; } = DateTime.Now;
    }
}