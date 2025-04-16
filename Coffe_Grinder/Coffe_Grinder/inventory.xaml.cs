using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Data.Entity;

namespace Coffe_Grinder
{
    public partial class inventory : Page
    {
        private readonly Coffe_Grinder_DBEntities db = new Coffe_Grinder_DBEntities();

        public inventory()
        {
            InitializeComponent();
            LoadCoffeeInventory();
            LoadCoffeeTypes();
            AttachEventHandlers();
        }

        private void AddNewCoffeeType(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(NewCoffeeTypeName.Text))
            {
                ShowErrorMessage("Please enter a coffee type name.");
                return;
            }

            try
            {
                var newCoffeeType = new CoffeeType
                {
                    TypeName = NewCoffeeTypeName.Text,
                };

                db.CoffeeTypes.Add(newCoffeeType);
                db.SaveChanges();

                LoadCoffeeTypes();
                NewCoffeeTypeName.Text = string.Empty;
                ShowSuccessMessage("New coffee type added successfully!");
            }
            catch (Exception ex)
            {
                ShowErrorMessage($"Error adding new coffee type: {ex.Message}");
            }
        }

        private void FindById(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(SearchId.Text))
            {
                ShowErrorMessage("Please enter an ID to search.");
                return;
            }

            if (!int.TryParse(SearchId.Text, out int coffeeId))
            {
                ShowErrorMessage("Please enter a valid numeric ID.");
                return;
            }

