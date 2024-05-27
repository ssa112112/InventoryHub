namespace InventoryHub.DTOs
{
    public class CreateProductDto
    {
        public required string Name { get; set; }
        
        public int Quantity { get; set; }
    }
}
