using CoffeeBar.AppData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace CoffeeBar.Views
{
    public partial class RegisterPage : Page
    {
        public RegisterPage()
        {
            InitializeComponent();
            SetupInputRestrictions();
        }

        private void SetupInputRestrictions()
        {
            TextBoxFirstName.MaxLength = 50;
            TextBoxFirstName.PreviewTextInput += (s, e) =>
            {
                e.Handled = !IsValidNameText(e.Text);
            };
            TextBoxFirstName.PreviewKeyDown += (s, e) =>
            {
                if (e.Key == Key.Space) e.Handled = TextBoxFirstName.Text.Length >= 50;
            };

            TextBoxLastName.MaxLength = 50;
            TextBoxLastName.PreviewTextInput += (s, e) =>
            {
                e.Handled = !IsValidNameText(e.Text);
            };
            TextBoxLastName.PreviewKeyDown += (s, e) =>
            {
                if (e.Key == Key.Space) e.Handled = TextBoxLastName.Text.Length >= 50;
            };

            TextBoxLogin.MaxLength = 20;
            TextBoxLogin.PreviewTextInput += (s, e) =>
            {
                e.Handled = !IsValidLoginText(e.Text);
            };

            TextBoxEmail.MaxLength = 100;
            TextBoxEmail.PreviewTextInput += (s, e) =>
            {
                // Разрешаем все символы, но проверяем при валидации
            };

            TextBoxPhone.MaxLength = 20;
            TextBoxPhone.PreviewTextInput += (s, e) =>
            {
                e.Handled = !IsValidPhoneText(e.Text);
            };

            PasswordBox.MaxLength = 50;
            ConfirmPasswordBox.MaxLength = 50;
        }

        private bool IsValidNameText(string text)
        {
            return Regex.IsMatch(text, @"^[a-zA-Zа-яА-ЯёЁ\s-]*$");
        }

        private bool IsValidLoginText(string text)
        {
            return Regex.IsMatch(text, @"^[a-zA-Z0-9_]*$");
        }

        private bool IsValidPhoneText(string text)
        {
            return Regex.IsMatch(text, @"^[0-9\s\(\)\+\-]*$");
        }

        private void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateForm())
            {
                return;
            }

            string firstName = TextBoxFirstName.Text.Trim();
            string lastName = TextBoxLastName.Text.Trim();
            string username = TextBoxLogin.Text.Trim();
            string email = TextBoxEmail.Text.Trim().ToLower();
            string password = PasswordBox.Password;
            string phone = TextBoxPhone.Text.Trim();

            try
            {
                if (AppConnect.model01.Users.Any(u => u.Username == username))
                {
                    MessageBox.Show("Этот логин уже занят", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    TextBoxLogin.Focus();
                    return;
                }

                if (AppConnect.model01.Users.Any(u => u.Email == email))
                {
                    MessageBox.Show("Этот email уже зарегистрирован", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    TextBoxEmail.Focus();
                    return;
                }

                var clientRole = AppConnect.model01.Roles.FirstOrDefault(r => r.RoleName == "Клиент");
                if (clientRole == null)
                {
                    MessageBox.Show("Ошибка: роль 'Клиент' не найдена в системе", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                Users newUser = new Users
                {
                    FirstName = firstName,
                    LastName = lastName,
                    Username = username,
                    Email = email,
                    Password = password,
                    Phone = string.IsNullOrWhiteSpace(phone) ? null : phone,
                    RoleID = clientRole.RoleID,
                    RegistrationDate = DateTime.Now
                };

                AppConnect.model01.Users.Add(newUser);
                AppConnect.model01.SaveChanges();

                MessageBox.Show("Регистрация прошла успешно! Теперь вы можете войти в систему.",
                    "Успешная регистрация", MessageBoxButton.OK, MessageBoxImage.Information);

                NavigationService?.Navigate(new LoginPage());
            }
            catch (System.Data.Entity.Validation.DbEntityValidationException ex)
            {
                var errorMessages = ex.EntityValidationErrors
                    .SelectMany(x => x.ValidationErrors)
                    .Select(x => x.ErrorMessage);

                string fullErrorMessage = string.Join("\n", errorMessages);
                MessageBox.Show($"Ошибки валидации:\n{fullErrorMessage}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка регистрации: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool ValidateForm()
        {
            if (string.IsNullOrWhiteSpace(TextBoxFirstName.Text))
            {
                MessageBox.Show("Введите имя", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                TextBoxFirstName.Focus();
                return false;
            }

            if (TextBoxFirstName.Text.Length < 2)
            {
                MessageBox.Show("Имя должно содержать минимум 2 символа", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                TextBoxFirstName.Focus();
                return false;
            }

            if (!Regex.IsMatch(TextBoxFirstName.Text, @"^[a-zA-Zа-яА-ЯёЁ\s\-]+$"))
            {
                MessageBox.Show("Имя может содержать только буквы, пробелы и дефисы", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                TextBoxFirstName.Focus();
                return false;
            }

            if (string.IsNullOrWhiteSpace(TextBoxLastName.Text))
            {
                MessageBox.Show("Введите фамилию", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                TextBoxLastName.Focus();
                return false;
            }

            if (TextBoxLastName.Text.Length < 2)
            {
                MessageBox.Show("Фамилия должна содержать минимум 2 символа", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                TextBoxLastName.Focus();
                return false;
            }

            if (!Regex.IsMatch(TextBoxLastName.Text, @"^[a-zA-Zа-яА-ЯёЁ\s\-]+$"))
            {
                MessageBox.Show("Фамилия может содержать только буквы, пробелы и дефисы", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                TextBoxLastName.Focus();
                return false;
            }

            if (string.IsNullOrWhiteSpace(TextBoxLogin.Text))
            {
                MessageBox.Show("Введите логин", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                TextBoxLogin.Focus();
                return false;
            }

            if (TextBoxLogin.Text.Length < 3)
            {
                MessageBox.Show("Логин должен содержать минимум 3 символа", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                TextBoxLogin.Focus();
                return false;
            }

            if (!Regex.IsMatch(TextBoxLogin.Text, @"^[a-zA-Z0-9_]+$"))
            {
                MessageBox.Show("Логин может содержать только буквы, цифры и подчеркивания", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                TextBoxLogin.Focus();
                return false;
            }

            if (string.IsNullOrWhiteSpace(TextBoxEmail.Text))
            {
                MessageBox.Show("Введите email", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                TextBoxEmail.Focus();
                return false;
            }

            if (!IsValidEmail(TextBoxEmail.Text))
            {
                MessageBox.Show("Введите корректный email адрес", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                TextBoxEmail.Focus();
                return false;
            }

            if (!string.IsNullOrWhiteSpace(TextBoxPhone.Text) && !IsValidPhone(TextBoxPhone.Text))
            {
                MessageBox.Show("Введите корректный номер телефона", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                TextBoxPhone.Focus();
                return false;
            }

            if (PasswordBox.Password.Length < 6)
            {
                MessageBox.Show("Пароль должен содержать минимум 6 символов", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                PasswordBox.Focus();
                return false;
            }

            if (PasswordBox.Password != ConfirmPasswordBox.Password)
            {
                MessageBox.Show("Пароли не совпадают", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                ConfirmPasswordBox.Focus();
                return false;
            }

            return true;
        }

        private bool IsValidEmail(string email)
        {
            string pattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
            return Regex.IsMatch(email, pattern);
        }

        private bool IsValidPhone(string phone)
        {
            string digitsOnly = Regex.Replace(phone, @"[^\d]", "");
            return digitsOnly.Length >= 10 && digitsOnly.Length <= 15;
        }

        private void LoginHyperlink_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            NavigationService?.Navigate(new LoginPage());
        }
    }
}