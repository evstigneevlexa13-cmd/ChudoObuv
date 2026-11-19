using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace ChudoObuv
{
    public partial class OrderDetailsWindow : Window
    {
        private readonly OrderRow order;

        public OrderDetailsWindow(OrderRow order)
        {
            InitializeComponent();

            this.order = order;

            Visibility adminVisibility = Session.IsAdmin
                ? Visibility.Visible
                : Visibility.Collapsed;

            DatePanel.Visibility = adminVisibility;
            DeleteItemButton.Visibility = adminVisibility;

            OrderDatePicker.SelectedDate = order.OrderDate;

            ShowHeader();
            LoadItems();
        }

        private void ShowHeader()
        {
            OrderTitleTextBlock.Text =
                "Заказ № " + order.OrderId;

            ClientTextBlock.Text =
                "Клиент: " + order.ClientName +
                ". Дата заказа: " + order.DateText;
        }

        private void LoadItems()
        {
            try
            {
                List<OrderItemRow> items =
                    OrderRepository.LoadOrderItems(order.OrderId);

                ItemsGrid.ItemsSource = items;

                TotalTextBlock.Text =
                    "Итого по заказу: " +
                    items.Sum(item => item.LineTotal)
                        .ToString("N2") +
                    " руб.";
            }
            catch (Exception ex)
            {
                ShowError(
                    "Не удалось загрузить состав заказа.",
                    ex);
            }
        }

        private void SaveDateButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            DateTime? selected = OrderDatePicker.SelectedDate;

            if (selected == null)
            {
                MessageBox.Show(
                    "Выберите дату заказа в календаре.",
                    "Дата не выбрана",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                return;
            }

            if (selected.Value.Date > DateTime.Today)
            {
                MessageBox.Show(
                    "Дата заказа не может быть позже сегодняшней. " +
                    "Выберите сегодняшнюю или более раннюю дату.",
                    "Неверная дата",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                return;
            }

            try
            {
                OrderRepository.UpdateOrderDate(
                    order.OrderId,
                    selected.Value.Date);

                order.OrderDate = selected.Value.Date;

                ShowHeader();

                MessageBox.Show(
                    "Дата заказа изменена.",
                    "Готово",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                ShowError(
                    "Не удалось изменить дату заказа.",
                    ex);
            }
        }

        private void DeleteItemButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            var item = ItemsGrid.SelectedItem as OrderItemRow;

            if (item == null)
            {
                MessageBox.Show(
                    "Выберите позицию в таблице и повторите действие.",
                    "Позиция не выбрана",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                return;
            }

            MessageBoxResult answer = MessageBox.Show(
                "Удалить из заказа позицию «" +
                item.ProductText +
                "», размер " +
                item.SizeText +
                "? Это действие нельзя отменить. Пары (" +
                item.Quantity +
                " шт.) вернутся в остатки.",
                "Удаление позиции",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (answer != MessageBoxResult.Yes)
            {
                return;
            }

            try
            {
                OrderRepository.DeleteOrderItem(item.OrderItemId);
            }
            catch (Exception ex)
            {
                ShowError(
                    "Не удалось удалить позицию.",
                    ex);

                return;
            }

            LoadItems();

            if (ItemsGrid.Items.Count == 0)
            {
                MessageBox.Show(
                    "В заказе не осталось позиций. Если заказ больше " +
                    "не нужен, удалите его в списке заказов.",
                    "Заказ пуст",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
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