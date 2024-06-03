using InventoryHub.Exceptions;
using InventoryHub.Models;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace InventoryHub.Repositories
{
    public class InMemoryProductRepository : IProductRepository
    {
        private readonly static ConcurrentDictionary<Guid, Product> _products = new();

        public Task<IEnumerable<Product>> GetAllAsync()
        {
            var products = _products.Values.ToList(); //ToList что бы набор продуктов не менялся
            return Task.FromResult(products.AsEnumerable());
        }

        public Task<Product> GetByIdAsync(Guid id)
        {
            if (_products.TryGetValue(id, out var product) == false)
            {
                throw new ProductNotFoundException(id);
            }
            
            return Task.FromResult(product);
        }

        public Task<Product> AddAsync(Product product)
        {
            product.Id = Guid.NewGuid();
            _products[product.Id] = product;
            return Task.FromResult(product);
        }

        public Task<Product> DeleteAsync(Guid id)
        {
            if (_products.TryRemove(id, out var product) == false)
            {
                throw new ProductNotFoundException(id);
            }

            return Task.FromResult(product);
        }

        public Task<(Product product, bool isUpdated)> AdjustQuantityAsync(Guid id, int adjustment)
        {
            return Task.FromResult(AdjustQuantityInner(id, adjustment));
        }

        public Task<bool> AdjustQuantityFastAsync(Guid id, int adjustment)
        {
            return Task.FromResult(AdjustQuantityInner(id, adjustment).isUpdated);
        }

        public Task<Product> PatchProductAsync(Guid id, string newName)
        {
            if (_products.TryGetValue(id, out var product) == false)
            {
                throw new ProductNotFoundException(id);
            }

            product.Name = newName;
            return Task.FromResult(product);
        }


        private (Product product, bool isUpdated) AdjustQuantityInner(Guid id, int adjustment)
        {
            if (_products.TryGetValue(id, out var product) == false)
            {
                throw new ProductNotFoundException(id);
            }

            var newQuantity = product.Quantity + adjustment;

            if (newQuantity < 0)
            {
                return (product, false);
            }

            product.Quantity = newQuantity;
            return (product, true);
        }
    }
}