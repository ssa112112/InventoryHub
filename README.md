# InventoryHub
Простой API-сервис для управления товарными остатками. Оптимизирован для параллельных запросов по одному товару

# Как запустить без SQL?
- Замените `SQLProductRepository` на `InMemoryProductRepository` в `Program.cs`

# Как запустить c SQL?
- Пропишите валидную `InvenotyHubConnectionString` в `appsettings.json`
 (Тестировалось на Microsoft SQL Server 2022 (RTM) - 16.0.1000.6 (X64))

# Как выглядит swagger проекта?
![image](https://github.com/ssa112112/InventoryHub/assets/69536429/d707683f-ea38-432a-ace3-6b78edba815d)
