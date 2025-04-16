using System;
using System.Linq;
using System.Net.NetworkInformation;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Data.Entity;

namespace Coffe_Grinder
{
    public partial class InvoiceWindow : Window
    {
        private readonly Coffe_Grinder_DBEntities db;
        private Order currentOrder;
        private string notes;

        public InvoiceWindow(int orderId, string orderNotes = null)
        {
            InitializeComponent();
            db = new Coffe_Grinder_DBEntities();
            notes = orderNotes;
            LoadOrderData(orderId);
        }

        private void LoadOrderData(int orderId)
        {
            try
            {
                // EF6 include syntax
                currentOrder = db.Orders
                    .Include(o => o.OrderDetails.Select(od => od.CoffeeInventory))
                    .FirstOrDefault(o => o.OrderID == orderId);

                if (currentOrder != null)
                {
                    // Set header information
                    InvoiceNumber.Text = $"INVOICE #: {currentOrder.OrderID}";
                    InvoiceDate.Text = $"DATE: {currentOrder.OrderDate:d}";
                    CustomerName.Text = $"CUSTOMER: {currentOrder.CustomerName}";

                    // Set order items
                    InvoiceItems.ItemsSource = currentOrder.OrderDetails.Select(od => new
                    {
                        od.CoffeeInventory,
                        od.Quantity,
                        od.UnitPrice,
                        Subtotal = od.Quantity * od.UnitPrice
                    }).ToList();

                    // Set total amount
                    TotalAmount.Text = $"TOTAL: {currentOrder.TotalPrice:C}";

                    // Set order notes
                    OrderNotes.Text = notes ?? string.Empty;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading invoice data: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Print_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                PrintDialog printDialog = new PrintDialog();
                if (printDialog.ShowDialog() == true)
                {
                    FlowDocument document = CreatePrintableDocument();
                    document.PageHeight = printDialog.PrintableAreaHeight;
                    document.PageWidth = printDialog.PrintableAreaWidth;
                    document.PagePadding = new Thickness(40);
                    document.ColumnGap = 0;
                    document.ColumnWidth = printDialog.PrintableAreaWidth;

                    IDocumentPaginatorSource paginatorSource = document;
                    printDialog.PrintDocument(paginatorSource.DocumentPaginator,
                        $"Coffee Grinder Invoice #{currentOrder.OrderID}");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error printing invoice: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private FlowDocument CreatePrintableDocument()
        {
            FlowDocument document = new FlowDocument();

            // Header
            Paragraph header = new Paragraph(new Run("COFFEE GRINDER"))
            {
                FontSize = 20,
                FontWeight = FontWeights.Bold,
                TextAlignment = TextAlignment.Center
            };
            document.Blocks.Add(header);

            // Invoice title
            Paragraph invoiceTitle = new Paragraph(new Run("INVOICE"))
            {
                FontSize = 16,
                FontWeight = FontWeights.Bold,
                TextAlignment = TextAlignment.Center,
                Margin = new Thickness(0, 10, 0, 20)
            };
            document.Blocks.Add(invoiceTitle);

            // Order information
            Paragraph orderInfo = new Paragraph();
            orderInfo.Inlines.Add(new Run($"Invoice #: {currentOrder.OrderID}\n"));
            orderInfo.Inlines.Add(new Run($"Date: {currentOrder.OrderDate:d}\n"));
            orderInfo.Inlines.Add(new Run($"Customer: {currentOrder.CustomerName}"));
            orderInfo.FontSize = 12;
            orderInfo.Margin = new Thickness(0, 0, 0, 20);
            document.Blocks.Add(orderInfo);

            // Items table
            Table table = new Table();
            table.CellSpacing = 0;
            table.Margin = new Thickness(0, 0, 0, 20);

            // Add columns
            table.Columns.Add(new TableColumn { Width = new GridLength(200) });
            table.Columns.Add(new TableColumn { Width = new GridLength(50) });
            table.Columns.Add(new TableColumn { Width = new GridLength(80) });
            table.Columns.Add(new TableColumn { Width = new GridLength(80) });

            // Add header row
            TableRowGroup rowGroup = new TableRowGroup();
            TableRow headerRow = new TableRow { Background = Brushes.LightGray };
            headerRow.Cells.Add(new TableCell(new Paragraph(new Run("Item")) { FontWeight = FontWeights.Bold }));
            headerRow.Cells.Add(new TableCell(new Paragraph(new Run("Qty")) { FontWeight = FontWeights.Bold, TextAlignment = TextAlignment.Center }));
            headerRow.Cells.Add(new TableCell(new Paragraph(new Run("Price")) { FontWeight = FontWeights.Bold, TextAlignment = TextAlignment.Right }));
            headerRow.Cells.Add(new TableCell(new Paragraph(new Run("Subtotal")) { FontWeight = FontWeights.Bold, TextAlignment = TextAlignment.Right }));
            rowGroup.Rows.Add(headerRow);

            // Add item rows
            foreach (var item in currentOrder.OrderDetails)
            {
                TableRow row = new TableRow();
                row.Cells.Add(new TableCell(new Paragraph(new Run(item.CoffeeInventory.CoffeeName))));
                row.Cells.Add(new TableCell(new Paragraph(new Run(item.Quantity.ToString())) { TextAlignment = TextAlignment.Center }));
                row.Cells.Add(new TableCell(new Paragraph(new Run(item.UnitPrice.ToString())) { TextAlignment = TextAlignment.Right }));
                row.Cells.Add(new TableCell(new Paragraph(new Run((item.Quantity * item.UnitPrice).ToString())) { TextAlignment = TextAlignment.Right }));
                rowGroup.Rows.Add(row);
            }

            table.RowGroups.Add(rowGroup);
            document.Blocks.Add(table);

            // Add total
            Paragraph total = new Paragraph(new Run($"TOTAL: {currentOrder.TotalPrice:C}"))
            {
                FontSize = 14,
                FontWeight = FontWeights.Bold,
                TextAlignment = TextAlignment.Right,
                Margin = new Thickness(0, 0, 0, 20)
            };
            document.Blocks.Add(total);

            // Add notes if available
            if (!string.IsNullOrWhiteSpace(notes))
            {
                Paragraph notesHeader = new Paragraph(new Run("ORDER NOTES:"))
                {
                    FontWeight = FontWeights.Bold,
                    Margin = new Thickness(0, 0, 0, 5)
                };
                document.Blocks.Add(notesHeader);

                Paragraph notesContent = new Paragraph(new Run(notes))
                {
                    Margin = new Thickness(20, 0, 0, 0)
                };
                document.Blocks.Add(notesContent);
            }

            return document;
        }
    }
}