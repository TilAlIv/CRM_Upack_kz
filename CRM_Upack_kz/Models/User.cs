using System;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;

namespace CRM_Upack_kz.Models
{
    public class User : IdentityUser
    {
        public string Name { get; set; }
        public string Surname { get; set; }
        public DateTime DateOfBirth   { get; set; }
        public string AvatarPath { get; set; }
        public string RoleDisplay { get; set; }
        
        [NotMapped] 
        public IFormFile File { get; set; }
    }
}