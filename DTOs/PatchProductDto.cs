using System.ComponentModel.DataAnnotations;

namespace InventoryHub.DTOs
{
    public class PatchProductDto
    {
        [MinLength(3, ErrorMessage = "The Name must be at least 3 characters long.")]   
        public required string Name { get; set; }
    }
}
