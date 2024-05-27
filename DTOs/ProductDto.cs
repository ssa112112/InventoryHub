namespace InventoryHub.DTOs
{
    public class ProductDto
    {
        public Guid Id { get; set; }
        public required string Name { get; set; }
        public int Quantity { get; set; }
    }
}
