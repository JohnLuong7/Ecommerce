using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Ecommerce.Models
{
    public class Cart
    {
        [Key]
        public int CartId { get; set; }

        // Liên kết với user đăng nhập
        [Required]
        public string UserId { get; set; }
        public ApplicationUser Id { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<CartItem> CartItems { get; set; }
    }
}
