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
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace CoffeeBar.Views
{
    public partial class CartWindow : Window
    {
        private readonly List<dynamic> _products;
        private readonly Dictionary<int, int> _cartItems;
        private readonly Action _onCheckout;
        private readonly Action _onCartUpdated;

        public CartWindow(List<dynamic> products, Dictionary<int, int> cartItems,
                         Action onCheckout = null, Action onCartUpdated = null)
        {
            InitializeComponent();
            _products = products;
            _cartItems = cartItems;
            _onCheckout = onCheckout;
            _onCartUpdated = onCartUpdated;
            LoadCartItems();
            UpdateTotal();
        }

        private void LoadCartItems()
        {
            var cartList = new List<dynamic>();
            foreach (var item in _cartItems)
            {
                var product = _products.FirstOrDefault(p => p.ProductID == item.Key);
                if (product != null)
                {
                    cartList.Add(new
                    {
                        ProductID = product.ProductID,
                        ProductName = product.ProductName,
                        Price = product.Price,
                        Quantity = item.Value,
                        TotalPrice = product.Price * item.Value
                    });
                }
            }
            CartItemsControl.ItemsSource = cartList;
        }

        private void UpdateTotal()
        {
            decimal total = 0;
            foreach (var item in _cartItems)
            {
                var product = _products.FirstOrDefault(p => p.ProductID == item.Key);
                if (product != null)
                {
                    total += product.Price * item.Value;
                }
            }
            TotalTextBlock.Text = $"Итого: {total} руб";
        }

        private void IncreaseQuantity_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag != null && int.TryParse(button.Tag.ToString(), out int productId))
            {
                if (_cartItems.ContainsKey(productId))
                {
                    _cartItems[productId]++;
                    LoadCartItems();
                    UpdateTotal();
                    _onCartUpdated?.Invoke();
                }
            }
        }

        private void DecreaseQuantity_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag != null && int.TryParse(button.Tag.ToString(), out int productId))
            {
                if (_cartItems.ContainsKey(productId))
                {
                    _cartItems[productId]--;
                    if (_cartItems[productId] <= 0)
                    {
                        _cartItems.Remove(productId);
                    }
                    LoadCartItems();
                    UpdateTotal();
                    _onCartUpdated?.Invoke();
                }
            }
        }

        private void RemoveItem_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag != null && int.TryParse(button.Tag.ToString(), out int productId))
            {
                if (_cartItems.ContainsKey(productId))
                {
                    _cartItems.Remove(productId);
                    LoadCartItems();
                    UpdateTotal();
                    _onCartUpdated?.Invoke();
                }
            }
        }

        private void ClearCart_Click(object sender, RoutedEventArgs e)
        {
            _cartItems.Clear();
            LoadCartItems();
            UpdateTotal();
            _onCartUpdated?.Invoke();
        }

        private void Checkout_Click(object sender, RoutedEventArgs e)
        {
            if (_cartItems.Count == 0)
            {
                MessageBox.Show("Корзина пуста!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            DialogResult = true;
            Close();
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}