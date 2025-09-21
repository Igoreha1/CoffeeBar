using CoffeeBar.AppData;
using System.Collections.ObjectModel;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace CoffeeBar.Views
{
    public partial class AdminPanelWindow : Window
    {
        private ObservableCollection<Dictionary<string, object>> _products;
        private ObservableCollection<Dictionary<string, object>> _categories;

        public AdminPanelWindow()
        {
            InitializeComponent();
            Loaded += AdminPanelWindow_Loaded;

            _products = new ObservableCollection<Dictionary<string, object>>();
            _categories = new ObservableCollection<Dictionary<string, object>>();

            ProductsDataGrid.ItemsSource = _products;
        }

        private void AdminPanelWindow_Loaded(object sender, RoutedEventArgs e)
        {
            LoadCategories();
            LoadProducts();
        }

        private void LoadCategories()
        {
            try
            {
                using (var connection = new SqlConnection(AppConnect.ConnectionString))
                {
                    connection.Open();
                    var command = new SqlCommand("SELECT CategoryID, CategoryName FROM Categories ORDER BY CategoryName", connection);

                    _categories.Clear();

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            _categories.Add(new Dictionary<string, object>
                            {
                                ["CategoryID"] = reader.GetInt32(0),
                                ["CategoryName"] = reader.GetString(1)
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки категорий: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadProducts()
        {
            try
            {
                using (var connection = new SqlConnection(AppConnect.ConnectionString))
                {
                    connection.Open();
                    var query = @"
                SELECT p.ProductID, p.ProductName, p.Description, p.Price, 
                       p.CategoryID, c.CategoryName, p.IsAvailable
                FROM Products p
                INNER JOIN Categories c ON p.CategoryID = c.CategoryID
                ORDER BY p.ProductID";

                    _products.Clear();

                    using (var command = new SqlCommand(query, connection))
                    {
                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                _products.Add(new Dictionary<string, object>
                                {
                                    ["ProductID"] = reader.GetInt32(0),
                                    ["ProductName"] = reader.IsDBNull(1) ? "" : reader.GetString(1),
                                    ["Description"] = reader.IsDBNull(2) ? "" : reader.GetString(2),
                                    ["Price"] = reader.GetDecimal(3),
                                    ["CategoryID"] = reader.GetInt32(4),
                                    ["CategoryName"] = reader.IsDBNull(5) ? "" : reader.GetString(5),
                                    ["IsAvailable"] = reader.GetBoolean(6)
                                });
                            }
                        }
                    }

                    System.Diagnostics.Debug.WriteLine($"Загружено товаров: {_products.Count}");
                    foreach (var product in _products)
                    {
                        System.Diagnostics.Debug.WriteLine($"Товар: {product["ProductName"]}, Цена: {product["Price"]}");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки товаров: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                System.Diagnostics.Debug.WriteLine($"Ошибка: {ex.Message}");
            }
        }

        private void ProductsDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            bool hasSelection = ProductsDataGrid.SelectedItem != null;
            EditButton.IsEnabled = hasSelection;
            DeleteButton.IsEnabled = hasSelection;
        }

        private void ProductsDataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (ProductsDataGrid.SelectedItem is Dictionary<string, object> selectedProduct)
            {
                OpenEditWindow(selectedProduct);
            }
        }

        private void AddProduct_Click(object sender, RoutedEventArgs e)
        {
            var editWindow = new ProductEditWindow(_categories, null);
            if (editWindow.ShowDialog() == true)
            {
                LoadProducts();
            }
        }

        private void EditProduct_Click(object sender, RoutedEventArgs e)
        {
            if (ProductsDataGrid.SelectedItem is Dictionary<string, object> selectedProduct)
            {
                OpenEditWindow(selectedProduct);
            }
        }

        private void OpenEditWindow(Dictionary<string, object> productData)
        {
            var editWindow = new ProductEditWindow(_categories, productData);
            if (editWindow.ShowDialog() == true)
            {
                LoadProducts();
            }
        }

        private void DeleteProduct_Click(object sender, RoutedEventArgs e)
        {
            if (ProductsDataGrid.SelectedItem is Dictionary<string, object> selectedProduct)
            {
                string productName = selectedProduct["ProductName"].ToString();
                int productId = (int)selectedProduct["ProductID"];

                var result = MessageBox.Show($"Вы уверены, что хотите удалить товар \"{productName}\"?",
                    "Подтверждение удаления", MessageBoxButton.YesNo, MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        using (var connection = new SqlConnection(AppConnect.ConnectionString))
                        {
                            connection.Open();
                            var command = new SqlCommand(
                                "DELETE FROM Products WHERE ProductID = @ProductID",
                                connection);

                            command.Parameters.AddWithValue("@ProductID", productId);
                            int rowsAffected = command.ExecuteNonQuery();

                            if (rowsAffected > 0)
                            {
                                MessageBox.Show("Товар успешно удален", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                                LoadProducts();
                            }
                            else
                            {
                                MessageBox.Show("Товар не найден или уже удален", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка при удалении товара: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

        private void Refresh_Click(object sender, RoutedEventArgs e)
        {
            LoadProducts();
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
                DragMove();
        }
    }
}