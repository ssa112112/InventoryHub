using System.ComponentModel.DataAnnotations;

namespace InventoryHub.DTOs
{
    public class CreateProductDto
    {
        [MinLength(3, ErrorMessage = "The Name must be at least 3 characters long.")]
        public required string Name { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "The Quantity must be greater than or equal to 0.")]
        public int Quantity { get; set; }
    }
}
