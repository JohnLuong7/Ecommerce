using Microsoft.AspNetCore.Identity;
using System;

namespace Ecommerce.Models
{
    public class ApplicationUser : IdentityUser
    {
        // Field custom từ Users.cs cũ
        public string FullName { get; set; }
    }
}
