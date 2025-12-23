using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;
using System.Linq;

namespace POS_Rice
{
    public partial class frmSalesReport : Form
    {
        private string connectionString = @"Server=.\SQLEXPRESS;Database=Rice;Integrated Security=true;";

        // Controls
        private DateTimePicker dateFrom, dateTo;
        private ComboBox comboReportType;
        private Button btnGenerate, btnExport, btnPrint;
        private DataGridView dataGridViewSales;
        private Label lblTotalSales, lblTotalCash, lblTotalCredit, lblTotalDebit;
        private Panel panelSummary;

        public frmSalesReport()
        {
            InitializeComponent();
            SetupForm();
        }

        private void SetupForm()
        {
            // Form settings
            this.Text = "Sales Report";
            this.Size = new Size(1000, 700);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.White;

            // Create controls
            CreateControls();
            LoadReportTypes();
        }

        private void CreateControls()
        {
            // Labels
            var lblFrom = new Label { Text = "From Date:", Location = new Point(20, 20), Size = new Size(70, 20), Font = new Font("Arial", 9, FontStyle.Bold) };
            var lblTo = new Label { Text = "To Date:", Location = new Point(220, 20), Size = new Size(70, 20), Font = new Font("Arial", 9, FontStyle.Bold) };
            var lblReportType = new Label { Text = "Report Type:", Location = new Point(420, 20), Size = new Size(80, 20), Font = new Font("Arial", 9, FontStyle.Bold) };

            // Date Pickers
            dateFrom = new DateTimePicker { Location = new Point(90, 20), Size = new Size(120, 20), Format = DateTimePickerFormat.Short };
            dateTo = new DateTimePicker { Location = new Point(290, 20), Size = new Size(120, 20), Format = DateTimePickerFormat.Short };

            // Set default dates (last 30 days)
            dateFrom.Value = DateTime.Now.AddDays(-30);
            dateTo.Value = DateTime.Now;

            // ComboBox
            comboReportType = new ComboBox { Location = new Point(500, 20), Size = new Size(150, 20), DropDownStyle = ComboBoxStyle.DropDownList };

            // Buttons
            btnGenerate = new Button { Text = "Generate Report", Location = new Point(670, 18), Size = new Size(100, 25), BackColor = Color.SteelBlue, ForeColor = Color.White };
            btnExport = new Button { Text = "Export Excel", Location = new Point(780, 18), Size = new Size(80, 25), BackColor = Color.Green, ForeColor = Color.White };
            btnPrint = new Button { Text = "Print", Location = new Point(870, 18), Size = new Size(80, 25), BackColor = Color.Orange, ForeColor = Color.White };

            // DataGridView
            dataGridViewSales = new DataGridView { Location = new Point(20, 60), Size = new Size(930, 400) };

            // Summary Panel
            panelSummary = new Panel { Location = new Point(20, 470), Size = new Size(930, 80), BorderStyle = BorderStyle.FixedSingle, BackColor = Color.LightYellow };

            // Summary Labels
            lblTotalSales = new Label { Text = "Total Sales: 0.00", Location = new Point(20, 15), Size = new Size(200, 20), Font = new Font("Arial", 10, FontStyle.Bold), ForeColor = Color.DarkBlue };
            lblTotalCash = new Label { Text = "Total Cash: 0.00", Location = new Point(250, 15), Size = new Size(200, 20), Font = new Font("Arial", 10, FontStyle.Bold), ForeColor = Color.DarkGreen };
            lblTotalCredit = new Label { Text = "Total Credit: 0.00", Location = new Point(500, 15), Size = new Size(200, 20), Font = new Font("Arial", 10, FontStyle.Bold), ForeColor = Color.DarkOrange };
            lblTotalDebit = new Label { Text = "Total Debit: 0.00", Location = new Point(750, 15), Size = new Size(200, 20), Font = new Font("Arial", 10, FontStyle.Bold), ForeColor = Color.DarkRed };

            var lblAvgSale = new Label { Text = "Average Sale: 0.00", Location = new Point(20, 45), Size = new Size(150, 20), Font = new Font("Arial", 9, FontStyle.Bold) };
            var lblTotalOrders = new Label { Text = "Total Orders: 0", Location = new Point(250, 45), Size = new Size(150, 20), Font = new Font("Arial", 9, FontStyle.Bold) };

            // Add controls to panel
            panelSummary.Controls.AddRange(new Control[] { lblTotalSales, lblTotalCash, lblTotalCredit, lblTotalDebit, lblAvgSale, lblTotalOrders });

            // Add controls to form
            this.Controls.AddRange(new Control[] {
                lblFrom, lblTo, lblReportType,
                dateFrom, dateTo, comboReportType,
                btnGenerate, btnExport, btnPrint,
                dataGridViewSales, panelSummary
            });

            // Event handlers
            btnGenerate.Click += btnGenerate_Click;
            btnExport.Click += btnExport_Click;
            btnPrint.Click += btnPrint_Click;
            dataGridViewSales.RowPostPaint += DataGridViewSales_RowPostPaint;
        }

