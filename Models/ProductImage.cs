using System.ComponentModel.DataAnnotations;

namespace Ecommerce.Models
{
    public class ProductImage
    {
        [Key]
        public int ProductImageId { get; set; }

        public int ProductId { get; set; }
        public Product Product { get; set; }

        [Required, StringLength(200)]
        public string FileName { get; set; }

        public bool IsMain { get; set; }
    }
}
