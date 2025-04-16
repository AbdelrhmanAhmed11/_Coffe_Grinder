using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Coffe_Grinder
{
    public partial class Adminhomepage : Page
    {
        private readonly Coffe_Grinder_DBEntities db = new Coffe_Grinder_DBEntities();

        public Adminhomepage()
        {
            InitializeComponent();
            Loaded += async (sender, e) => await LoadDataAsync();
        }

        private async Task LoadDataAsync()
        {
            try
            {
                ToggleLoading(CoffeeDataGrid, CoffeeLoadingIndicator, true);
                var coffeeInventory = await db.CoffeeInventories
                    .Include(c => c.CoffeeType)
                    .OrderBy(c => c.CoffeeName)
                    .ToListAsync();
                CoffeeDataGrid.ItemsSource = coffeeInventory;
                ToggleLoading(CoffeeDataGrid, CoffeeLoadingIndicator, false);

                // Load Recent Orders
                ToggleLoading(OrdersDataGrid, OrdersLoadingIndicator, true);
                var recentOrders = await db.Orders
                    .Include(o => o.OrderStatus)
                    .Include(o => o.User)
                    .OrderByDescending(o => o.OrderDate)
                    .Take(10)
                    .ToListAsync();

                OrdersDataGrid.ItemsSource = recentOrders;

                if (!recentOrders.Any())
                {
                    MessageBox.Show("No recent orders found.", "Info",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
                ToggleLoading(OrdersDataGrid, OrdersLoadingIndicator, false);

                // Load Low Stock Alerts (under 10kg)
                var lowStockItems = coffeeInventory
                    .Where(c => c.QuantityInStock < 10)
                    .OrderBy(c => c.QuantityInStock)
                    .ToList();

                if (!lowStockItems.Any())
                {
                    LowStockList.ItemsSource = new[] {
                new {
                    CoffeeName = "No low stock items",
                    QuantityInStock = 0,
                    CoffeeType = new { TypeName = "All items are sufficiently stocked" }
                }
            };
                }
                else
                {
                    LowStockList.ItemsSource = lowStockItems;
                }
            }
            catch (System.Exception ex)
            {
                MessageBox.Show($"Error loading data: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void ToggleLoading(DataGrid dataGrid, FrameworkElement loader, bool isLoading)
        {
            dataGrid.Visibility = isLoading ? Visibility.Collapsed : Visibility.Visible;
            loader.Visibility = isLoading ? Visibility.Visible : Visibility.Collapsed;
        }


        private void ManageInventory_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new inventory());
        }

        private void ViewAllOrders_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new OrdersPage());
        }
    }
}