        private void LoadReportTypes()
        {
            comboReportType.Items.AddRange(new string[] {
                "Daily Sales Summary",
                "Detailed Sales Report",
                "Customer Wise Sales",
                "Product Wise Sales",
                "Payment Method Summary"
            });
            comboReportType.SelectedIndex = 0;
        }

        private void btnGenerate_Click(object sender, EventArgs e)
        {
            GenerateSalesReport();
        }

        private void GenerateSalesReport()
        {
            try
            {
                string reportType = comboReportType.SelectedItem.ToString();
                string query = "";

                switch (reportType)
                {
                    case "Daily Sales Summary":
                        query = GetDailySalesSummaryQuery();
                        break;
                    case "Detailed Sales Report":
                        query = GetDetailedSalesQuery();
                        break;
                    case "Customer Wise Sales":
                        query = GetCustomerWiseSalesQuery();
                        break;
                    case "Product Wise Sales":
                        query = GetProductWiseSalesQuery();
                        break;
                    case "Payment Method Summary":
                        query = GetPaymentMethodSummaryQuery();
                        break;
                }

                using (SqlConnection conn = new SqlConnection(connectionString))
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@FromDate", dateFrom.Value.Date);
                    cmd.Parameters.AddWithValue("@ToDate", dateTo.Value.Date);

                    conn.Open();
                    DataTable dt = new DataTable();
                    using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                    {
                        da.Fill(dt);
                        dataGridViewSales.DataSource = dt;
                    }
                }

                FormatDataGridView();
                CalculateSummary();

                // Refresh the grid to ensure row numbers are visible
                dataGridViewSales.Refresh();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error generating report: {ex.Message}", "Error",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private string GetDailySalesSummaryQuery()
        {
            return @"
            SELECT 
                CONVERT(DATE, OrderDate) AS SaleDate,
                COUNT(OrderID) AS TotalOrders,
                SUM(ISNULL(Cash, 0) + ISNULL(Credit, 0) + ISNULL(Debit, 0)) AS TotalSales,
                SUM(ISNULL(Cash, 0)) AS CashSales,
                SUM(ISNULL(Credit, 0)) AS CreditSales,
                SUM(ISNULL(Debit, 0)) AS DebitSales,
                AVG(ISNULL(Cash, 0) + ISNULL(Credit, 0) + ISNULL(Debit, 0)) AS AverageSale
            FROM Orders
            WHERE OrderDate BETWEEN @FromDate AND @ToDate
            GROUP BY CONVERT(DATE, OrderDate)
            ORDER BY SaleDate DESC";
        }

        private string GetDetailedSalesQuery()
        {
            return @"
            SELECT 
                o.OrderID,
                o.OrderDate,
                o.Customer_Name,
                o.Cash,
                o.Credit,
                o.Debit,
                (ISNULL(o.Cash, 0) + ISNULL(o.Credit, 0) + ISNULL(o.Debit, 0)) AS TotalAmount,
                o.Balance,
                COUNT(sp.SP_ID) AS TotalItems,
                SUM(sp.Amount) AS ItemsTotal
            FROM Orders o
            LEFT JOIN SaleProduct sp ON o.OrderID = sp.OrderID
            WHERE o.OrderDate BETWEEN @FromDate AND @ToDate
            GROUP BY o.OrderID, o.OrderDate, o.Customer_Name, o.Cash, o.Credit, o.Debit, o.Balance
            ORDER BY o.OrderDate DESC";
        }

        private string GetCustomerWiseSalesQuery()
        {
            return @"
            SELECT 
                o.Customer_Name,
                COUNT(o.OrderID) AS TotalOrders,
                SUM(ISNULL(o.Cash, 0) + ISNULL(o.Credit, 0) + ISNULL(o.Debit, 0)) AS TotalSales,
                SUM(ISNULL(o.Cash, 0)) AS CashAmount,
                SUM(ISNULL(o.Credit, 0)) AS CreditAmount,
                SUM(ISNULL(o.Debit, 0)) AS DebitAmount,
                AVG(ISNULL(o.Cash, 0) + ISNULL(o.Credit, 0) + ISNULL(o.Debit, 0)) AS AverageOrderValue
            FROM Orders o
            WHERE o.OrderDate BETWEEN @FromDate AND @ToDate
            GROUP BY o.Customer_Name
            ORDER BY TotalSales DESC";
        }

        private string GetProductWiseSalesQuery()
        {
            return @"
            SELECT 
                p.ProductName,
                p.Brand,
                COUNT(sp.SP_ID) AS TimesSold,
                SUM(sp.QtyBag) AS TotalBags,
                SUM(sp.Weight) AS TotalWeight,
                SUM(sp.Amount) AS TotalAmount,
                AVG(sp.Rate) AS AverageRate
            FROM SaleProduct sp
            INNER JOIN Product p ON sp.ProductID = p.ProductID
            INNER JOIN Orders o ON sp.OrderID = o.OrderID
            WHERE o.OrderDate BETWEEN @FromDate AND @ToDate
            GROUP BY p.ProductName, p.Brand
            ORDER BY TotalAmount DESC";
        }

        private string GetPaymentMethodSummaryQuery()
        {
            return @"
            SELECT 
                'Cash' AS PaymentMethod,
                SUM(ISNULL(Cash, 0)) AS Amount,
                COUNT(CASE WHEN ISNULL(Cash, 0) > 0 THEN 1 END) AS TransactionCount
            FROM Orders WHERE OrderDate BETWEEN @FromDate AND @ToDate AND ISNULL(Cash, 0) > 0
            
            UNION ALL
            
            SELECT 
                'Credit' AS PaymentMethod,
                SUM(ISNULL(Credit, 0)) AS Amount,
                COUNT(CASE WHEN ISNULL(Credit, 0) > 0 THEN 1 END) AS TransactionCount
            FROM Orders WHERE OrderDate BETWEEN @FromDate AND @ToDate AND ISNULL(Credit, 0) > 0
            
            UNION ALL
            
            SELECT 
                'Debit' AS PaymentMethod,
                SUM(ISNULL(Debit, 0)) AS Amount,
                COUNT(CASE WHEN ISNULL(Debit, 0) > 0 THEN 1 END) AS TransactionCount
            FROM Orders WHERE OrderDate BETWEEN @FromDate AND @ToDate AND ISNULL(Debit, 0) > 0
            
            ORDER BY Amount DESC";
        }

        private void FormatDataGridView()
        {
            if (dataGridViewSales.Columns.Count == 0) return;

            // Add row number column if it doesn't exist
            if (!dataGridViewSales.Columns.Contains("RowNumber"))
            {
                DataGridViewTextBoxColumn rowNumberColumn = new DataGridViewTextBoxColumn();
                rowNumberColumn.Name = "RowNumber";
                rowNumberColumn.HeaderText = "Sr. No.";
                rowNumberColumn.Width = 60;
                rowNumberColumn.ReadOnly = true;
                rowNumberColumn.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                rowNumberColumn.DefaultCellStyle.BackColor = Color.LightGray;
                rowNumberColumn.DefaultCellStyle.Font = new Font("Arial", 8, FontStyle.Bold);

                dataGridViewSales.Columns.Insert(0, rowNumberColumn);
            }

            dataGridViewSales.CellBorderStyle = DataGridViewCellBorderStyle.None;
            dataGridViewSales.AllowUserToAddRows = false;
            dataGridViewSales.RowHeadersVisible = false; // Hide default row headers
            dataGridViewSales.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dataGridViewSales.ReadOnly = true;
            dataGridViewSales.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

            // Format headers
            dataGridViewSales.EnableHeadersVisualStyles = false;
            dataGridViewSales.ColumnHeadersDefaultCellStyle.BackColor = Color.SteelBlue;
            dataGridViewSales.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dataGridViewSales.ColumnHeadersDefaultCellStyle.Font = new Font("Arial", 9, FontStyle.Bold);
            dataGridViewSales.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;

            // Format other columns
            foreach (DataGridViewColumn column in dataGridViewSales.Columns)
            {
                if (column.Name == "RowNumber") continue; // Skip row number column

                column.HeaderText = FormatHeaderText(column.Name);

                if (column.ValueType == typeof(decimal) ||
                    column.Name.Contains("Amount") || column.Name.Contains("Cash") ||
                    column.Name.Contains("Credit") || column.Name.Contains("Debit") ||
                    column.Name.Contains("Sales") || column.Name.Contains("Rate") ||
                    column.Name.Contains("Average") || (column.Name.Contains("Total") &&
                    !column.Name.Contains("Orders") && !column.Name.Contains("Items")))
                {
                    column.DefaultCellStyle.Format = "N2";
                    column.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
                    column.DefaultCellStyle.ForeColor = Color.DarkGreen;
                }
                else if (column.Name.Contains("Date"))
                {
                    column.DefaultCellStyle.Format = "dd/MM/yyyy";
                    column.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                }
                else if (column.Name.Contains("ID") || column.Name.Contains("Orders") || column.Name.Contains("Count"))
                {
                    column.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                }
            }

            // Populate row numbers
            for (int i = 0; i < dataGridViewSales.Rows.Count; i++)
            {
                if (!dataGridViewSales.Rows[i].IsNewRow)
                {
                    dataGridViewSales.Rows[i].Cells["RowNumber"].Value = (i + 1).ToString();
                }
            }

            dataGridViewSales.AlternatingRowsDefaultCellStyle.BackColor = Color.LightCyan;

        }



        private string FormatHeaderText(string headerText)
        {
            return headerText
                .Replace("_", "  ")
                .Replace("ID", " ID")
                .Replace("Name", " Name")
                .Replace("Date", " Date")
                .Replace("Amount", " Amount")
                .Replace("Total", " Total")
                .Replace("Average", " Average");
        }

        private void CalculateSummary()
        {
            try
            {
                decimal totalSales = 0;
                decimal totalCash = 0;
                decimal totalCredit = 0;
                decimal totalDebit = 0;
                int totalOrders = 0;

                foreach (DataGridViewRow row in dataGridViewSales.Rows)
                {
                    if (!row.IsNewRow)
                    {
                        // Calculate based on report type
                        string reportType = comboReportType.SelectedItem.ToString();

                        switch (reportType)
                        {
                            case "Daily Sales Summary":
                                totalSales += GetCellDecimalValue(row, "TotalSales");
                                totalCash += GetCellDecimalValue(row, "CashSales");
                                totalCredit += GetCellDecimalValue(row, "CreditSales");
                                totalDebit += GetCellDecimalValue(row, "DebitSales");
                                totalOrders += GetCellIntValue(row, "TotalOrders");
                                break;
                            case "Detailed Sales Report":
                                totalSales += GetCellDecimalValue(row, "TotalAmount");
                                totalCash += GetCellDecimalValue(row, "Cash");
                                totalCredit += GetCellDecimalValue(row, "Credit");
                                totalDebit += GetCellDecimalValue(row, "Debit");
                                totalOrders++;
                                break;
                            case "Customer Wise Sales":
                                totalSales += GetCellDecimalValue(row, "TotalSales");
                                totalCash += GetCellDecimalValue(row, "CashAmount");
                                totalCredit += GetCellDecimalValue(row, "CreditAmount");
                                totalDebit += GetCellDecimalValue(row, "DebitAmount");
                                totalOrders += GetCellIntValue(row, "TotalOrders");
                                break;
                            case "Product Wise Sales":
                                totalSales += GetCellDecimalValue(row, "TotalAmount");
                                break;
                            case "Payment Method Summary":
                                string method = GetCellStringValue(row, "PaymentMethod");
                                decimal amount = GetCellDecimalValue(row, "Amount");
                                if (method == "Cash") totalCash += amount;
                                else if (method == "Credit") totalCredit += amount;
                                else if (method == "Debit") totalDebit += amount;
                                totalSales += amount;
                                break;
                        }
                    }
                }

                // Update summary labels
                lblTotalSales.Text = $"Total Sales: {totalSales:N2}";
                lblTotalCash.Text = $"Total Cash: {totalCash:N2}";
                lblTotalCredit.Text = $"Total Credit: {totalCredit:N2}";
                lblTotalDebit.Text = $"Total Debit: {totalDebit:N2}";

                // Update other summary labels in panel
                var avgSaleLabel = panelSummary.Controls.OfType<Label>().FirstOrDefault(l => l.Text.StartsWith("Average Sale"));
                var totalOrdersLabel = panelSummary.Controls.OfType<Label>().FirstOrDefault(l => l.Text.StartsWith("Total Orders"));

                if (avgSaleLabel != null && totalOrders > 0)
                    avgSaleLabel.Text = $"Average Sale: {(totalSales / totalOrders):N2}";

                if (totalOrdersLabel != null)
                    totalOrdersLabel.Text = $"Total Orders: {totalOrders}";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error calculating summary: {ex.Message}", "Error",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private decimal GetCellDecimalValue(DataGridViewRow row, string columnName)
        {
            if (row.Cells[columnName].Value != null &&
                decimal.TryParse(row.Cells[columnName].Value.ToString(), out decimal value))
            {
                return value;
            }
            return 0;
        }

        private int GetCellIntValue(DataGridViewRow row, string columnName)
        {
            if (row.Cells[columnName].Value != null &&
                int.TryParse(row.Cells[columnName].Value.ToString(), out int value))
            {
                return value;
            }
            return 0;
        }

        private string GetCellStringValue(DataGridViewRow row, string columnName)
        {
            return row.Cells[columnName].Value?.ToString() ?? "";
        }

        private void DataGridViewSales_RowPostPaint(object sender, DataGridViewRowPostPaintEventArgs e)
        {
            // Add row numbers
            var grid = sender as DataGridView;
            if (grid != null)
            {
                var rowIndex = (e.RowIndex + 1).ToString();
                var centerFormat = new StringFormat()
                {
                    Alignment = StringAlignment.Center,
                    LineAlignment = StringAlignment.Center
                };

                var headerBounds = new Rectangle(e.RowBounds.Left, e.RowBounds.Top,
                    grid.RowHeadersWidth, e.RowBounds.Height);
                e.Graphics.DrawString(rowIndex, this.Font, SystemBrushes.ControlText,
                    headerBounds, centerFormat);
            }
        }

        private void btnExport_Click(object sender, EventArgs e)
        {
            ExportToExcel();
        }

        private void btnPrint_Click(object sender, EventArgs e)
        {
            PrintReport();
        }

        private void ExportToExcel()
        {
            try
            {
                // Simple Excel export using CSV format
                SaveFileDialog saveFileDialog = new SaveFileDialog();
                saveFileDialog.Filter = "CSV files (*.csv)|*.csv";
                saveFileDialog.FileName = $"Sales_Report_{DateTime.Now:yyyyMMdd_HHmmss}.csv";

                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    using (System.IO.StreamWriter sw = new System.IO.StreamWriter(saveFileDialog.FileName))
                    {
                        // Write headers
                        var headers = dataGridViewSales.Columns.Cast<DataGridViewColumn>()
                                            .Select(column => column.HeaderText);
                        sw.WriteLine(string.Join(",", headers));

                        // Write data
                        foreach (DataGridViewRow row in dataGridViewSales.Rows)
                        {
                            if (!row.IsNewRow)
                            {
                                var cells = row.Cells.Cast<DataGridViewCell>()
                                            .Select(cell => $"\"{cell.Value}\"");
                                sw.WriteLine(string.Join(",", cells));
                            }
                        }
                    }

                    MessageBox.Show($"Report exported successfully to {saveFileDialog.FileName}", "Success",
                                  MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error exporting to Excel: {ex.Message}", "Error",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void PrintReport()
        {
            MessageBox.Show("Print functionality would be implemented here with proper formatting",
                          "Print Report", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
}