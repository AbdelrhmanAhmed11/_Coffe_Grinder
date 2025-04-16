using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Data.Entity;

namespace Coffe_Grinder
{
    public partial class OrdersPage : Page
    {
        private readonly Coffe_Grinder_DBEntities db = new Coffe_Grinder_DBEntities();

        public OrdersPage()
        {
            InitializeComponent();
            LoadOrders();
            AttachEventHandlers();
        }

        private void AttachEventHandlers()
        {
            OrdersDataGrid.SelectionChanged += OrdersDataGrid_SelectionChanged;
        }

        private void OrdersDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (OrdersDataGrid.SelectedItem is Order selectedOrder)
            {
                LoadOrderDetails(selectedOrder.OrderID);
                DisplayOrderNotes(selectedOrder);
            }
            else
            {
                OrderDetailsDataGrid.ItemsSource = null;
                OrderNotesTextBox.Text = string.Empty;
            }
        }

        private void LoadOrders()
        {
            try
            {
                db.Orders.Include(o => o.OrderStatus).Load();
                OrdersDataGrid.ItemsSource = db.Orders.Local
                    .OrderByDescending(o => o.OrderDate)
                    .ToList();
            }
            catch (Exception ex)
            {
                ShowErrorMessage("Error loading orders", ex);
            }
        }

        private void LoadOrderDetails(int orderId)
        {
            try
            {
                db.OrderDetails
                    .Where(od => od.OrderID == orderId)
                    .Include(od => od.CoffeeInventory)
                    .Load();

                var orderDetails = db.OrderDetails.Local
                    .Where(od => od.OrderID == orderId)
                    .Select(od => new
                    {
                        od.OrderDetailID,
                        od.CoffeeInventory,
                        od.Quantity,
                        od.UnitPrice,
                        Subtotal = od.Quantity * od.UnitPrice
                    })
                    .ToList();

                OrderDetailsDataGrid.ItemsSource = orderDetails;
            }
            catch (Exception ex)
            {
                ShowErrorMessage("Error loading order details", ex);
            }
        }

        private void DisplayOrderNotes(Order order)
        {
            OrderNotesTextBox.Text = string.IsNullOrWhiteSpace(order.Notes)
                ? "No notes available"
                : order.Notes;
        }

        private void RefreshOrders(object sender, RoutedEventArgs e)
        {
            try
            {
                db.SaveChanges();
                LoadOrders();
                OrderDetailsDataGrid.ItemsSource = null;
                OrderNotesTextBox.Text = string.Empty;
                MessageBox.Show("Orders refreshed successfully.", "Success",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                ShowErrorMessage("Error refreshing orders", ex);
            }
        }

        private void UpdateOrderStatus(object sender, RoutedEventArgs e)
        {
            if (OrdersDataGrid.SelectedItem == null)
            {
                MessageBox.Show("Please select an order to update.", "Warning",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var selectedOrder = (Order)OrdersDataGrid.SelectedItem;
            var statusWindow = new OrderStatusWindow(selectedOrder.OrderID, db);

            if (statusWindow.ShowDialog() == true)
            {
                LoadOrders();
                LoadOrderDetails(selectedOrder.OrderID);
            }
        }

        private void PrintInvoice(object sender, RoutedEventArgs e)
        {
            if (OrdersDataGrid.SelectedItem == null)
            {
                MessageBox.Show("Please select an order to print.", "Warning",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                var selectedOrder = (Order)OrdersDataGrid.SelectedItem;
                var invoiceWindow = new InvoiceWindow(selectedOrder.OrderID);
                invoiceWindow.Show();
            }
            catch (Exception ex)
            {
                ShowErrorMessage("Error generating invoice", ex);
            }
        }

        private void ShowErrorMessage(string message, Exception ex)
        {
            MessageBox.Show($"{message}: {ex.Message}", "Error",
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}