            try
            {
                var inventoryItem = db.CoffeeInventories
                    .Include(c => c.CoffeeType)
                    .FirstOrDefault(c => c.CoffeeID == coffeeId);

                if (inventoryItem == null)
                {
                    ShowErrorMessage($"No coffee found with ID: {coffeeId}");
                    return;
                }

                Id.Text = inventoryItem.CoffeeID.ToString();
                CoffeeName.Text = inventoryItem.CoffeeName;
                CoffeeType.SelectedValue = inventoryItem.CoffeeTypeID;
                Description.Text = inventoryItem.Description;
                Amount.Text = inventoryItem.QuantityInStock.ToString();
                PricePerKg.Text = inventoryItem.PricePerKg.ToString();

                CoffeeDataGrid.SelectedItem = inventoryItem;
                CoffeeDataGrid.ScrollIntoView(inventoryItem);

                ShowSuccessMessage($"Coffee ID {coffeeId} loaded successfully.");
            }
            catch (Exception ex)
            {
                ShowErrorMessage($"Error finding coffee: {ex.Message}");
            }
        }

        private void AttachEventHandlers()
        {
            CoffeeDataGrid.SelectionChanged += (sender, e) =>
            {
                if (CoffeeDataGrid.SelectedItem is CoffeeInventory selectedItem)
                {
                    Id.Text = selectedItem.CoffeeID.ToString();
                    CoffeeName.Text = selectedItem.CoffeeName;
                    CoffeeType.SelectedValue = selectedItem.CoffeeTypeID;
                    Description.Text = selectedItem.Description;
                    Amount.Text = selectedItem.QuantityInStock.ToString();
                    PricePerKg.Text = selectedItem.PricePerKg.ToString();
                }
            };
        }

        private void LoadCoffeeTypes()
        {
            try
            {
                CoffeeType.ItemsSource = db.CoffeeTypes
                    .OrderBy(ct => ct.TypeName)
                    .ToList();
                CoffeeType.SelectedValuePath = "CoffeeTypeID";
                CoffeeType.DisplayMemberPath = "TypeName";
            }
            catch (Exception ex)
            {
                ShowErrorMessage($"Error loading coffee types: {ex.Message}");
            }
        }

        private void refresh(object sender, RoutedEventArgs e)
        {
            LoadCoffeeInventory();
            LoadCoffeeTypes();
            ClearForm();
            SearchId.Text = string.Empty;
            ShowSuccessMessage("Inventory refreshed successfully.");
        }

        private void add(object sender, RoutedEventArgs e)
        {
            if (!ValidateForm()) return;

            try
            {
                var selectedCoffeeType = (CoffeeType)CoffeeType.SelectedItem;

                var newInventory = new CoffeeInventory
                {
                    CoffeeName = CoffeeName.Text,
                    CoffeeTypeID = selectedCoffeeType.CoffeeTypeID,
                    QuantityInStock = int.Parse(Amount.Text),
                    PricePerKg = decimal.Parse(PricePerKg.Text),
                    Description = Description.Text
                };

                db.CoffeeInventories.Add(newInventory);
                db.SaveChanges();

                LoadCoffeeInventory();
                ClearForm();
                ShowSuccessMessage("Coffee added to inventory successfully.");
            }
            catch (Exception ex)
            {
                ShowErrorMessage($"Error adding coffee: {ex.Message}");
            }
        }

        private void update(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(Id.Text))
            {
                ShowErrorMessage("Please select an item to update.");
                return;
            }

            if (!ValidateForm()) return;

            try
            {
                int coffeeId = int.Parse(Id.Text);
                var inventory = db.CoffeeInventories.Find(coffeeId);

                if (inventory == null)
                {
                    ShowErrorMessage("Selected coffee not found in database.");
                    return;
                }

                inventory.CoffeeName = CoffeeName.Text;
                inventory.CoffeeTypeID = (int)CoffeeType.SelectedValue;
                inventory.QuantityInStock = int.Parse(Amount.Text);
                inventory.PricePerKg = decimal.Parse(PricePerKg.Text);
                inventory.Description = Description.Text;

                db.SaveChanges();
                LoadCoffeeInventory();
                ClearForm();
                ShowSuccessMessage("Coffee updated successfully.");
            }
            catch (Exception ex)
            {
                ShowErrorMessage($"Error updating coffee: {ex.Message}");
            }
        }

        private void delete(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(Id.Text))
            {
                ShowErrorMessage("Please select an item to delete.");
                return;
            }

            var result = MessageBox.Show("Are you sure you want to delete this coffee? This will also delete all related order details.",
                "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Warning);

            if (result != MessageBoxResult.Yes) return;

            try
            {
                int coffeeId = int.Parse(Id.Text);

                // First delete any related order details
                var relatedOrderDetails = db.OrderDetails.Where(od => od.CoffeeID == coffeeId).ToList();
                if (relatedOrderDetails.Any())
                {
                    db.OrderDetails.RemoveRange(relatedOrderDetails);
                }

                // Then delete the coffee inventory item
                var inventory = db.CoffeeInventories.FirstOrDefault(x => x.CoffeeID == coffeeId);
                if (inventory == null)
                {
                    ShowErrorMessage("Selected coffee not found in database.");
                    return;
                }

                db.CoffeeInventories.Remove(inventory);
                db.SaveChanges();

                // Reseed the identity column to fill gaps
                var maxId = db.CoffeeInventories.Any() ? db.CoffeeInventories.Max(c => c.CoffeeID) : 0;
                db.Database.ExecuteSqlCommand($"DBCC CHECKIDENT ('CoffeeInventory', RESEED, {maxId})");

                LoadCoffeeInventory();
                ClearForm();
                ShowSuccessMessage("Coffee deleted successfully. ID sequence has been reorganized.");
            }
            catch (Exception ex)
            {
                ShowErrorMessage($"Error deleting coffee: {ex.Message}\n\nMake sure there are no orders referencing this coffee item.");
            }
        }

        private void LoadCoffeeInventory()
        {
            try
            {
                CoffeeDataGrid.ItemsSource = db.CoffeeInventories
                    .Include(c => c.CoffeeType)
                    .OrderBy(c => c.CoffeeID)
                    .ToList();
            }
            catch (Exception ex)
            {
                ShowErrorMessage($"Error loading coffee inventory: {ex.Message}");
            }
        }

        private bool ValidateForm()
        {
            if (string.IsNullOrEmpty(CoffeeName.Text))
            {
                ShowErrorMessage("Please enter a coffee name.");
                return false;
            }

            if (CoffeeType.SelectedItem == null)
            {
                ShowErrorMessage("Please select a coffee type.");
                return false;
            }

            if (!int.TryParse(Amount.Text, out int quantity) || quantity <= 0)
            {
                ShowErrorMessage("Please enter a valid quantity (positive number).");
                return false;
            }

            if (!decimal.TryParse(PricePerKg.Text, out decimal price) || price <= 0)
            {
                ShowErrorMessage("Please enter a valid price (positive number).");
                return false;
            }

            return true;
        }

        private void ClearForm()
        {
            Id.Text = string.Empty;
            CoffeeName.Text = string.Empty;
            CoffeeType.SelectedIndex = -1;
            Description.Text = string.Empty;
            Amount.Text = string.Empty;
            PricePerKg.Text = string.Empty;
        }

        private void ShowErrorMessage(string message)
        {
            MessageBox.Show(message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private void ShowSuccessMessage(string message)
        {
            MessageBox.Show(message, "Success", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}