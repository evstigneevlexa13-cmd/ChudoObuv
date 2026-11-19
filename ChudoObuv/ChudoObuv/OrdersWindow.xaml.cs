using System;
using System.Windows;
using System.Windows.Input;

namespace ChudoObuv
{
    public partial class OrdersWindow : Window
    {
        public OrdersWindow()
        {
            InitializeComponent();
            LoadOrders();
        }

        private void LoadOrders()
        {
            try
            {
                OrdersGrid.ItemsSource =
                    OrderRepository.LoadOrders();
            }
            catch (Exception ex)
            {
                ShowError(
                    "Не удалось загрузить список заказов.",
                    ex);
            }
        }

        private OrderRow GetSelectedOrder()
        {
            var order = OrdersGrid.SelectedItem as OrderRow;

            if (order == null)
            {
                MessageBox.Show(
                    "Выберите заказ в списке и повторите действие.",
                    "Заказ не выбран",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }

            return order;
        }

        private void AddOrderButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            var catalog = new MainWindow(true);

            catalog.Owner = this;
            catalog.ShowDialog();

            LoadOrders();
        }

        private void DetailsButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            OpenDetails(GetSelectedOrder());
        }

        private void OrdersGrid_MouseDoubleClick(
            object sender,
            MouseButtonEventArgs e)
        {
            OpenDetails(OrdersGrid.SelectedItem as OrderRow);
        }

        private void OpenDetails(OrderRow order)
        {
            if (order == null)
            {
                return;
            }

            var window = new OrderDetailsWindow(order);

            window.Owner = this;
            window.ShowDialog();

            LoadOrders();
        }

        private void DeleteButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            OrderRow order = GetSelectedOrder();

            if (order == null)
            {
                return;
            }

            MessageBoxResult answer = MessageBox.Show(
                "Удалить заказ № " + order.OrderId +
                " от " + order.DateText +
                " (" + order.ClientName + ")? " +
                "Заказ будет удалён безвозвратно, " +
                "а его товары вернутся в остатки.",
                "Удаление заказа",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (answer != MessageBoxResult.Yes)
            {
                return;
            }

            try
            {
                OrderRepository.DeleteOrder(order.OrderId);
            }
            catch (Exception ex)
            {
                ShowError("Не удалось удалить заказ.", ex);
            }

            LoadOrders();
        }

        private void BackButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            Close();
        }

        private static void ShowError(
            string message,
            Exception ex)
        {
            MessageBox.Show(
                message +
                " Проверьте подключение к базе данных и повторите попытку.\n\n" +
                ex.Message,
                "Ошибка",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }
}