using CoffeeBar.AppData;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.SqlClient;
using System.Windows;
using System.Windows.Input;

namespace CoffeeBar.Views
{
    public partial class ProductEditWindow : Window
    {
        private readonly ObservableCollection<Dictionary<string, object>> _categories;
        private readonly Dictionary<string, object> _productData;
        private readonly bool _isNew;

        // ✅ Принимаем ObservableCollection, а не List
        public ProductEditWindow(ObservableCollection<Dictionary<string, object>> categories, Dictionary<string, object> productData)
        {
            InitializeComponent();
            _categories = categories;
            _productData = productData;
            _isNew = productData == null;

            TitleText = _isNew ? "➕ Добавление нового товара" : "✏️ Редактирование товара";

            CategoryComboBox.ItemsSource = _categories;

            if (!_isNew)
            {
                LoadProductData();
            }
        }

        public string TitleText { get; set; }

        private void LoadProductData()
        {
            if (_productData != null)
            {
                ProductNameBox.Text = _productData["ProductName"].ToString();
                DescriptionBox.Text = _productData["Description"].ToString();
                PriceBox.Text = ((decimal)_productData["Price"]).ToString("F2");
                IsAvailableBox.IsChecked = (bool)_productData["IsAvailable"];

                int categoryId = (int)_productData["CategoryID"];
                foreach (var cat in _categories)
                {
                    if ((int)cat["CategoryID"] == categoryId)
                    {
                        CategoryComboBox.SelectedItem = cat;
                        break;
                    }
                }
            }
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateInput()) return;

            try
            {
                using (var connection = new SqlConnection(AppConnect.ConnectionString))
                {
                    connection.Open();

                    string query;
                    var command = new SqlCommand();

                    if (_isNew)
                    {
                        query = @"
                            INSERT INTO Products (ProductName, Description, Price, CategoryID, IsAvailable)
                            VALUES (@ProductName, @Description, @Price, @CategoryID, @IsAvailable)";
                    }
                    else
                    {
                        query = @"
                            UPDATE Products 
                            SET ProductName = @ProductName, Description = @Description, Price = @Price, 
                                CategoryID = @CategoryID, IsAvailable = @IsAvailable
                            WHERE ProductID = @ProductID";
                        command.Parameters.AddWithValue("@ProductID", _productData["ProductID"]);
                    }

                    command.CommandText = query;
                    command.Connection = connection;

                    command.Parameters.AddWithValue("@ProductName", ProductNameBox.Text.Trim());
                    command.Parameters.AddWithValue("@Description", DescriptionBox.Text.Trim());
                    command.Parameters.AddWithValue("@Price", decimal.Parse(PriceBox.Text));

                    var selectedCategory = (Dictionary<string, object>)CategoryComboBox.SelectedItem;
                    command.Parameters.AddWithValue("@CategoryID", selectedCategory["CategoryID"]);

                    command.Parameters.AddWithValue("@IsAvailable", IsAvailableBox.IsChecked == true);

                    int rowsAffected = command.ExecuteNonQuery();

                    if (rowsAffected > 0)
                    {
                        MessageBox.Show(_isNew ? "Товар успешно добавлен!" : "Товар успешно обновлен!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                        DialogResult = true;
                        Close();
                    }
                    else
                    {
                        MessageBox.Show("Не удалось сохранить изменения.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сохранении: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool ValidateInput()
        {
            if (string.IsNullOrWhiteSpace(ProductNameBox.Text))
            {
                MessageBox.Show("Введите название товара.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (!decimal.TryParse(PriceBox.Text, out decimal price) || price < 0)
            {
                MessageBox.Show("Введите корректную цену (неотрицательное число).", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (CategoryComboBox.SelectedItem == null)
            {
                MessageBox.Show("Выберите категорию.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            return true;
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
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