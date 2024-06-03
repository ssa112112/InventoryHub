using InventoryHub.Models;

namespace InventoryHub.Repositories
{
    public interface IProductRepository
    {
        //Todo: добавить пагинацию
        /// <summary>
        /// Получить все продукты
        /// </summary>
        /// <returns></returns>
        Task<IEnumerable<Product>> GetAllAsync();

        /// <summary>
        /// Получить продукт по ID. 
        /// </summary>
        /// <returns></returns>
        /// <exception cref="ProductNotFoundException">Выбрасывается, если продукт с указанным ID не найден.</exception>
        Task<Product> GetByIdAsync(Guid id);

        /// <summary>
        /// Добавить новый продукт
        /// </summary>
        /// <returns>Добавленный продукт</returns>
        Task<Product> AddAsync(Product product);

        /// <summary>
        /// Обновить часть свойств продукта
        /// </summary>
        /// <returns>Обновленный продукт</returns>
        /// <exception cref="ProductNotFoundException">Выбрасывается, если продукт с указанным ID не найден.</exception>
        Task<Product> PatchProductAsync(Guid id, string newName);

        //Todo: Можно удалять продукты, которые есть на складе?
        /// <summary>
        /// Удалить продукт по ID
        /// </summary>
        /// <returns>Удаленный продукт</returns>
        /// <exception cref="ProductNotFoundException">Выбрасывается, если продукт с указанным ID не найден.</exception>
        Task<Product> DeleteAsync(Guid id);

        /// <summary>
        /// Изменить остаток товара с указанным ID
        /// </summary>
        /// <param name="id"></param>
        /// <param name="adjustment">Отрицательное или положительное число, которое будет добавлено к текущему количеству товара. 
        /// Например, -5 для уменьшения количества на 5 или +10 для увеличения количества на 10</param>
        /// <returns>
        /// Кортеж, содержащий товар и булево значение isUpdated.
        /// isUpdated.True - остаток товара успешно обновлен.
        /// isUpdated.False - операцию невозможно выполнить, так как остаток товара всегда должен быть >= 0.
        /// </returns>
        /// <exception cref="ProductNotFoundException">Выбрасывается, если продукт с указанным ID не найден.</exception>
        Task<(Product product, bool isUpdated)> AdjustQuantityAsync(Guid id, int adjustment);

        /// <summary>
        /// Оптимизрованная версия AdjustQuantity
        /// </summary>
        /// <param name="id"></param>
        /// <param name="adjustment"></param>
        /// <returns>False - операцию невозможно выполнить, так как остаток товара всегда должен быть >= 0</returns>
        Task<bool> AdjustQuantityFastAsync(Guid id, int adjustment);
    }
}
