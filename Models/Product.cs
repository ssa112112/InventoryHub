using System.ComponentModel.DataAnnotations;

namespace InventoryHub.Models
{
    public class Product
    {
        [Key]
        public Guid Id { get; set; }

        //Todo: вынести 3 в конфиг
        [MinLength(3)]
        public required string Name { get; set; }

        [Range(0, int.MaxValue)]
        public int Quantity { get; set; }
    }
}
