using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace ChudoObuv
{
    public partial class CartWindow : Window
    {
        public CartWindow()
        {
            InitializeComponent();
            RefreshCart();
        }

        private void RefreshCart()
        {
            CartListBox.ItemsSource = null;
            CartListBox.ItemsSource = Session.Cart.ToList();

            decimal total = Session.Cart.Sum(item => item.LineTotal);

            TotalTextBlock.Text = Session.Cart.Count == 0
                ? "В заказе пока нет товаров"
                : "Итого: " + total.ToString("N2") + " руб.";

            ConfirmButton.IsEnabled = Session.Cart.Count > 0;
        }

        private static CartItem GetItem(object sender)
        {
            return (CartItem)((Button)sender).DataContext;
        }

        private void PlusButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            CartItem item = GetItem(sender);

            if (item.Quantity >= item.MaxQuantity)
            {
                MessageBox.Show(
                    "Больше пар этого размера в наличии нет: доступно " +
                    item.MaxQuantity + ".",
                    "Недостаточно товара",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                return;
            }

            item.Quantity++;
            RefreshCart();
        }

        private void MinusButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            CartItem item = GetItem(sender);

            if (item.Quantity <= 1)
            {
                MessageBox.Show(
                    "Минимальное количество — одна пара. Чтобы убрать " +
                    "товар из заказа, нажмите «Удалить».",
                    "Минимальное количество",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                return;
            }

            item.Quantity--;
            RefreshCart();
        }

        private void RemoveButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            Session.Cart.Remove(GetItem(sender));
            RefreshCart();
        }

        private void ConfirmButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            decimal total = Session.Cart.Sum(item => item.LineTotal);

            MessageBoxResult answer = MessageBox.Show(
                "Оформить заказ на сумму " +
                total.ToString("N2") +
                " руб.? После подтверждения выбранные пары будут закреплены за вами.",
                "Подтверждение заказа",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (answer != MessageBoxResult.Yes)
            {
                return;
            }

            try
            {
                int orderId = OrderRepository.CreateOrder(
                    Session.CurrentUser.UserId,
                    Session.Cart);

                Session.Cart.Clear();

                MessageBox.Show(
                    "Заказ № " + orderId +
                    " оформлен. Спасибо за покупку!",
                    "Заказ оформлен",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                Close();
            }
            catch (InvalidOperationException ex)
            {
                MessageBox.Show(
                    ex.Message,
                    "Заказ не оформлен",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Не удалось оформить заказ. Проверьте подключение " +
                    "к базе данных и повторите попытку.\n\n" + ex.Message,
                    "Ошибка оформления",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void CancelOrderButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            MessageBoxResult answer = MessageBox.Show(
                "Отказаться от заказа? Все выбранные товары будут " +
                "убраны из заказа, это действие нельзя отменить.",
                "Отказ от заказа",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (answer == MessageBoxResult.Yes)
            {
                Session.Cart.Clear();
                Close();
            }
        }

        private void BackButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            Close();
        }
    }
}