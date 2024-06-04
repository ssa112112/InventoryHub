using InventoryHub.Exceptions;
using InventoryHub.Models;
using InventoryHub.SQL;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Polly;
using System.Data;

namespace InventoryHub.Repositories
{
    public class SQLProductRepository : IProductRepository
    {
        private readonly InventoryHubDBContext _dbContext;
        private readonly ILogger<SQLProductRepository> _logger;

        public SQLProductRepository(InventoryHubDBContext dbContext, ILogger<SQLProductRepository> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
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

        /// <summary>
        /// Оптимизация 1: один SQL запрос вместо двух
        /// Оптимизация 2: не работаем с лишними полями Product
        /// => Средний прирост производительности под нагрузкой около 25%
        /// </summary>
        /// <param name="id"></param>
        /// <param name="adjustment"></param>
        /// <returns></returns>
        /// <exception cref="ProductNotFoundException"></exception>
        /// <exception cref="Exception"></exception>
        public async Task<bool> AdjustQuantityFastAsync(Guid id, int adjustment)
        {
            var policy = Policy
            .Handle<SqlException>()
            .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)), (exception, timeSpan, retryCount, context) =>
            {
                _logger.LogWarning(exception, "Exception caught, retrying...");
            });

            return await policy.ExecuteAsync(async () =>
            {
                var statusParam = new SqlParameter
                {
                    ParameterName = "@status",
                    SqlDbType = SqlDbType.Int,
                    Direction = ParameterDirection.Output
                };

                var productIdParam = new SqlParameter("@productId", id);
                var adjustmentParam = new SqlParameter("@adjustment", adjustment);

                await _dbContext.Database.ExecuteSqlRawAsync(@"
            SET TRANSACTION ISOLATION LEVEL READ COMMITTED;
            
            DECLARE @currentQuantity INT;
            DECLARE @newQuantity INT;
            BEGIN TRANSACTION;

            -- Выбираем текущее Quantity
            SELECT @currentQuantity = Quantity
            FROM Products
            WITH (ROWLOCK, UPDLOCK)
            WHERE Id = @productId;

            -- Возвращаем статус -1 есть продукт не найден
            IF @currentQuantity IS NULL
            BEGIN
                SET @status = -1;
                ROLLBACK TRANSACTION;
                RETURN;
            END

            -- Считаем новое Quantity
            SET @newQuantity = @currentQuantity + @adjustment;

            -- Если новое Quantity >= 0, то обновляем продукт м возвращаем статус 0
            IF @newQuantity >= 0
            BEGIN
                UPDATE Products
                SET Quantity = @newQuantity
                WHERE Id = @productId;
                SET @status = 0;
                COMMIT TRANSACTION;
            END
            -- Если новое Quantity < 0 возвращаем статус -2
            ELSE
            BEGIN
                SET @status = -2;
                ROLLBACK TRANSACTION;
            END
        ", productIdParam, adjustmentParam, statusParam);

                var status = (int)statusParam.Value;

                return status switch
                {
                    -2 => false,
                    -1 => throw new ProductNotFoundException(id),
                    0 => true,
                    _ => throw new Exception($"Unexpected status. Status = {status}"),
                };
            });
        }

        public async Task<(Product product, bool isUpdated)> AdjustQuantityAsync(Guid id, int adjustment)
        {
            var policy = Policy
                .Handle<SqlException>()
                .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)), (exception, timeSpan, retryCount, context) =>
                {
                    _logger.LogWarning(exception, "Exception caught, retrying...");
                });

            return await policy.ExecuteAsync(async () =>
            {
                using var transaction = await _dbContext.Database.BeginTransactionAsync(IsolationLevel.ReadCommitted);
                try
                {

#pragma warning disable EF1001 // Игнорируем "The query uses the 'First'/'FirstOrDefault' operator without 'OrderBy' and filter operators", т.к. делаем выборку по точному совпадению PrimaryKey
                    // Читаем и блокируем строку
                    var product = await _dbContext.Products
                        .FromSqlInterpolated($"SELECT * FROM Products WITH (ROWLOCK, UPDLOCK) WHERE Id = {id}")
                        .FirstOrDefaultAsync();
#pragma warning restore EF1001 

                    if (product == null)
                    {
                        throw new ProductNotFoundException(id);
                    }

                    int newQuantity = product.Quantity + adjustment;

                    if (newQuantity < 0)
                    {
                        await transaction.RollbackAsync();
                        return (product, false);
                    }

                    product.Quantity = newQuantity;

                    await _dbContext.SaveChangesAsync();
                    await transaction.CommitAsync();

                    return (product, true);

                }
                catch (Exception)
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            });
        }

        public async Task<Product> PatchProductAsync(Guid id, string newName)
        {
            var product = await _dbContext.Products.FindAsync(id) ?? throw new ProductNotFoundException(id);
            product.Name = newName;
            await _dbContext.SaveChangesAsync();
            return product;
        }
    }
}
