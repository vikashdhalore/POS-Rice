using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace POS_Rice
{
    public partial class frmExpenseReport : Form
    {
        private string connectionString = @"Server=.\SQLEXPRESS;Database=Rice;Integrated Security=true;";

        // Controls
        private DateTimePicker dateFrom, dateTo;
        private ComboBox comboExpenseType, comboAccount;
        private Button btnGenerate, btnExport, btnPrint, btnClearFilters;
        private DataGridView dataGridViewExpenses;
        private Label lblTotalExpenses, lblAvgExpense, lblTotalRecords;
        private Panel panelSummary;
        private TextBox textSearch;

        public frmExpenseReport()
        {
            InitializeComponent();
            SetupForm();
        }

        private void SetupForm()
        {
            // Form settings
            this.Text = "Expense Report";
            this.Size = new Size(1100, 700);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.White;

            // Create controls
            CreateControls();
            LoadFilterOptions();
        }

        private void CreateControls()
        {
            // Labels
            var lblFrom = new Label { Text = "From Date:", Location = new Point(20, 20), Size = new Size(70, 20), Font = new Font("Arial", 9, FontStyle.Bold) };
            var lblTo = new Label { Text = "To Date:", Location = new Point(220, 20), Size = new Size(70, 20), Font = new Font("Arial", 9, FontStyle.Bold) };
            var lblExpenseType = new Label { Text = "Expense Type:", Location = new Point(420, 20), Size = new Size(85, 20), Font = new Font("Arial", 9, FontStyle.Bold) };
            var lblAccount = new Label { Text = "Account:", Location = new Point(620, 20), Size = new Size(60, 20), Font = new Font("Arial", 9, FontStyle.Bold) };
            var lblSearch = new Label { Text = "Search:", Location = new Point(820, 20), Size = new Size(50, 20), Font = new Font("Arial", 9, FontStyle.Bold) };

            // Date Pickers
            dateFrom = new DateTimePicker { Location = new Point(90, 20), Size = new Size(120, 20), Format = DateTimePickerFormat.Short };
            dateTo = new DateTimePicker { Location = new Point(290, 20), Size = new Size(120, 20), Format = DateTimePickerFormat.Short };

            // Set default dates (last 30 days)
            dateFrom.Value = DateTime.Now.AddDays(-30);
            dateTo.Value = DateTime.Now;

            // ComboBoxes
            comboExpenseType = new ComboBox { Location = new Point(505, 20), Size = new Size(110, 20), DropDownStyle = ComboBoxStyle.DropDownList };
            comboAccount = new ComboBox { Location = new Point(680, 20), Size = new Size(130, 20), DropDownStyle = ComboBoxStyle.DropDownList };

            // Search TextBox
            textSearch = new TextBox { Location = new Point(870, 20), Size = new Size(120, 20) };
           // textSearch.PlaceholderText = "Search expenses...";

            // Buttons
            btnGenerate = new Button { Text = "Generate", Location = new Point(20, 50), Size = new Size(80, 25), BackColor = Color.SteelBlue, ForeColor = Color.White };
            btnExport = new Button { Text = "Export Excel", Location = new Point(110, 50), Size = new Size(80, 25), BackColor = Color.Green, ForeColor = Color.White };
            btnPrint = new Button { Text = "Print", Location = new Point(200, 50), Size = new Size(80, 25), BackColor = Color.Orange, ForeColor = Color.White };
            btnClearFilters = new Button { Text = "Clear Filters", Location = new Point(290, 50), Size = new Size(80, 25), BackColor = Color.Gray, ForeColor = Color.White };

            // DataGridView
            dataGridViewExpenses = new DataGridView { Location = new Point(20, 85), Size = new Size(1050, 400) };

            // Summary Panel
            panelSummary = new Panel { Location = new Point(20, 495), Size = new Size(1050, 120), BorderStyle = BorderStyle.FixedSingle, BackColor = Color.LightYellow };

            // Summary Labels
            lblTotalExpenses = new Label { Text = "Total Expenses: RS 0.00", Location = new Point(20, 15), Size = new Size(250, 20), Font = new Font("Arial", 10, FontStyle.Bold), ForeColor = Color.DarkRed };
            lblAvgExpense = new Label { Text = "Average Expense: RS 0.00", Location = new Point(20, 50), Size = new Size(250, 20), Font = new Font("Arial", 10, FontStyle.Bold), ForeColor = Color.DarkBlue };
            lblTotalRecords = new Label { Text = "Total Records: 0", Location = new Point(380, 15), Size = new Size(250, 20), Font = new Font("Arial", 10, FontStyle.Bold), ForeColor = Color.DarkGreen };

            var lblHighestExpense = new Label { Text = "Highest Expense: RS 0.00", Location = new Point(380, 50), Size = new Size(250, 20), Font = new Font("Arial", 9, FontStyle.Bold) };
            var lblLowestExpense = new Label { Text = "Lowest Expense: RS 0.00", Location = new Point(700, 15), Size = new Size(250, 20), Font = new Font("Arial", 9, FontStyle.Bold) };
            var lblDailyAverage = new Label { Text = "Daily Average: RS 0.00", Location = new Point(700, 50), Size = new Size(250, 20), Font = new Font("Arial", 9, FontStyle.Bold) };

            // Add controls to panel
            panelSummary.Controls.AddRange(new Control[] {
                lblTotalExpenses, lblAvgExpense, lblTotalRecords,
                lblHighestExpense, lblLowestExpense, lblDailyAverage
            });

            // Add controls to form
            this.Controls.AddRange(new Control[] {
                lblFrom, lblTo, lblExpenseType, lblAccount, lblSearch,
                dateFrom, dateTo, comboExpenseType, comboAccount, textSearch,
                btnGenerate, btnExport, btnPrint, btnClearFilters,
                dataGridViewExpenses, panelSummary
            });

            // Event handlers
            btnGenerate.Click += btnGenerate_Click;
            btnExport.Click += btnExport_Click;
            btnPrint.Click += btnPrint_Click;
            btnClearFilters.Click += btnClearFilters_Click;
            textSearch.TextChanged += textSearch_TextChanged;
            dataGridViewExpenses.RowPostPaint += DataGridViewExpenses_RowPostPaint;
        }

        private void LoadFilterOptions()
        {
            // Expense Types
            comboExpenseType.Items.AddRange(new string[] {
                "All Expenses",
                "Local Expenses",
                "Party Payments",
                "Inventory Payments",
                "Cashbook Payments"
            });
            comboExpenseType.SelectedIndex = 0;

            // Load Accounts
            LoadAccounts();
        }

        private void LoadAccounts()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    string query = "SELECT ActID, Act_Name FROM Accounts ORDER BY Act_Name";
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        conn.Open();
                        DataTable dt = new DataTable();
                        using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                        {
                            da.Fill(dt);
                            comboAccount.Items.Clear();
                            comboAccount.Items.Add("All Accounts");

                            foreach (DataRow row in dt.Rows)
                            {
                                comboAccount.Items.Add(new KeyValuePair<int, string>(
                                    Convert.ToInt32(row["ActID"]),
                                    row["Act_Name"].ToString()
                                ));
                            }

                            comboAccount.DisplayMember = "Value";
                            comboAccount.ValueMember = "Key";
                            comboAccount.SelectedIndex = 0;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading accounts: {ex.Message}", "Error",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnGenerate_Click(object sender, EventArgs e)
        {
            GenerateExpenseReport();
        }

        private void GenerateExpenseReport()
        {
            try
            {
                string expenseType = comboExpenseType.SelectedItem.ToString();

                if (expenseType == "Cashbook Payments" || expenseType == "All Expenses")
                {
                    // Handle CashBook separately to avoid conversion errors
                    GenerateExpenseReportWithSafeCashBook();
                }
                else
                {
                    GenerateExpenseReportNormal(expenseType);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error generating report: {ex.Message}", "Error",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void GenerateExpenseReportNormal(string expenseType)
        {
            string query = GetExpenseQuery(expenseType);

            using (SqlConnection conn = new SqlConnection(connectionString))
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@FromDate", dateFrom.Value.Date);
                cmd.Parameters.AddWithValue("@ToDate", dateTo.Value.Date);

                // Add filters...
                if (comboAccount.SelectedIndex > 0 && comboAccount.SelectedItem is KeyValuePair<int, string> selectedAccount)
                {
                    cmd.Parameters.AddWithValue("@AccountID", selectedAccount.Key);
                }

                if (!string.IsNullOrEmpty(textSearch.Text.Trim()))
                {
                    cmd.Parameters.AddWithValue("@SearchText", $"%{textSearch.Text.Trim()}%");
                }

                conn.Open();
                DataTable dt = new DataTable();
                using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                {
                    da.Fill(dt);
                    dataGridViewExpenses.DataSource = dt;
                }
            }

            FormatDataGridView();
            CalculateSummary();
        }

        private void GenerateExpenseReportWithSafeCashBook()
        {
            DataTable dtAll = new DataTable();

            // Get CashBook entries safely
            DataTable dtCashBook = GetSafeCashBookData();

            string expenseType = comboExpenseType.SelectedItem.ToString();

            if (expenseType == "Cashbook Payments")
            {
                dataGridViewExpenses.DataSource = dtCashBook;
            }
            else // All Expenses
            {
                // Get other expenses
                DataTable dtOther = GetOtherExpenses();

                // Merge both DataTables
                dtAll = dtOther.Clone();

                foreach (DataRow row in dtOther.Rows)
                {
                    dtAll.ImportRow(row);
                }

                foreach (DataRow row in dtCashBook.Rows)
                {
                    dtAll.ImportRow(row);
                }

                dataGridViewExpenses.DataSource = dtAll;
            }

            FormatDataGridView();
            CalculateSummary();
        }

        private DataTable GetSafeCashBookData()
        {
            DataTable dt = new DataTable();

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = @"
        SELECT 
            CashID,
            Cash_Entry,
            Date,
            CustID
        FROM CashBook 
        WHERE Date BETWEEN @FromDate AND @ToDate
        ORDER BY Date DESC";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@FromDate", dateFrom.Value.Date);
                    cmd.Parameters.AddWithValue("@ToDate", dateTo.Value.Date);

                    conn.Open();
                    using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                    {
                        da.Fill(dt);
                    }
                }
            }

            // Create result DataTable
            DataTable result = new DataTable();
            result.Columns.Add("ExpenseType", typeof(string));
            result.Columns.Add("ExpenseID", typeof(int));
            result.Columns.Add("Description", typeof(string));
            result.Columns.Add("ExpenseDate", typeof(DateTime));
            result.Columns.Add("Amount", typeof(decimal));
            result.Columns.Add("Account", typeof(string));
            result.Columns.Add("PartyName", typeof(string));
            result.Columns.Add("SourceTable", typeof(string));

            // Process each row and extract numeric values safely in C#
            foreach (DataRow row in dt.Rows)
            {
                string cashEntry = row["Cash_Entry"] != DBNull.Value ? row["Cash_Entry"].ToString() : "";
                decimal amount = ExtractNumericValueFromText(cashEntry);

                if (amount > 0)
                {
                    DataRow newRow = result.NewRow();
                    newRow["ExpenseType"] = "Cashbook Payment";
                    newRow["ExpenseID"] = row["CashID"];
                    newRow["Description"] = string.IsNullOrEmpty(cashEntry) ? "Cash Payment" : cashEntry;
                    newRow["ExpenseDate"] = row["Date"];
                    newRow["Amount"] = amount;
                    newRow["Account"] = "Cashbook";
                    newRow["PartyName"] = "";
                    newRow["SourceTable"] = "CashBook";

                    result.Rows.Add(newRow);
                }
            }

            return result;
        }

        private DataTable GetOtherExpenses()
        {
            string query = @"
    SELECT * FROM (
        -- Local Expenses
        SELECT 
            'Local Expense' AS ExpenseType,
            Local_Exp_ID AS ExpenseID,
            ActName AS Description,
            Local_Exp_Date AS ExpenseDate,
            ISNULL(Local_Exp_Amount, 0) AS Amount,
            ActName AS Account,
            '' AS PartyName,
            'LocalExpense' AS SourceTable
        FROM LocalExpense
        WHERE Local_Exp_Date BETWEEN @FromDate AND @ToDate

        UNION ALL

        -- Party Payments
        SELECT 
            'Party Payment' AS ExpenseType,
            P_PaymentID AS ExpenseID,
            'Payment to ' + ISNULL(PartyName, '') AS Description,
            Pay_Date AS ExpenseDate,
            ISNULL(Entry_Amount, 0) AS Amount,
            'Party Payment' AS Account,
            ISNULL(PartyName, '') AS PartyName,
            'PartyPayment' AS SourceTable
        FROM PartyPayment
        WHERE Pay_Date BETWEEN @FromDate AND @ToDate

        UNION ALL

        -- Inventory Payments
        SELECT 
            'Inventory Payment' AS ExpenseType,
            InventoryID AS ExpenseID,
            'Inventory Payment - Laat: ' + ISNULL(LaatNo, '') AS Description,
            InvDate AS ExpenseDate,
            ISNULL(Debit, 0) AS Amount,
            'Inventory' AS Account,
            '' AS PartyName,
            'ProInventory' AS SourceTable
        FROM ProInventory
        WHERE InvDate BETWEEN @FromDate AND @ToDate 
        AND ISNULL(Debit, 0) > 0
    ) AS OtherExpenses
    ORDER BY ExpenseDate DESC";

            DataTable dt = new DataTable();

            using (SqlConnection conn = new SqlConnection(connectionString))
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@FromDate", dateFrom.Value.Date);
                cmd.Parameters.AddWithValue("@ToDate", dateTo.Value.Date);

                conn.Open();
                using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                {
                    da.Fill(dt);
                }
            }

            return dt;
        }

        private decimal ExtractNumericValueFromText(string text)
        {
            if (string.IsNullOrEmpty(text))
                return 0;

            try
            {
                // Remove all non-numeric characters except decimal point and minus sign
                System.Text.RegularExpressions.Regex regex = new System.Text.RegularExpressions.Regex(@"[^\d.-]");
                string cleanText = regex.Replace(text, "");

                // Remove multiple decimal points
                if (cleanText.Count(c => c == '.') > 1)
                {
                    int firstDecimal = cleanText.IndexOf('.');
                    cleanText = cleanText.Substring(0, firstDecimal + 1) +
                               cleanText.Substring(firstDecimal + 1).Replace(".", "");
                }

                if (decimal.TryParse(cleanText, out decimal result))
                {
                    return result;
                }

                return 0;
            }
            catch
            {
                return 0;
            }
        }

        private string GetExpenseQuery(string expenseType)
        {
            string baseQuery = "";

            switch (expenseType)
            {
                case "Local Expenses":
                    baseQuery = @"
            SELECT 
                'Local Expense' AS ExpenseType,
                Local_Exp_ID AS ExpenseID,
                ActName AS Description,
                Local_Exp_Date AS ExpenseDate,
                Local_Exp_Amount AS Amount,
                ActName AS Account,
                '' AS PartyName,
                'LocalExpense' AS SourceTable
            FROM LocalExpense
            WHERE Local_Exp_Date BETWEEN @FromDate AND @ToDate";
                    break;

                case "Party Payments":
                    baseQuery = @"
            SELECT 
                'Party Payment' AS ExpenseType,
                P_PaymentID AS ExpenseID,
                'Payment to ' + PartyName AS Description,
                Pay_Date AS ExpenseDate,
                Entry_Amount AS Amount,
                'Party Payment' AS Account,
                PartyName AS PartyName,
                'PartyPayment' AS SourceTable
            FROM PartyPayment
            WHERE Pay_Date BETWEEN @FromDate AND @ToDate";
                    break;

                case "Inventory Payments":
                    baseQuery = @"
            SELECT 
                'Inventory Payment' AS ExpenseType,
                InventoryID AS ExpenseID,
                'Inventory Payment - Laat: ' + ISNULL(LaatNo, '') AS Description,
                InvDate AS ExpenseDate,
                ISNULL(Debit, 0) AS Amount,
                'Inventory' AS Account,
                '' AS PartyName,
                'ProInventory' AS SourceTable
            FROM ProInventory
            WHERE InvDate BETWEEN @FromDate AND @ToDate 
            AND ISNULL(Debit, 0) > 0";
                    break;

                case "Cashbook Payments":
                    baseQuery = @"
    SELECT 
        'Cashbook Payment' AS ExpenseType,
        CashID AS ExpenseID,
        ISNULL(Cash_Entry, 'Cash Payment') AS Description,
        Date AS ExpenseDate,
        -- Safe conversion using ISNUMERIC (older SQL Server)
        CASE 
            WHEN ISNUMERIC(Cash_Entry) = 1 
            THEN CAST(Cash_Entry AS DECIMAL(18,2))
            ELSE 0 
        END AS Amount,
        'Cashbook' AS Account,
        '' AS PartyName,
        'CashBook' AS SourceTable
    FROM CashBook
    WHERE Date BETWEEN @FromDate AND @ToDate
    AND ISNUMERIC(Cash_Entry) = 1
    AND CAST(Cash_Entry AS DECIMAL(18,2)) > 0";
                    break;

                default: // All Expenses
                    baseQuery = @"
            SELECT * FROM (
                -- Local Expenses
                SELECT 
                    'Local Expense' AS ExpenseType,
                    Local_Exp_ID AS ExpenseID,
                    ActName AS Description,
                    Local_Exp_Date AS ExpenseDate,
                    ISNULL(Local_Exp_Amount, 0) AS Amount,
                    ActName AS Account,
                    '' AS PartyName,
                    'LocalExpense' AS SourceTable
                FROM LocalExpense
                WHERE Local_Exp_Date BETWEEN @FromDate AND @ToDate

                UNION ALL

                -- Party Payments
                SELECT 
                    'Party Payment' AS ExpenseType,
                    P_PaymentID AS ExpenseID,
                    'Payment to ' + ISNULL(PartyName, '') AS Description,
                    Pay_Date AS ExpenseDate,
                    ISNULL(Entry_Amount, 0) AS Amount,
                    'Party Payment' AS Account,
                    ISNULL(PartyName, '') AS PartyName,
                    'PartyPayment' AS SourceTable
                FROM PartyPayment
                WHERE Pay_Date BETWEEN @FromDate AND @ToDate

                UNION ALL

                -- Inventory Payments
                SELECT 
                    'Inventory Payment' AS ExpenseType,
                    InventoryID AS ExpenseID,
                    'Inventory Payment - Laat: ' + ISNULL(LaatNo, '') AS Description,
                    InvDate AS ExpenseDate,
                    ISNULL(Debit, 0) AS Amount,
                    'Inventory' AS Account,
                    '' AS PartyName,
                    'ProInventory' AS SourceTable
                FROM ProInventory
                WHERE InvDate BETWEEN @FromDate AND @ToDate 
                AND ISNULL(Debit, 0) > 0

                UNION ALL

                -- Cashbook Payments (Safe conversion)
                SELECT 
    'Cashbook Payment' AS ExpenseType,
    CashID AS ExpenseID,
    ISNULL(Cash_Entry, 'Cash Payment') AS Description,
    Date AS ExpenseDate,
    CASE 
        WHEN ISNUMERIC(Cash_Entry) = 1 
        THEN CAST(Cash_Entry AS DECIMAL(18,2))
        ELSE 0 
    END AS Amount,
    'Cashbook' AS Account,
    '' AS PartyName,
    'CashBook' AS SourceTable
FROM CashBook
WHERE Date BETWEEN @FromDate AND @ToDate
AND ISNUMERIC(Cash_Entry) = 1
AND CAST(Cash_Entry AS DECIMAL(18,2)) > 0
            ) AS AllExpenses
            WHERE 1=1";
                    break;
            }

            // Add account filter for Local Expenses
            if (comboAccount.SelectedIndex > 0 && expenseType == "Local Expenses")
            {
                if (comboAccount.SelectedItem is KeyValuePair<int, string> selectedAccount)
                {
                    baseQuery += " AND ActID = @AccountID";
                }
            }

            // Add search filter
            if (!string.IsNullOrEmpty(textSearch.Text.Trim()))
            {
                if (expenseType == "All Expenses")
                {
                    baseQuery += " AND (Description LIKE @SearchText OR PartyName LIKE @SearchText)";
                }
                else
                {
                    baseQuery += " AND Description LIKE @SearchText";
                }
            }

            baseQuery += " ORDER BY ExpenseDate DESC, ExpenseID DESC";

            return baseQuery;
        }

        private void FormatDataGridView()
        {
            if (dataGridViewExpenses.Columns.Count == 0) return;

            // Add row number column
            if (!dataGridViewExpenses.Columns.Contains("RowNumber"))
            {
                DataGridViewTextBoxColumn rowNumberColumn = new DataGridViewTextBoxColumn();
                rowNumberColumn.Name = "RowNumber";
                rowNumberColumn.HeaderText = "Sr. No.";
                rowNumberColumn.Width = 60;
                rowNumberColumn.ReadOnly = true;
                rowNumberColumn.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                rowNumberColumn.DefaultCellStyle.BackColor = Color.LightGray;
                rowNumberColumn.DefaultCellStyle.Font = new Font("Arial", 8, FontStyle.Bold);

                dataGridViewExpenses.Columns.Insert(0, rowNumberColumn);
            }

            dataGridViewExpenses.RowHeadersVisible = false;
            dataGridViewExpenses.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dataGridViewExpenses.ReadOnly = true;
            dataGridViewExpenses.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

            // Format headers
            dataGridViewExpenses.EnableHeadersVisualStyles = false;
            dataGridViewExpenses.ColumnHeadersDefaultCellStyle.BackColor = Color.SteelBlue;
            dataGridViewExpenses.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dataGridViewExpenses.ColumnHeadersDefaultCellStyle.Font = new Font("Arial", 9, FontStyle.Bold);
            dataGridViewExpenses.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;

            // Format columns
            foreach (DataGridViewColumn column in dataGridViewExpenses.Columns)
            {
                if (column.Name == "RowNumber") continue;

                column.HeaderText = FormatHeaderText(column.HeaderText);

                if (column.Name.Contains("Amount"))
                {
                    column.DefaultCellStyle.Format = "N2";
                    column.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
                    column.DefaultCellStyle.ForeColor = Color.DarkRed;
                    column.DefaultCellStyle.BackColor = Color.LightCoral;
                }
                else if (column.Name.Contains("Date"))
                {
                    column.DefaultCellStyle.Format = "dd/MM/yyyy";
                    column.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                }
                else if (column.Name.Contains("ID"))
                {
                    column.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                }
                else
                {
                    column.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
                }
            }

            // Populate row numbers
            for (int i = 0; i < dataGridViewExpenses.Rows.Count; i++)
            {
                if (!dataGridViewExpenses.Rows[i].IsNewRow)
                {
                    dataGridViewExpenses.Rows[i].Cells["RowNumber"].Value = (i + 1).ToString();
                }
            }

            dataGridViewExpenses.AlternatingRowsDefaultCellStyle.BackColor = Color.LightCyan;
        }

        private string FormatHeaderText(string headerText)
        {
            return headerText
                .Replace("_", " ")
                .Replace("ID", " ID")
                .Replace("Date", " Date")
                .Replace("Amount", " Amount")
                .Replace("Name", " Name")
                .Replace("Type", " Type");
        }

        private void CalculateSummary()
        {
            try
            {
                decimal totalExpenses = 0;
                decimal highestExpense = 0;
                decimal lowestExpense = decimal.MaxValue;
                int totalRecords = 0;
                int daysCount = (dateTo.Value.Date - dateFrom.Value.Date).Days + 1;

                foreach (DataGridViewRow row in dataGridViewExpenses.Rows)
                {
                    if (!row.IsNewRow)
                    {
                        decimal amount = GetCellDecimalValue(row, "Amount");
                        totalExpenses += amount;
                        totalRecords++;

                        if (amount > highestExpense) highestExpense = amount;
                        if (amount < lowestExpense && amount > 0) lowestExpense = amount;
                    }
                }

                if (lowestExpense == decimal.MaxValue) lowestExpense = 0;

                // Update main summary labels
                lblTotalExpenses.Text = $"Total Expenses: RS{totalExpenses:N2}";
                lblAvgExpense.Text = $"Average Expense: RS{(totalRecords > 0 ? totalExpenses / totalRecords : 0):N2}";
                lblTotalRecords.Text = $"Total Records: {totalRecords}";

                // Update additional summary labels
                var lblHighest = panelSummary.Controls.OfType<Label>().FirstOrDefault(l => l.Text.StartsWith("Highest Expense"));
                var lblLowest = panelSummary.Controls.OfType<Label>().FirstOrDefault(l => l.Text.StartsWith("Lowest Expense"));
                var lblDaily = panelSummary.Controls.OfType<Label>().FirstOrDefault(l => l.Text.StartsWith("Daily Average"));

                if (lblHighest != null) lblHighest.Text = $"Highest Expense: RS{highestExpense:N2}";
                if (lblLowest != null) lblLowest.Text = $"Lowest Expense: RS{lowestExpense:N2}";
                if (lblDaily != null) lblDaily.Text = $"Daily Average: RS{(daysCount > 0 ? totalExpenses / daysCount : 0):N2}";
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

        private void DataGridViewExpenses_RowPostPaint(object sender, DataGridViewRowPostPaintEventArgs e)
        {
            // Optional: Additional row formatting can be added here
        }

        private void btnExport_Click(object sender, EventArgs e)
        {
            ExportToExcel();
        }

        private void btnPrint_Click(object sender, EventArgs e)
        {
            PrintReport();
        }

        private void btnClearFilters_Click(object sender, EventArgs e)
        {
            ClearFilters();
        }

        private void textSearch_TextChanged(object sender, EventArgs e)
        {
            // Real-time search - you can add a delay timer here for better performance
            if (textSearch.Text.Length > 2 || textSearch.Text.Length == 0)
            {
                GenerateExpenseReport();
            }
        }

        private void ClearFilters()
        {
            dateFrom.Value = DateTime.Now.AddDays(-30);
            dateTo.Value = DateTime.Now;
            comboExpenseType.SelectedIndex = 0;
            comboAccount.SelectedIndex = 0;
            textSearch.Clear();
            GenerateExpenseReport();
        }

        private void ExportToExcel()
        {
            try
            {
                SaveFileDialog saveFileDialog = new SaveFileDialog();
                saveFileDialog.Filter = "CSV files (*.csv)|*.csv";
                saveFileDialog.FileName = $"Expense_Report_{DateTime.Now:yyyyMMdd_HHmmss}.csv";

                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    using (System.IO.StreamWriter sw = new System.IO.StreamWriter(saveFileDialog.FileName))
                    {
                        // Write headers (skip RowNumber column)
                        var headers = dataGridViewExpenses.Columns.Cast<DataGridViewColumn>()
                                            .Where(col => col.Name != "RowNumber")
                                            .Select(column => column.HeaderText);
                        sw.WriteLine(string.Join(",", headers));

                        // Write data (skip RowNumber column)
                        foreach (DataGridViewRow row in dataGridViewExpenses.Rows)
                        {
                            if (!row.IsNewRow)
                            {
                                var cells = row.Cells.Cast<DataGridViewCell>()
                                            .Where(cell => cell.OwningColumn.Name != "RowNumber")
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