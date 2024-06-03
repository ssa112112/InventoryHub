using InventoryHub.Exceptions;
using InventoryHub.Models;
using InventoryHub.SQL;
using Microsoft.EntityFrameworkCore;

namespace InventoryHub.Repositories
{
    public class SQLProductRepository : IProductRepository
    {
        private readonly InventoryHubDBContext _dbContext;

        public SQLProductRepository(InventoryHubDBContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<IEnumerable<Product>> GetAllAsync()
        {
            return await _dbContext.Products.ToListAsync();
        }

        public async Task<Product> GetByIdAsync(Guid id)
        {
            var product = await _dbContext.Products.FindAsync(id);
            return product ?? throw new ProductNotFoundException(id);
        }

        public async Task<Product> AddAsync(Product product)
        {
            product.Id = Guid.NewGuid();
            _dbContext.Products.Add(product);
            await _dbContext.SaveChangesAsync();
            return product;
        }

        public async Task<Product> DeleteAsync(Guid id)
        {
            var product = await _dbContext.Products.FindAsync(id) ?? throw new ProductNotFoundException(id);
            _dbContext.Products.Remove(product);
            await _dbContext.SaveChangesAsync();
            return product;
        }

        public async Task<(Product product, bool isUpdated)> AdjustQuantityAsync(Guid id, int adjustment)
        {
            var product = await _dbContext.Products.FindAsync(id) ?? throw new ProductNotFoundException(id);
            var newQuantity = product.Quantity + adjustment;

            if (newQuantity < 0)
            {
                return (product, false);
            }

            product.Quantity = newQuantity;
            _dbContext.Products.Update(product);
            await _dbContext.SaveChangesAsync();
            return (product, true);
        }

        public async Task<Product> PatchProductAsync(Guid id, string newName)
        {
            var product = await _dbContext.Products.FindAsync(id) ?? throw new ProductNotFoundException(id);
            product.Name = newName;
            _dbContext.Products.Update(product);
            await _dbContext.SaveChangesAsync();
            return product;
        }
    }
}
