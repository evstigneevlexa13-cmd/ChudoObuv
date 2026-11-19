using System;
using System.Collections.Generic;
using System.Data.SqlClient;

namespace ChudoObuv
{
    public static class ProductRepository
    {
        public static List<ProductItem> LoadCatalog(
            DateTime calculationDate)
        {
            HashSet<int> productsWithOrders =
                LoadProductsWithOrdersLastMonth(calculationDate);

            var products = new List<ProductItem>();

            const string sql = @"
SELECT p.ProductId, p.ProductName, p.Description, p.Composition,
    p.Price, p.ImageFileName, c.CategoryId, c.CategoryName,
    m.ManufacturerName,
    (SELECT ISNULL(SUM(s.Quantity), 0)
     FROM StockItems s
     WHERE s.ProductId = p.ProductId) AS TotalQuantity
FROM Products p
JOIN Categories c ON c.CategoryId = p.CategoryId
JOIN Manufacturers m ON m.ManufacturerId = p.ManufacturerId
ORDER BY p.ProductName";

            using (SqlConnection connection = Db.CreateConnection())
            using (var command = new SqlCommand(sql, connection))
            {
                connection.Open();

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var product = new ProductItem();

                        product.ProductId = reader.GetInt32(0);
                        product.ProductName = reader.GetString(1);
                        product.Description = Db.ReadString(reader, 2);
                        product.Composition = Db.ReadString(reader, 3);
                        product.BasePrice = reader.GetDecimal(4);
                        product.ImageFileName = Db.ReadString(reader, 5);
                        product.CategoryId = reader.GetInt32(6);
                        product.CategoryName = reader.GetString(7);
                        product.ManufacturerName = reader.GetString(8);
                        product.TotalQuantity = reader.GetInt32(9);

                        bool hadOrdersLastMonth =
                            productsWithOrders.Contains(product.ProductId);

                        product.FinalPrice =
                            PriceCalculator.GetFinalPrice(
                                product.BasePrice,
                                hadOrdersLastMonth);

                        products.Add(product);
                    }
                }
            }

