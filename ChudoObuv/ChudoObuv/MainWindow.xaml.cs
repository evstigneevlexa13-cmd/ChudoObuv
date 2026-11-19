using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace ChudoObuv
{
    public partial class MainWindow : Window
    {
        private readonly bool isOrderMode;

        private List<ProductItem> allProducts =
            new List<ProductItem>();

        private bool isReady;

        public MainWindow(bool isOrderMode)
        {
            InitializeComponent();

            LogoImage.Source =
                ImageLoader.Load("Чудо Обувь.png");

            this.isOrderMode = isOrderMode;

            PrepareInterface();
            LoadCategories();
            ReloadCatalog();

            isReady = true;
            ApplyFilters();
        }

        private void PrepareInterface()
        {
            UserNameTextBlock.Text = Session.IsAuthorized
                ? Session.CurrentUser.FullName
                : "Гость";

            FilterPanel.Visibility = Session.IsAuthorized
                ? Visibility.Visible
                : Visibility.Collapsed;

            CartButton.Visibility = Session.IsAuthorized
                ? Visibility.Visible
                : Visibility.Collapsed;
            OrdersButton.Visibility = Session.IsStaff && !isOrderMode
    ? Visibility.Visible
    : Visibility.Collapsed;

            BackButton.Content = isOrderMode
                ? "Назад"
                : "Выйти";

            if (isOrderMode)
            {
                Title = "Чудо Обувь — Добавление заказа";
            }
        }

        private void LoadCategories()
        {
            try
            {
                CategoryComboBox.ItemsSource =
                    ProductRepository.LoadCategories();

                CategoryComboBox.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                ShowLoadError(ex);
            }
        }

        private void ReloadCatalog()
        {
            try
            {
                allProducts =
                    ProductRepository.LoadCatalog(DateTime.Today);
            }
            catch (Exception ex)
            {
                ShowLoadError(ex);
            }

            ApplyFilters();
        }

        private void ApplyFilters()
        {
            if (!isReady)
            {
                return;
            }

            IEnumerable<ProductItem> result = allProducts;

            var category =
                CategoryComboBox.SelectedItem as CategoryItem;

            if (category != null && category.CategoryId != 0)
            {
                result = result.Where(
                    p => p.CategoryId == category.CategoryId);
            }

            string searchText = SearchTextBox.Text.Trim();

            if (searchText.Length > 0)
            {
                result = result.Where(
                    p => Contains(p.ProductName, searchText) ||
                         Contains(p.Description, searchText));
            }

            switch (SortComboBox.SelectedIndex)
            {
                case 1:
                    result = result.OrderBy(p => p.FinalPrice);
                    break;

                case 2:
                    result = result.OrderByDescending(
                        p => p.FinalPrice);
                    break;
            }

            ProductsListBox.ItemsSource = result.ToList();
        }

        private static bool Contains(string text, string part)
        {
            return text != null &&
                text.IndexOf(
                    part,
                    StringComparison.CurrentCultureIgnoreCase) >= 0;
        }

        private void SearchTextBox_TextChanged(
            object sender,
            TextChangedEventArgs e)
        {
            ApplyFilters();
        }

        private void FilterChanged(
            object sender,
            SelectionChangedEventArgs e)
        {
            ApplyFilters();
        }

        private void ProductsListBox_SelectionChanged(
            object sender,
            SelectionChangedEventArgs e)
        {
            var product =
                ProductsListBox.SelectedItem as ProductItem;

            if (product == null)
            {
                return;
            }

            ProductsListBox.SelectedItem = null;

            if (!Session.IsAuthorized)
            {
                MessageBox.Show(
                    "Чтобы заказать обувь, войдите в систему под своим " +
                    "логином. Для этого нажмите «Выйти» и введите логин.",
                    "Требуется вход",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                return;
            }

            var window = new ProductWindow(product);

            window.Owner = this;
            window.ShowDialog();

            ReloadCatalog();
        }

        private void CartButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            var window = new CartWindow();

            window.Owner = this;
            window.ShowDialog();

            ReloadCatalog();
        }

        private void OrdersButton_Click(
    object sender,
    RoutedEventArgs e)
        {
            var window = new OrdersWindow();

            window.Owner = this;
            window.ShowDialog();

            ReloadCatalog();
        }

        private void BackButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            if (!isOrderMode)
            {
                Session.Logout();

                var loginWindow = new LoginWindow();
                loginWindow.Show();
            }

            Close();
        }

        private static void ShowLoadError(Exception ex)
        {
            MessageBox.Show(
                "Не удалось загрузить данные. Проверьте, что SQL Server " +
                "запущен и база ChudoObuv существует.\n\n" + ex.Message,
                "Ошибка загрузки данных",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }
}