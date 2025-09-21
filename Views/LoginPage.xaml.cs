using CoffeeBar.AppData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace CoffeeBar.Views
{
    public partial class LoginPage : Page
    {
        public LoginPage()
        {
            InitializeComponent();
            AppConnect.model01 = new CoffeeBarDBEntities1();
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(TextBoxLogin.Text) ||
                    string.IsNullOrWhiteSpace(PasswordBox.Password))
                {
                    ShowError("Введите логин и пароль");
                    return;
                }

                var userObj = AppConnect.model01.Users
                    .FirstOrDefault(x => x.Username == TextBoxLogin.Text &&
                                         x.Password == PasswordBox.Password);

                if (userObj == null)
                {
                    ShowError("Неверный логин или пароль");
                }
                else
                {
                    AppConnect.CurrentUser = userObj;
                    MessageBox.Show($"Добро пожаловать, {userObj.FirstName}!", "Успешный вход",
                        MessageBoxButton.OK, MessageBoxImage.Information);

                    NavigationService.Navigate(new MainMenuPage());
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при авторизации: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ShowError(string message)
        {
            ErrorTextBlock.Text = message;
            ErrorBorder.Visibility = Visibility.Visible;

            DispatcherTimer timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(3);
            timer.Tick += (s, e) =>
            {
                ErrorBorder.Visibility = Visibility.Collapsed;
                timer.Stop();
            };
            timer.Start();
        }

        private void RegisterHyperlink_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            NavigationService.Navigate(new RegisterPage());
        }
    }
}