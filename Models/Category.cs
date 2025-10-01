using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Ecommerce.Models
{
    public class Category
    {
        [Key]
        public int CategoryId { get; set; }

        [Required, StringLength(100)]
        public string Name { get; set; }

        [Required, StringLength(100)]
        public string Slug { get; set; }

        [StringLength(500)]
        public string Description { get; set; }

        public ICollection<Product> Products { get; set; }
    }
}
