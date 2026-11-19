using System;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;

namespace ChudoObuv
{
    public partial class LoginWindow : Window
    {
        public LoginWindow()
        {
            InitializeComponent();
            LogoImage.Source = ImageLoader.Load("Чудо Обувь.png");
        }

        private void LoginTextBox_PreviewTextInput(
            object sender,
            TextCompositionEventArgs e)
        {
            bool isValid = Regex.IsMatch(e.Text, "^[a-zA-Z0-9]+$");

            e.Handled = !isValid;

            LoginHintTextBlock.Text = isValid
                ? string.Empty
                : "Логин состоит только из латинских букв и цифр. " +
                  "Переключите раскладку клавиатуры на английскую.";
        }

        private void LoginButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            string login = LoginTextBox.Text.Trim();

            if (login.Length == 0)
            {
                MessageBox.Show(
                    "Введите логин. Пароль вводить не нужно.",
                    "Логин не указан",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                return;
            }

            try
            {
                AppUser user = UserRepository.FindByLogin(login);

                if (user == null)
                {
                    MessageBox.Show(
                        "Пользователя с логином «" + login +
                        "» нет в системе. Проверьте логин и попробуйте " +
                        "ещё раз или войдите как гость.",
                        "Ошибка входа",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);

                    return;
                }

                Session.CurrentUser = user;
                OpenCatalog();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Не удалось связаться с базой данных. " +
                    "Проверьте, что SQL Server запущен.\n\n" + ex.Message,
                    "Ошибка подключения",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void GuestButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            Session.CurrentUser = null;
            OpenCatalog();
        }

        private void OpenCatalog()
        {
            var catalog = new MainWindow(false);

            catalog.Show();
            Close();
        }
    }
}