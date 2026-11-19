using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace ChudoObuv
{
    public partial class ProductWindow : Window
    {
        private readonly ProductItem product;

        public ProductWindow(ProductItem product)
        {
            InitializeComponent();

            this.product = product;

            ShowProduct();
            LoadSizes();
        }

        private void ShowProduct()
        {
            string priceLabel =
                product.FinalPrice < product.BasePrice
                    ? "Цена со скидкой: "
                    : "Цена: ";

            ProductImage.Source = product.Image;
            TitleTextBlock.Text = product.TitleText;
            CategoryTextBlock.Text = "Категория: " + product.CategoryName;
            PriceTextBlock.Text = priceLabel + product.PriceText;
            CompositionTextBlock.Text = product.Composition;
            DescriptionTextBlock.Text = product.Description;
        }

        private void LoadSizes()
        {
            try
            {
                List<StockOption> all =
                    ProductRepository.LoadStockOptions(product.ProductId);

                List<StockOption> available =
                    all.Where(o => o.Quantity > 0).ToList();

                SizeRangeTextBlock.Text = all.Count == 0
                    ? "Размерный ряд: нет данных"
                    : "Размерный ряд: " +
                      all.First().SizeText + " – " +
                      all.Last().SizeText;

                AvailableSizesTextBlock.Text = available.Count == 0
                    ? "Доступные для заказа размеры: сейчас нет в наличии"
                    : "Доступные для заказа размеры: " +
                      string.Join(", ", available.Select(o => o.SizeText));

                SizeComboBox.ItemsSource = available;
                SizeComboBox.SelectedIndex =
                    available.Count > 0 ? 0 : -1;

                AddButton.IsEnabled = available.Count > 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Не удалось загрузить размеры товара. " +
                    "Закройте окно и попробуйте снова.\n\n" + ex.Message,
                    "Ошибка загрузки данных",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void QuantityTextBox_PreviewTextInput(
            object sender,
            TextCompositionEventArgs e)
        {
            bool isDigits = e.Text.All(char.IsDigit);

            e.Handled = !isDigits;

            QuantityHintTextBlock.Text = isDigits
                ? string.Empty
                : "В поле «Количество пар» можно вводить только цифры.";
        }

        private void AddButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            var option = SizeComboBox.SelectedItem as StockOption;

            if (option == null)
            {
                MessageBox.Show(
                    "Выберите размер из списка.",
                    "Размер не выбран",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                return;
            }

            int quantity;

            if (!int.TryParse(QuantityTextBox.Text, out quantity) ||
                quantity <= 0)
            {
                MessageBox.Show(
                    "Укажите количество пар: целое число больше нуля.",
                    "Неверное количество",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                return;
            }

            int alreadyInCart =
                Session.GetCartQuantity(option.StockItemId);

            if (alreadyInCart + quantity > option.Quantity)
            {
                MessageBox.Show(
                    "Столько пар заказать нельзя. В наличии: " +
                    option.Quantity + ", уже в вашем заказе: " +
                    alreadyInCart +
                    ". Уменьшите количество или выберите другой размер.",
                    "Недостаточно товара",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                return;
            }

            Session.AddToCart(product, option, quantity);

            MessageBox.Show(
                "Товар добавлен в заказ. Добавьте ещё модели или " +
                "нажмите «Мой заказ» в каталоге, чтобы оформить заказ.",
                "Товар добавлен",
                MessageBoxButton.OK,
                MessageBoxImage.Information);

            Close();
        }

        private void BackButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            Close();
        }
    }
}