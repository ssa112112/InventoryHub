# InventoryHub
Простой API-сервис для управления товарными остатками. Оптимизирован для параллельных запросов по одному товару

# Как запустить без SQL?
- Замените `SQLProductRepository` на `InMemoryProductRepository` в `Program.cs`

# Как запустить c SQL?
- Пропишите валидную `InventoryHubConnectionString` в `appsettings.json`
 (Тестировалось на Microsoft SQL Server 2022 (RTM) - 16.0.1000.6 (X64))
- Примените миграции (команда `Update-Database` в `Package Manager Console` в VS)

# Как выглядит swagger проекта?
![image](https://github.com/ssa112112/InventoryHub/assets/69536429/d707683f-ea38-432a-ace3-6b78edba815d)

# Что принимает adjust метод?
Отрицательное или положительное число, которое будет добавлено к текущему количеству товара. 
Например, -5 для уменьшения количества на 5 или 10 для увеличения количества на 10

# Чем отличается fastAdjust от adjust?
- FastAdjust не возвращает Product
- Использует внутренние оптимизации 
(уменьшенное кол-во взаимодействий с SQL-сервером)
