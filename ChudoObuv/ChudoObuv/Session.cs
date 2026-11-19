using System.Collections.Generic;
using System.Linq;

namespace ChudoObuv
{
    public static class Session
    {
        private const string AdminRole = "Администратор";
        private const string ManagerRole = "Менеджер";

        public static AppUser CurrentUser { get; set; }

        public static List<CartItem> Cart { get; } =
            new List<CartItem>();

        public static bool IsAuthorized
        {
            get { return CurrentUser != null; }
        }

        public static bool IsAdmin
        {
            get
            {
                return CurrentUser != null &&
                    CurrentUser.RoleName == AdminRole;
            }
        }

        public static bool IsStaff
        {
            get
            {
                return IsAdmin ||
                    (CurrentUser != null &&
                     CurrentUser.RoleName == ManagerRole);
            }
        }

        public static int GetCartQuantity(int stockItemId)
        {
            CartItem item = Cart.FirstOrDefault(
                c => c.StockItemId == stockItemId);

            return item == null ? 0 : item.Quantity;
        }

        public static void AddToCart(
            ProductItem product,
            StockOption option,
            int quantity)
        {
            CartItem item = Cart.FirstOrDefault(
                c => c.StockItemId == option.StockItemId);

            if (item != null)
            {
                item.Quantity += quantity;
                return;
            }

            Cart.Add(new CartItem
            {
                StockItemId = option.StockItemId,
                ProductName = product.ProductName,
                ManufacturerName = product.ManufacturerName,
                SizeText = option.SizeText,
                Quantity = quantity,
                MaxQuantity = option.Quantity,
                UnitPrice = product.FinalPrice
            });
        }

        public static void Logout()
        {
            CurrentUser = null;
            Cart.Clear();
        }
    }
}