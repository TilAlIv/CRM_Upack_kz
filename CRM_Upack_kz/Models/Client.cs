using System;

namespace CRM_Upack_kz.Models
{
    public class Client
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string CodeClient { get; set; }
        public string Title { get; set; }
    }
}