            return products;
        }

        private static HashSet<int> LoadProductsWithOrdersLastMonth(
            DateTime calculationDate)
        {
            DateTime startDate;
            DateTime endDate;

            PriceCalculator.GetPreviousMonthRange(
                calculationDate,
                out startDate,
                out endDate);

            const string sql = @"
SELECT DISTINCT s.ProductId
FROM OrderItems oi
JOIN Orders o ON o.OrderId = oi.OrderId
JOIN StockItems s ON s.StockItemId = oi.StockItemId
WHERE o.OrderDate >= @startDate
  AND o.OrderDate < @endDate";

            var result = new HashSet<int>();

            using (SqlConnection connection = Db.CreateConnection())
            using (var command = new SqlCommand(sql, connection))
            {
                command.Parameters.AddWithValue("@startDate", startDate);
                command.Parameters.AddWithValue("@endDate", endDate);

                connection.Open();

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        result.Add(reader.GetInt32(0));
                    }
                }
            }

            return result;
        }

        public static List<CategoryItem> LoadCategories()
        {
            var categories = new List<CategoryItem>();

            categories.Add(new CategoryItem
            {
                CategoryId = 0,
                CategoryName = "Все категории"
            });

            const string sql =
                "SELECT CategoryId, CategoryName " +
                "FROM Categories ORDER BY CategoryName";

            using (SqlConnection connection = Db.CreateConnection())
            using (var command = new SqlCommand(sql, connection))
            {
                connection.Open();

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        categories.Add(new CategoryItem
                        {
                            CategoryId = reader.GetInt32(0),
                            CategoryName = reader.GetString(1)
                        });
                    }
                }
            }

            return categories;
        }

        public static List<StockOption> LoadStockOptions(int productId)
        {
            var options = new List<StockOption>();

            const string sql = @"
SELECT s.StockItemId, z.SizeValue, s.Quantity
FROM StockItems s
JOIN Sizes z ON z.SizeId = s.SizeId
WHERE s.ProductId = @productId
ORDER BY z.SizeValue";

            using (SqlConnection connection = Db.CreateConnection())
            using (var command = new SqlCommand(sql, connection))
            {
                command.Parameters.AddWithValue("@productId", productId);
                connection.Open();

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var option = new StockOption();

                        option.StockItemId = reader.GetInt32(0);
                        option.SizeValue = reader.GetDecimal(1);
                        option.Quantity = reader.GetInt32(2);

                        options.Add(option);
                    }
                }
            }

            return options;
        }
    }

    public static class UserRepository
    {
        public static AppUser FindByLogin(string login)
        {
            const string sql = @"
SELECT u.UserId, u.LastName, u.FirstName, u.MiddleName,
    r.RoleName
FROM Users u
JOIN Roles r ON r.RoleId = u.RoleId
WHERE u.UserLogin = @login";

            using (SqlConnection connection = Db.CreateConnection())
            using (var command = new SqlCommand(sql, connection))
            {
                command.Parameters.AddWithValue("@login", login);
                connection.Open();

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    if (!reader.Read())
                    {
                        return null;
                    }

                    var user = new AppUser();

                    user.UserId = reader.GetInt32(0);
                    user.LastName = reader.GetString(1);
                    user.FirstName = reader.GetString(2);
                    user.MiddleName = Db.ReadString(reader, 3);
                    user.RoleName = reader.GetString(4);

                    return user;
                }
            }
        }
    }

    public static class OrderRepository
    {
        public static int CreateOrder(int userId, List<CartItem> items)
        {
            using (SqlConnection connection = Db.CreateConnection())
            {
                connection.Open();

                using (SqlTransaction transaction =
                    connection.BeginTransaction())
                {
                    int orderId;

                    const string insertOrderSql =
                        "INSERT INTO Orders (OrderDate, UserId) " +
                        "OUTPUT INSERTED.OrderId " +
                        "VALUES (@orderDate, @userId)";

                    using (var insertOrder = new SqlCommand(
                        insertOrderSql, connection, transaction))
                    {
                        insertOrder.Parameters.AddWithValue(
                            "@orderDate", DateTime.Today);

                        insertOrder.Parameters.AddWithValue(
                            "@userId", userId);

                        orderId = (int)insertOrder.ExecuteScalar();
                    }

                    foreach (CartItem item in items)
                    {
                        ReserveStock(connection, transaction, item);

                        InsertOrderItem(
                            connection,
                            transaction,
                            orderId,
                            item);
                    }

                    transaction.Commit();
                    return orderId;
                }
            }
        }

        private static void ReserveStock(
            SqlConnection connection,
            SqlTransaction transaction,
            CartItem item)
        {
            const string sql = @"
UPDATE StockItems
SET Quantity = Quantity - @quantity
WHERE StockItemId = @stockItemId
  AND Quantity >= @quantity";

            using (var command = new SqlCommand(
                sql, connection, transaction))
            {
                command.Parameters.AddWithValue("@quantity", item.Quantity);
                command.Parameters.AddWithValue(
                    "@stockItemId", item.StockItemId);

                if (command.ExecuteNonQuery() == 0)
                {
                    throw new InvalidOperationException(
                        "Модели «" + item.ProductName +
                        "» размера " + item.SizeText +
                        " уже не хватает в наличии. " +
                        "Уменьшите количество или уберите позицию из заказа.");
                }
            }
        }

        private static void InsertOrderItem(
            SqlConnection connection,
            SqlTransaction transaction,
            int orderId,
            CartItem item)
        {
            const string sql = @"
INSERT INTO OrderItems (
    OrderId, StockItemId, Quantity, UnitPrice
)
VALUES (
    @orderId, @stockItemId, @quantity, @unitPrice
)";

            using (var command = new SqlCommand(
                sql, connection, transaction))
            {
                command.Parameters.AddWithValue("@orderId", orderId);
                command.Parameters.AddWithValue(
                    "@stockItemId", item.StockItemId);
                command.Parameters.AddWithValue("@quantity", item.Quantity);
                command.Parameters.AddWithValue(
                    "@unitPrice", item.UnitPrice);

                command.ExecuteNonQuery();
            }
        }

        public static List<OrderRow> LoadOrders()
        {
            var orders = new List<OrderRow>();

            const string sql = @"
SELECT o.OrderId, o.OrderDate, u.LastName, u.FirstName,
    u.MiddleName,
    ISNULL(SUM(oi.Quantity * oi.UnitPrice), 0) AS Total
FROM Orders o
JOIN Users u ON u.UserId = o.UserId
LEFT JOIN OrderItems oi ON oi.OrderId = o.OrderId
GROUP BY o.OrderId, o.OrderDate, u.LastName, u.FirstName,
    u.MiddleName
ORDER BY o.OrderDate DESC, o.OrderId DESC";

            using (SqlConnection connection = Db.CreateConnection())
            using (var command = new SqlCommand(sql, connection))
            {
                connection.Open();

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var order = new OrderRow();

                        order.OrderId = reader.GetInt32(0);
                        order.OrderDate = reader.GetDateTime(1);
                        order.ClientName = (
                            reader.GetString(2) + " " +
                            reader.GetString(3) + " " +
                            Db.ReadString(reader, 4)).Trim();

                        order.Total = reader.GetDecimal(5);

                        orders.Add(order);
                    }
                }
            }

            return orders;
        }

        public static List<OrderItemRow> LoadOrderItems(int orderId)
        {
            var items = new List<OrderItemRow>();

            const string sql = @"
SELECT oi.OrderItemId, p.ProductName, m.ManufacturerName,
    z.SizeValue, oi.Quantity, oi.UnitPrice
FROM OrderItems oi
JOIN StockItems s ON s.StockItemId = oi.StockItemId
JOIN Products p ON p.ProductId = s.ProductId
JOIN Manufacturers m ON m.ManufacturerId = p.ManufacturerId
JOIN Sizes z ON z.SizeId = s.SizeId
WHERE oi.OrderId = @orderId
ORDER BY oi.OrderItemId";

            using (SqlConnection connection = Db.CreateConnection())
            using (var command = new SqlCommand(sql, connection))
            {
                command.Parameters.AddWithValue("@orderId", orderId);
                connection.Open();

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var item = new OrderItemRow();

                        item.OrderItemId = reader.GetInt32(0);
                        item.ProductName = reader.GetString(1);
                        item.ManufacturerName = reader.GetString(2);
                        item.SizeValue = reader.GetDecimal(3);
                        item.Quantity = reader.GetInt32(4);
                        item.UnitPrice = reader.GetDecimal(5);

                        items.Add(item);
                    }
                }
            }

            return items;
        }

        public static void DeleteOrder(int orderId)
        {
            const string restoreSql = @"
UPDATE s
SET s.Quantity = s.Quantity + oi.Quantity
FROM StockItems s
JOIN OrderItems oi ON oi.StockItemId = s.StockItemId
WHERE oi.OrderId = @id";

            ExecuteInTransaction(
                restoreSql,
                "DELETE FROM Orders WHERE OrderId = @id",
                orderId);
        }

        public static void DeleteOrderItem(int orderItemId)
        {
            const string restoreSql = @"
UPDATE s
SET s.Quantity = s.Quantity + oi.Quantity
FROM StockItems s
JOIN OrderItems oi ON oi.StockItemId = s.StockItemId
WHERE oi.OrderItemId = @id";

            ExecuteInTransaction(
                restoreSql,
                "DELETE FROM OrderItems WHERE OrderItemId = @id",
                orderItemId);
        }

        public static void UpdateOrderDate(
            int orderId,
            DateTime newDate)
        {
            const string sql =
                "UPDATE Orders SET OrderDate = @date WHERE OrderId = @id";

            using (SqlConnection connection = Db.CreateConnection())
            using (var command = new SqlCommand(sql, connection))
            {
                command.Parameters.AddWithValue("@date", newDate);
                command.Parameters.AddWithValue("@id", orderId);

                connection.Open();
                command.ExecuteNonQuery();
            }
        }

        private static void ExecuteInTransaction(
            string firstSql,
            string secondSql,
            int id)
        {
            using (SqlConnection connection = Db.CreateConnection())
            {
                connection.Open();

                using (SqlTransaction transaction =
                    connection.BeginTransaction())
                {
                    foreach (string sql in new[] { firstSql, secondSql })
                    {
                        using (var command = new SqlCommand(
                            sql, connection, transaction))
                        {
                            command.Parameters.AddWithValue("@id", id);
                            command.ExecuteNonQuery();
                        }
                    }

                    transaction.Commit();
                }
            }
        }
    }
}