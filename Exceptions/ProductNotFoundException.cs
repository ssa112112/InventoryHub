namespace InventoryHub.Exceptions
{
    //Предпологается, что сервисом будут пользоваться доверенные абоненты и данное исключение будет падать редко
    public class ProductNotFoundException : Exception
    {
        public Guid ProductId { get; }

        public ProductNotFoundException(Guid id)
            : base($"Product with ID {id} was not found.")
        {
            ProductId = id;
        }

        public ProductNotFoundException(Guid id, string message)
            : base(message)
        {
            ProductId = id;
        }

        public ProductNotFoundException(Guid id, string message, Exception innerException)
            : base(message, innerException)
        {
            ProductId = id;
        }
    }
}
