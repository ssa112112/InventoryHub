﻿using InventoryHub.Exceptions;
using InventoryHub.Models;
using InventoryHub.SQL;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Data;

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

        /// <summary>
        /// Оптимизация 1: один SQL запрос вместо двух
        /// Оптимизация 2: не работаем с лишними полями Product
        /// </summary>
        /// <param name="id"></param>
        /// <param name="adjustment"></param>
        /// <returns></returns>
        /// <exception cref="ProductNotFoundException"></exception>
        /// <exception cref="Exception"></exception>
        public async Task<bool> AdjustQuantityFastAsync(Guid id, int adjustment)
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
            DECLARE @currentQuantity INT;
            DECLARE @newQuantity INT;

            -- Выбираем текущее Quantity
            SELECT @currentQuantity = Quantity
            FROM Products
            WHERE Id = @productId;

            -- Возвращаем статус -1 есть продукт не найден
            IF @currentQuantity IS NULL
            BEGIN
                SET @status = -1;
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
            END
            -- Если новое Quantity < 0 возвращаем статус -2
            ELSE
            BEGIN
                SET @status = -2;
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
