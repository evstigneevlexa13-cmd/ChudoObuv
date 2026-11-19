using System;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ChudoObuv
{
    public class ProductItem
    {
        private static readonly Brush LowStockBrush =
            new SolidColorBrush(Color.FromRgb(0xFF, 0x80, 0x80));

        private BitmapImage image;

        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public string ManufacturerName { get; set; }
        public int CategoryId { get; set; }
        public string CategoryName { get; set; }
        public string Description { get; set; }
        public string Composition { get; set; }
        public string ImageFileName { get; set; }
        public decimal BasePrice { get; set; }
        public decimal FinalPrice { get; set; }
        public int TotalQuantity { get; set; }

        public string TitleText
        {
            get { return ManufacturerName + " | " + ProductName; }
        }

        public string QuantityText
        {
            get
            {
                return "Количество: " +
                    (TotalQuantity > 5 ? "много" : "мало");
            }
        }

        public string PriceText
        {
            get { return FinalPrice.ToString("N2") + " руб."; }
        }

        public Brush RowBrush
        {
            get
            {
                return TotalQuantity <= 3
                    ? LowStockBrush
                    : Brushes.White;
            }
        }

        public BitmapImage Image
        {
            get
            {
                if (image == null)
                {
                    image = ImageLoader.Load(ImageFileName);
                }

                return image;
            }
        }
    }

    public class AppUser
    {
        public int UserId { get; set; }
        public string LastName { get; set; }
        public string FirstName { get; set; }
        public string MiddleName { get; set; }
        public string RoleName { get; set; }

        public string FullName
        {
            get
            {
                return (LastName + " " + FirstName + " " +
                    MiddleName).Trim();
            }
        }
    }

    public class CategoryItem
    {
        public int CategoryId { get; set; }
        public string CategoryName { get; set; }
    }

    public class StockOption
    {
        public int StockItemId { get; set; }
        public decimal SizeValue { get; set; }
        public int Quantity { get; set; }

        public string SizeText
        {
            get { return SizeValue.ToString("0.#"); }
        }

        public string DisplayText
        {
            get
            {
                return SizeText + " (в наличии: " +
                    Quantity + ")";
            }
        }
    }

    public class CartItem
    {
        public int StockItemId { get; set; }
        public string ProductName { get; set; }
        public string ManufacturerName { get; set; }
        public string SizeText { get; set; }
        public int Quantity { get; set; }
        public int MaxQuantity { get; set; }
        public decimal UnitPrice { get; set; }

        public string TitleText
        {
            get { return ManufacturerName + " | " + ProductName; }
        }

        public decimal LineTotal
        {
            get { return UnitPrice * Quantity; }
        }

        public string UnitPriceText
        {
            get { return UnitPrice.ToString("N2") + " руб."; }
        }

        public string LineTotalText
        {
            get { return LineTotal.ToString("N2") + " руб."; }
        }
    }

    public class OrderRow
    {
        public int OrderId { get; set; }
        public DateTime OrderDate { get; set; }
        public string ClientName { get; set; }
        public decimal Total { get; set; }

        public string DateText
        {
            get { return OrderDate.ToString("dd.MM.yyyy"); }
        }

        public string TotalText
        {
            get { return Total.ToString("N2") + " руб."; }
        }
    }

    public class OrderItemRow
    {
        public int OrderItemId { get; set; }
        public string ProductName { get; set; }
        public string ManufacturerName { get; set; }
        public decimal SizeValue { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }

        public string ProductText
        {
            get
            {
                return ProductName + " (" + ManufacturerName + ")";
            }
        }

        public string SizeText
        {
            get { return SizeValue.ToString("0.#"); }
        }

        public decimal LineTotal
        {
            get { return UnitPrice * Quantity; }
        }

        public string UnitPriceText
        {
            get { return UnitPrice.ToString("N2") + " руб."; }
        }

        public string LineTotalText
        {
            get { return LineTotal.ToString("N2") + " руб."; }
        }
    }
}