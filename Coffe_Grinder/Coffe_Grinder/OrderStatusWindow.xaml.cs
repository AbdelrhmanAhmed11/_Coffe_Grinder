using System;
using System.Data.Entity;
using System.Linq;
using System.Windows;

namespace Coffe_Grinder
{
    public partial class OrderStatusWindow : Window
    {
        private readonly int _orderId;
        private readonly Coffe_Grinder_DBEntities _db;

        public OrderStatusWindow(int orderId, Coffe_Grinder_DBEntities db)
        {
            InitializeComponent();
            _orderId = orderId;
            _db = db;
            LoadStatuses();
        }

        private void LoadStatuses()
        {
            StatusComboBox.ItemsSource = _db.OrderStatuses.ToList();
            StatusComboBox.SelectedValuePath = "StatusID";
            StatusComboBox.DisplayMemberPath = "StatusName";
        }

        private void Update_Click(object sender, RoutedEventArgs e)
        {
            if (StatusComboBox.SelectedItem == null)
            {
                MessageBox.Show("Please select a status.", "Warning",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                var order = _db.Orders.Find(_orderId);
                if (order != null)
                {
                    order.StatusID = ((OrderStatus)StatusComboBox.SelectedItem).StatusID;
                    _db.SaveChanges();
                    MessageBox.Show("Order status updated successfully.", "Success",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    this.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error updating status: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}