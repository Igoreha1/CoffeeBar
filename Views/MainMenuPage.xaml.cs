using CoffeeBar.AppData;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace CoffeeBar.Views
{
    public partial class MainMenuPage : Page
    {
        private List<dynamic> _categories;
        private List<dynamic> _products;
        private List<dynamic> _filteredProducts;
        private Dictionary<int, int> _cartItems = new Dictionary<int, int>();
        private int _currentCategoryId = 0;
        private string _currentSort = "default";
        private decimal _totalPrice = 0;
        private bool _isAdmin = false;

        public MainMenuPage()
        {
            InitializeComponent();
            Loaded += OnPageLoaded;
        }

        private void OnPageLoaded(object sender, RoutedEventArgs e)
        {
            try
            {
                UsernameTextBlock.Text = $"Добро пожаловать, {AppConnect.CurrentUser?.FirstName}!";
                CheckAdminStatus();
                LoadCategories();
                LoadProducts();
                UpdateTotalPrice();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка инициализации: {ex.Message}");
            }
        }

        private void UpdateTotalPrice()
        {
            _totalPrice = 0;
            foreach (var item in _cartItems)
            {
                var product = _products.FirstOrDefault(p => p.ProductID == item.Key);
                if (product != null)
                {
                    _totalPrice += product.Price * item.Value;
                }
            }
            TotalPriceTextBlock.Text = $"Итого: {_totalPrice} руб";
        }

        private void CheckAdminStatus()
        {
            _isAdmin = AppConnect.CurrentUser?.RoleID == 1;
            AdminPanelButton.Visibility = _isAdmin ? Visibility.Visible : Visibility.Collapsed;
        }

        private void LoadCategories()
        {
            try
            {
                using (var connection = new SqlConnection(AppConnect.ConnectionString))
                {
                    connection.Open();
                    var command = new SqlCommand("SELECT CategoryID, CategoryName FROM Categories ORDER BY CategoryName", connection);

                    _categories = new List<dynamic>();
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            _categories.Add(new
                            {
                                CategoryID = reader.GetInt32(0),
                                CategoryName = reader.GetString(1)
                            });
                        }
                    }

                    CategoryComboBox.Items.Clear();
                    CategoryComboBox.Items.Add(new ComboBoxItem { Content = "Все категории", Tag = "0", IsSelected = true });

                    foreach (var category in _categories)
                    {
                        var item = new ComboBoxItem
                        {
                            Content = category.CategoryName,
                            Tag = category.CategoryID.ToString()
                        };
                        CategoryComboBox.Items.Add(item);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки категорий: {ex.Message}");
            }
        }

        private void LoadProducts()
        {
            try
            {
                using (var connection = new SqlConnection(AppConnect.ConnectionString))
                {
                    connection.Open();

                    string whereClause = _currentCategoryId == 0
                        ? "WHERE p.IsAvailable = 1"
                        : $"WHERE p.CategoryID = {_currentCategoryId} AND p.IsAvailable = 1";

                    string query = $@"
                        SELECT p.ProductID, p.ProductName, p.Description, p.Price, 
                               c.CategoryName, p.CategoryID, p.IsAvailable
                        FROM Products p
                        INNER JOIN Categories c ON p.CategoryID = c.CategoryID
                        {whereClause}";

                    _products = new List<dynamic>();
                    using (var reader = new SqlCommand(query, connection).ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            _products.Add(new
                            {
                                ProductID = reader.GetInt32(0),
                                ProductName = reader.GetString(1),
                                Description = reader.IsDBNull(2) ? string.Empty : reader.GetString(2),
                                Price = reader.GetDecimal(3),
                                CategoryName = reader.GetString(4),
                                CategoryID = reader.GetInt32(5),
                                IsAvailable = reader.GetBoolean(6)
                            });
                        }
                    }

                    ApplyFilters();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки продуктов: {ex.Message}");
            }
        }

        private void ApplyFilters()
        {
            if (_products == null || ProductsItemsControl == null) return;

            string searchText = SearchTextBox.Text?.ToLower() ?? string.Empty;
            _filteredProducts = _products
                .Where(p => p.ProductName.ToLower().Contains(searchText) ||
                           p.Description.ToLower().Contains(searchText) ||
                           p.CategoryName.ToLower().Contains(searchText))
                .ToList();

            switch (_currentSort)
            {
                case "price_asc":
                    _filteredProducts = _filteredProducts.OrderBy(p => p.Price).ToList();
                    break;
                case "price_desc":
                    _filteredProducts = _filteredProducts.OrderByDescending(p => p.Price).ToList();
                    break;
                case "name_asc":
                    _filteredProducts = _filteredProducts.OrderBy(p => p.ProductName).ToList();
                    break;
                case "name_desc":
                    _filteredProducts = _filteredProducts.OrderByDescending(p => p.ProductName).ToList();
                    break;
                default:
                    _filteredProducts = _filteredProducts.OrderBy(p => p.ProductID).ToList();
                    break;
            }

            ProductsItemsControl.ItemsSource = null;
            ProductsItemsControl.ItemsSource = _filteredProducts;
        }

        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            ApplyFilters();
        }

        private void CategoryComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CategoryComboBox.SelectedItem is ComboBoxItem selectedItem && selectedItem.Tag != null)
            {
                if (int.TryParse(selectedItem.Tag.ToString(), out int categoryId))
                {
                    _currentCategoryId = categoryId;
                }
                else
                {
                    _currentCategoryId = 0;
                }
                LoadProducts();
            }
        }

        private void SortComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (SortComboBox.SelectedItem is ComboBoxItem selectedItem && selectedItem.Tag != null)
            {
                _currentSort = selectedItem.Tag.ToString();
                ApplyFilters();
            }
        }

        private void AddToCartButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag != null)
            {
                if (int.TryParse(button.Tag.ToString(), out int productId))
                {
                    var product = _products.FirstOrDefault(p => p.ProductID == productId);
                    if (product != null)
                    {
                        if (!_cartItems.ContainsKey(productId))
                        {
                            _cartItems[productId] = 1;
                        }
                        else
                        {
                            _cartItems[productId]++;
                        }

                        UpdateTotalPrice();
                        LoadProducts();

                        button.Content = "В корзине";
                        button.IsEnabled = false;
                    }
                }
            }
        }

        private void CartButton_Click(object sender, RoutedEventArgs e)
        {
            var cartWindow = new CartWindow(_products, _cartItems, CheckoutFromCart, OnCartUpdated);
            cartWindow.Owner = Window.GetWindow(this);
            bool? result = cartWindow.ShowDialog();

            if (result == true)
            {
                CheckoutFromCart();
            }
            else
            {
                UpdateTotalPrice();
                LoadProducts();
            }
        }

        private void OnCartUpdated()
        {
            UpdateTotalPrice();
            LoadProducts();
        }

        private void CheckoutFromCart()
        {
            CheckoutButton_Click(null, null);
        }

        private void CheckoutButton_Click(object sender, RoutedEventArgs e)
        {
            if (_cartItems.Count == 0)
            {
                MessageBox.Show("Корзина пуста!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                using (var connection = new SqlConnection(AppConnect.ConnectionString))
                {
                    connection.Open();
                    using (var transaction = connection.BeginTransaction())
                    {
                        try
                        {
                            var orderCommand = new SqlCommand(
                                @"INSERT INTO Orders (UserID, OrderDate, TotalAmount, StatusID) 
                                  VALUES (@UserID, @OrderDate, @TotalAmount, @StatusID);
                                  SELECT SCOPE_IDENTITY();",
                                connection, transaction);

                            orderCommand.Parameters.AddWithValue("@UserID", AppConnect.CurrentUser.UserID);
                            orderCommand.Parameters.AddWithValue("@OrderDate", DateTime.Now);
                            orderCommand.Parameters.AddWithValue("@TotalAmount", _totalPrice);
                            orderCommand.Parameters.AddWithValue("@StatusID", 2);

                            int orderId = Convert.ToInt32(orderCommand.ExecuteScalar());

                            foreach (var item in _cartItems)
                            {
                                var product = _products.First(p => p.ProductID == item.Key);

                                var itemCommand = new SqlCommand(
                                    @"INSERT INTO OrderItems (OrderID, ProductID, Quantity, UnitPrice) 
                                      VALUES (@OrderID, @ProductID, @Quantity, @UnitPrice)",
                                    connection, transaction);

                                itemCommand.Parameters.AddWithValue("@OrderID", orderId);
                                itemCommand.Parameters.AddWithValue("@ProductID", item.Key);
                                itemCommand.Parameters.AddWithValue("@Quantity", item.Value);
                                itemCommand.Parameters.AddWithValue("@UnitPrice", product.Price);

                                itemCommand.ExecuteNonQuery();
                            }

                            transaction.Commit();

                            MessageBox.Show($"Заказ №{orderId} оформлен успешно!\nСумма: {_totalPrice} руб",
                                "Заказ оформлен", MessageBoxButton.OK, MessageBoxImage.Information);

                            _cartItems.Clear();
                            UpdateTotalPrice();
                            LoadProducts();
                        }
                        catch
                        {
                            transaction.Rollback();
                            throw;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка оформления заказа: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LogoutButton_Click(object sender, RoutedEventArgs e)
        {
            AppConnect.CurrentUser = null;
            NavigationService?.Navigate(new LoginPage());
        }

        private void AdminPanelButton_Click(object sender, RoutedEventArgs e)
        {
            if (_isAdmin)
            {
                var adminWindow = new AdminPanelWindow();
                adminWindow.Owner = Window.GetWindow(this);
                adminWindow.ShowDialog();
            }
        }
    }
}