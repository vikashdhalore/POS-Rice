using Microsoft.Reporting.Map.WebForms.BingMaps;
using POS_Rice;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Text;
using System.Windows.Forms;

public partial class frmLedger : Form
{
    private string connectionString = @"Server=.\SQLEXPRESS;Database=Rice;Integrated Security=true;";

    public frmLedger()
    {
        InitializeComponent();
    }
    // Load Filter Options
    private void LoadFilterOptions()
    {
        try
        {
            // Load Parties
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = "SELECT PartyID, PartyName FROM Party ORDER BY PartyName";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    conn.Open();
                    DataTable dt = new DataTable();
                    using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                    {
                        da.Fill(dt);
                        comboParty.Items.Clear();
                        comboParty.Items.Add("-- All Parties --");
                        foreach (DataRow row in dt.Rows)
                        {
                            comboParty.Items.Add(new KeyValuePair<int, string>(
                                Convert.ToInt32(row["PartyID"]),
                                row["PartyName"].ToString()
                            ));
                        }
                        comboParty.DisplayMember = "Value";
                        comboParty.ValueMember = "Key";
                        comboParty.SelectedIndex = 0;
                    }
                }
            }

            // Load Voucher Types
            comboVoucherType.Items.AddRange(new string[] {
            "All Types", "Payment", "Sale","Inventory", "Expense", "Receipt"
        });
            comboVoucherType.SelectedIndex = 0;
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error loading filters: {ex.Message}", "Error",
                          MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    // Load Ledger Data
    // Load Ledger Data from All Transaction Tables
    // Load Ledger Data from All Transaction Tables
    private void LoadLedgerData()
    {
        try
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                // Combined query that unions data from all transaction tables
                string query = @"
            -- Party Payments (Cash Outflow)
            SELECT 
            P_PaymentID AS TransactionID,
            'Payment' AS VoucherType,
            'PY-' + CAST(P_PaymentID AS NVARCHAR(10)) AS VoucherNo,
            PartyID,
            PartyName +'('+ Description +')' AS Description,
            Pay_Date AS TransactionDate,
    CASE 
        WHEN Status = 'Credit' THEN Entry_Amount
        ELSE 0 
    END AS CreditAmount,
    CASE 
        WHEN Status = 'Debit' THEN Entry_Amount
        ELSE 0 
    END AS DebitAmount,
    'PartyPayment' AS SourceTable
        FROM PartyPayment
        WHERE Pay_Date BETWEEN @FromDate AND @ToDate
            UNION ALL

            -- Sales from Orders (Revenue - Cash Inflow)
            SELECT 
                o.OrderID AS TransactionID,
                'Sale' AS VoucherType,
                'SL-' + CAST(o.OrderID AS NVARCHAR(10)) AS VoucherNo,
                NULL AS PartyID,
                'Sale - ' + o.Customer_Name AS Description,
                o.OrderDate AS TransactionDate,
                (ISNULL(o.Cash, 0) + ISNULL(o.Debit, 0)) AS CreditAmount,
                ISNULL(o.Credit,0) AS DebitAmount,
                'Orders' AS SourceTable
            FROM Orders o
            WHERE o.OrderDate BETWEEN @FromDate AND @ToDate

            UNION ALL

            -- Local Expenses (Cash Outflow)
            SELECT 
                Local_Exp_ID AS TransactionID,
                'Expense' AS VoucherType,
                'EXP-' + CAST(Local_Exp_ID AS NVARCHAR(10)) AS VoucherNo,
                NULL AS PartyID,
                'Expense - ' + ActName AS Description,
                Local_Exp_Date AS TransactionDate,
                0 AS CreditAmount,
                ISNULL(Local_Exp_Amount,0) AS DebitAmount,
                'LocalExpense' AS SourceTable
            FROM LocalExpense
            WHERE Local_Exp_Date BETWEEN @FromDate AND @ToDate

            UNION ALL

            -- ProInventory Credit (Party Receivables)
            SELECT 
                InventoryID AS TransactionID,
                'Inventory' AS VoucherType,
                'INV-' + CAST(InventoryID AS NVARCHAR(10)) AS VoucherNo,
                PartyID,
                LaatNo +'Credit' AS Description,
                InvDate AS TransactionDate,
                ISNULL(Credit, 0) AS CreditAmount,
                ISNULL(Debit, 0) AS DebitAmount,
                'ProInventory' AS SourceTable
            FROM ProInventory
            WHERE InvDate BETWEEN @FromDate AND @ToDate AND ISNULL(Credit, 0) > 0

            UNION ALL

             -- ProInventory Debit (Party Payments)
            SELECT 
                InventoryID AS TransactionID,
                'Inventory' AS VoucherType,
                'INV-' + CAST(InventoryID AS NVARCHAR(10)) AS VoucherNo,
                PartyID,
                LaatNo + 'Debit' AS Description,
                InvDate AS TransactionDate,
                ISNULL(Credit, 0) AS CreditAmount,
                ISNULL(Debit, 0) AS DebitAmount,            
                'ProInventory' AS SourceTable
            FROM ProInventory
            WHERE InvDate BETWEEN @FromDate AND @ToDate AND ISNULL(Debit, 0) > 0
                    
            UNION ALL  
                            
            -- CashBook Receipts
            SELECT 
                cb.CashID AS TransactionID,
                'Receipt' AS VoucherType,
                'Rec-' + CAST(cb.CashID AS NVARCHAR(10)) AS VoucherNo,
                NULL AS PartyID,
                ISNULL(ca.Customer_Name, 'Unknown Customer ') + cb.Description AS Description,
                cb.Date AS TransactionDate,
                CASE WHEN Status = 'Credit' THEN cb.Cash_Entry ELSE 0 END AS CreditAmount,
                CASE WHEN Status = 'Debit' THEN cb.Cash_Entry ELSE 0 END AS DebitAmount,
                'Cashbook' AS SourceTable
            FROM CashBook cb
            LEFT JOIN CustomerAct ca ON cb.CustID = ca.CustID
            WHERE cb.Date BETWEEN @FromDate AND @ToDate
            ORDER BY TransactionDate DESC, TransactionID DESC";

                // Build additional WHERE conditions
                List<SqlParameter> parameters = new List<SqlParameter>();

                // Add date parameters
                parameters.Add(new SqlParameter("@FromDate", dateFrom.Value.Date));
                parameters.Add(new SqlParameter("@ToDate", dateTo.Value.Date));

                // Apply filters
                if (comboParty.SelectedIndex > 0 && comboParty.SelectedItem is KeyValuePair<int, string> selectedParty)
                {
                    query = AddPartyFilterToQuery(query, selectedParty.Key);
                    parameters.Add(new SqlParameter("@PartyID", selectedParty.Key));
                }

                if (comboVoucherType.SelectedIndex > 0)
                {
                    string typeComboVal = comboVoucherType.SelectedItem.ToString();
                    if (typeComboVal == "Payment")
                    {
                        comboParty.Enabled = true;
                    }
                    else
                    {
                        comboParty.Enabled = false;
                    }
                    query = AddVoucherTypeFilterToQuery(query, comboVoucherType.SelectedItem.ToString());
                    parameters.Add(new SqlParameter("@VoucherType", comboVoucherType.SelectedItem.ToString()));
                }

                if (!string.IsNullOrEmpty(textSearch.Text.Trim()))
                {
                    query = AddSearchFilterToQuery(query);
                    parameters.Add(new SqlParameter("@SearchText", $"%{textSearch.Text.Trim()}%"));
                }

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    foreach (var param in parameters)
                    {
                        cmd.Parameters.Add(param);
                    }

                    conn.Open();
                    DataTable dt = new DataTable();
                    using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                    {
                        da.Fill(dt);
                        dataGridViewLedger.DataSource = dt;
                    }
                }
            }
            FormatDataGridView();
            CalculateTotals();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error loading ledger data: {ex.Message}", "Error",
                          MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private decimal GetOpeningBalance()
    {
        try
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = @"
            SELECT 
                ISNULL(SUM(IncomeAmount) - SUM(ExpenseAmount), 0) AS OpeningBalance
            FROM (
                -- Party Payments: Money paid to parties (Expense)
                SELECT
                CASE When Status = 'Credit' Then
                    ISNULL(Entry_Amount, 0) ELSE 0
                END AS IncomeAmount, 
                CASE When Status = 'Debit' Then
                    ISNULL(Entry_Amount, 0) ELSE 0 
                END AS ExpenseAmount
                FROM PartyPayment 
                WHERE Pay_Date < @FromDate

                UNION ALL

                -- Sales from Orders: Money received (Income)
                SELECT ISNULL(Cash, 0) + ISNULL(Debit, 0) AS IncomeAmount, 0 AS ExpenseAmount
                FROM Orders 
                WHERE OrderDate < @FromDate

                UNION ALL

                -- Local Expenses: Money spent (Expense)
                SELECT 0 AS IncomeAmount, ISNULL(Local_Exp_Amount, 0) AS ExpenseAmount
                FROM LocalExpense 
                WHERE Local_Exp_Date < @FromDate

                UNION ALL

                -- ProInventory: 
                -- Debit = Purchase (Expense), Credit = Return/Refund (Income)
                SELECT 0 AS IncomeAmount, ISNULL(Debit, 0) AS ExpenseAmount
                FROM ProInventory 
                WHERE InvDate < @FromDate

                UNION ALL

                -- CashBook: Cash entries (Income)
                SELECT 
                    CASE 
                        WHEN Status = 'Credit' THEN Cash_Entry ELSE 0 END AS IncomeAmount,
                    CASE 
                        WHEN Status = 'Debit' THEN Cash_Entry ELSE 0 END AS ExpenseAmount
                FROM CashBook 
                WHERE Date < @FromDate
            ) AS AllTransactions";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@FromDate", dateFrom.Value.Date);
                    conn.Open();
                    object result = cmd.ExecuteScalar();
                    return result != null ? Convert.ToDecimal(result) : 0;
                }
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error calculating opening balance: {ex.Message}", "Error",
                          MessageBoxButtons.OK, MessageBoxIcon.Error);
            return 0;
        }
    }

    private string AddPartyFilterToQuery(string query, int partyId)
    {
        return query
            .Replace("WHERE Pay_Date BETWEEN @FromDate AND @ToDate",
                     "WHERE Pay_Date BETWEEN @FromDate AND @ToDate AND PartyID = @PartyID")
            .Replace("WHERE o.OrderDate BETWEEN @FromDate AND @ToDate",
                     "WHERE o.OrderDate BETWEEN @FromDate AND @ToDate") // Orders don't have PartyID
            .Replace("WHERE Local_Exp_Date BETWEEN @FromDate AND @ToDate",
                     "WHERE Local_Exp_Date BETWEEN @FromDate AND @ToDate") // Expenses don't have PartyID
            .Replace("WHERE InvDate BETWEEN @FromDate AND @ToDate AND ISNULL(Credit, 0) > 0",
                     "WHERE InvDate BETWEEN @FromDate AND @ToDate AND ISNULL(Credit, 0) > 0 AND PartyID = @PartyID")
            .Replace("WHERE InvDate BETWEEN @FromDate AND @ToDate AND ISNULL(Debit, 0) > 0",
                     "WHERE InvDate BETWEEN @FromDate AND @ToDate AND ISNULL(Debit, 0) > 0 AND PartyID = @PartyID")
            .Replace("WHERE cb.Date BETWEEN @FromDate AND @ToDate",
                     "WHERE cb.Date BETWEEN @FromDate AND @ToDate"); // CashBook doesn't have PartyID
    }


    // Helper method to add voucher type filter
    private string AddVoucherTypeFilterToQuery(string query, string voucherType)
    {
        return query
            .Replace("WHERE Pay_Date BETWEEN @FromDate AND @ToDate",
                     $"WHERE Pay_Date BETWEEN @FromDate AND @ToDate AND 'Payment' = @VoucherType")
            .Replace("WHERE o.OrderDate BETWEEN @FromDate AND @ToDate",
                     $"WHERE o.OrderDate BETWEEN @FromDate AND @ToDate AND 'Sale' = @VoucherType")
            .Replace("WHERE Local_Exp_Date BETWEEN @FromDate AND @ToDate",
                     $"WHERE Local_Exp_Date BETWEEN @FromDate AND @ToDate AND 'Expense' = @VoucherType")
            .Replace("WHERE InvDate BETWEEN @FromDate AND @ToDate AND ISNULL(Credit, 0) > 0",
                     $"WHERE InvDate BETWEEN @FromDate AND @ToDate AND ISNULL(Credit, 0) > 0 AND 'Inventory' = @VoucherType")
            .Replace("WHERE InvDate BETWEEN @FromDate AND @ToDate AND ISNULL(Debit, 0) > 0",
                     $"WHERE InvDate BETWEEN @FromDate AND @ToDate AND ISNULL(Debit, 0) > 0 AND 'Inventory' = @VoucherType")
            .Replace("WHERE cb.Date BETWEEN @FromDate AND @ToDate",
                     $"WHERE cb.Date BETWEEN @FromDate AND @ToDate AND 'Receipt' = @VoucherType");
    }

    private string AddSearchFilterToQuery(string query)
    {
        return query
            .Replace("WHERE Pay_Date BETWEEN @FromDate AND @ToDate",
                     "WHERE Pay_Date BETWEEN @FromDate AND @ToDate AND (PartyName LIKE @SearchText OR 'PY-' + CAST(P_PaymentID AS NVARCHAR(10)) LIKE @SearchText)")
            .Replace("WHERE o.OrderDate BETWEEN @FromDate AND @ToDate",
                     "WHERE o.OrderDate BETWEEN @FromDate AND @ToDate AND (Customer_Name LIKE @SearchText OR 'SL-' + CAST(OrderID AS NVARCHAR(10)) LIKE @SearchText)")
            .Replace("WHERE Local_Exp_Date BETWEEN @FromDate AND @ToDate",
                     "WHERE Local_Exp_Date BETWEEN @FromDate AND @ToDate AND (ActName LIKE @SearchText OR 'EXP-' + CAST(Local_Exp_ID AS NVARCHAR(10)) LIKE @SearchText)")
            .Replace("WHERE InvDate BETWEEN @FromDate AND @ToDate AND ISNULL(Credit, 0) > 0",
                     "WHERE InvDate BETWEEN @FromDate AND @ToDate AND ISNULL(Credit, 0) > 0 AND (LaatNo LIKE @SearchText OR 'INV-' + CAST(InventoryID AS NVARCHAR(10)) LIKE @SearchText)")
            .Replace("WHERE InvDate BETWEEN @FromDate AND @ToDate AND ISNULL(Debit, 0) > 0",
                     "WHERE InvDate BETWEEN @FromDate AND @ToDate AND ISNULL(Debit, 0) > 0 AND ('Inventory' LIKE @SearchText OR 'INV-' + CAST(InventoryID AS NVARCHAR(10)) LIKE @SearchText)")
            .Replace("WHERE cb.Date BETWEEN @FromDate AND @ToDate",
                     "WHERE cb.Date BETWEEN @FromDate AND @ToDate AND (ISNULL(ca.Customer_Name, 'Unknown Customer') LIKE @SearchText OR 'Rec-' + CAST(cb.CashID AS NVARCHAR(10)) LIKE @SearchText)");
    }

    // Calculate Totals
    // Calculate Totals from Actual Data

    public decimal GetCashInHand(DateTime? fromDate = null, DateTime? toDate = null)
    {
        decimal cashInHand = 0;

        // Simple approach: Build the SQL step by step
        StringBuilder sql = new StringBuilder();

        sql.Append(@"
SELECT 
    ((ISNULL((SELECT SUM(Cash_Entry) FROM Cashbook WHERE Status = 'Credit'");

        if (fromDate.HasValue && toDate.HasValue)
        {
            sql.Append($" AND Date BETWEEN '{fromDate.Value:yyyy-MM-dd}' AND '{toDate.Value:yyyy-MM-dd}'");
        }

        sql.Append(@"), 0) +
      ISNULL((SELECT SUM(Cash) FROM Orders");

        if (fromDate.HasValue && toDate.HasValue)
        {
            sql.Append($" WHERE OrderDate BETWEEN '{fromDate.Value:yyyy-MM-dd}' AND '{toDate.Value:yyyy-MM-dd}'");
        }

        sql.Append(@"), 0) +
      ISNULL((SELECT SUM(Entry_Amount) FROM PartyPayment WHERE Status = 'Credit'");

        if (fromDate.HasValue && toDate.HasValue)
        {
            sql.Append($" AND Pay_Date BETWEEN '{fromDate.Value:yyyy-MM-dd}' AND '{toDate.Value:yyyy-MM-dd}'");
        }

        sql.Append(@"), 0)) -
     (ISNULL((SELECT SUM(Cash) FROM ProInventory");

        if (fromDate.HasValue && toDate.HasValue)
        {
            sql.Append($" WHERE InvDate BETWEEN '{fromDate.Value:yyyy-MM-dd}' AND '{toDate.Value:yyyy-MM-dd}'");
        }

        sql.Append(@"), 0) +
      ISNULL((SELECT SUM(Entry_Amount) FROM PartyPayment WHERE Status = 'Debit'");

        if (fromDate.HasValue && toDate.HasValue)
        {
            sql.Append($" AND Pay_Date BETWEEN '{fromDate.Value:yyyy-MM-dd}' AND '{toDate.Value:yyyy-MM-dd}'");
        }

        sql.Append(@"), 0) +
      ISNULL((SELECT SUM(Local_Exp_Amount) FROM LocalExpense");

        if (fromDate.HasValue && toDate.HasValue)
        {
            sql.Append($" WHERE Local_Exp_Date BETWEEN '{fromDate.Value:yyyy-MM-dd}' AND '{toDate.Value:yyyy-MM-dd}'");
        }

        sql.Append(@"), 0) +
      ISNULL((SELECT SUM(Cash_Entry) FROM Cashbook WHERE Status = 'Debit'");

        if (fromDate.HasValue && toDate.HasValue)
        {
            sql.Append($" AND Date BETWEEN '{fromDate.Value:yyyy-MM-dd}' AND '{toDate.Value:yyyy-MM-dd}'");
        }

        sql.Append(@"), 0))) AS CashInHand");

        string query = sql.ToString();

        using (SqlConnection conn = new SqlConnection(connectionString))
        {
            conn.Open();
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                object result = cmd.ExecuteScalar();
                if (result != null && result != DBNull.Value)
                {
                    cashInHand = Convert.ToDecimal(result);
                }
            }
        }

        return cashInHand;
    }
    private void CalculateTotals()
    {
        try
        {
            decimal totalDebit = 0;
            decimal totalCredit = 0;

            foreach (DataGridViewRow row in dataGridViewLedger.Rows)
            {
                if (!row.IsNewRow && row.Visible)
                {
                    // Handle DebitAmount with better parsing
                    if (row.Cells["DebitAmount"].Value != null &&
                        decimal.TryParse(row.Cells["DebitAmount"].Value.ToString(),
                                        NumberStyles.Any,
                                        CultureInfo.InvariantCulture,
                                        out decimal debit))
                    {
                        totalDebit += debit;
                    }

                    // Handle CreditAmount with better parsing
                    if (row.Cells["CreditAmount"].Value != null &&
                        decimal.TryParse(row.Cells["CreditAmount"].Value.ToString(),
                                        NumberStyles.Any,
                                        CultureInfo.InvariantCulture,
                                        out decimal credit))
                    {
                        totalCredit += credit;
                    }
                }
            }

            decimal openingBalance = GetOpeningBalance();
            decimal closingBalance = openingBalance + totalCredit - totalDebit;

            // Update labels
            lblTotalDebit.Text = $"Total Debit: {totalDebit:N2}";
            lblTotalCredit.Text = $"Total Credit: {totalCredit:N2}";
            lblOpeningBalance.Text = $"Opening Balance: {openingBalance:N2}";
            lblClosingBalance.Text = $"Closing Balance: {closingBalance:N2}";

            // Color coding
            lblClosingBalance.ForeColor = closingBalance >= 0 ? Color.DarkGreen : Color.Red;
            lblOpeningBalance.ForeColor = openingBalance >= 0 ? Color.DarkGreen : Color.Red;
            lblTotalDebit.ForeColor = totalDebit >= 0 ? Color.Red : Color.DarkGreen;
            lblTotalCredit.ForeColor = totalCredit >= 0 ? Color.DarkGreen : Color.Red;
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error calculating totals: {ex.Message}\n\nStack Trace: {ex.StackTrace}", "Error",
                          MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
    // Get Opening Balance (balance before from date)
    // Get Opening Balance from actual transactions before from date


    // Format DataGridView with proper column names
    private void FormatDataGridView()
    {
        if (dataGridViewLedger.Columns.Count > 0)
        {
            dataGridViewLedger.Columns["TransactionID"].HeaderText = "Trans ID";
            dataGridViewLedger.Columns["TransactionID"].Width = 70;

            dataGridViewLedger.Columns["VoucherType"].HeaderText = "Type";
            dataGridViewLedger.Columns["VoucherType"].Width = 80;

            dataGridViewLedger.Columns["VoucherNo"].HeaderText = "Voucher No";
            dataGridViewLedger.Columns["VoucherNo"].Width = 100;

            if (dataGridViewLedger.Columns.Contains("PartyID"))
            {
                dataGridViewLedger.Columns["PartyID"].HeaderText = "Party ID";
                dataGridViewLedger.Columns["PartyID"].Width = 70;
            }

            dataGridViewLedger.Columns["Description"].HeaderText = "Description";
            dataGridViewLedger.Columns["Description"].Width = 200;

            dataGridViewLedger.Columns["TransactionDate"].HeaderText = "Date";
            dataGridViewLedger.Columns["TransactionDate"].Width = 80;
            dataGridViewLedger.Columns["TransactionDate"].DefaultCellStyle.Format = "dd/MM/yyyy";

            dataGridViewLedger.Columns["DebitAmount"].HeaderText = "Cash Out";
            dataGridViewLedger.Columns["DebitAmount"].Width = 90;
            dataGridViewLedger.Columns["DebitAmount"].DefaultCellStyle.Format = "N2";
            dataGridViewLedger.Columns["DebitAmount"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;

            dataGridViewLedger.Columns["CreditAmount"].HeaderText = "Cash In";
            dataGridViewLedger.Columns["CreditAmount"].Width = 90;
            dataGridViewLedger.Columns["CreditAmount"].DefaultCellStyle.Format = "N2";
            dataGridViewLedger.Columns["CreditAmount"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;

            dataGridViewLedger.Columns["SourceTable"].HeaderText = "Source";
            dataGridViewLedger.Columns["SourceTable"].Width = 100;

            dataGridViewLedger.RowHeadersVisible = false;
            dataGridViewLedger.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dataGridViewLedger.ReadOnly = true;

            // Color coding
            dataGridViewLedger.Columns["DebitAmount"].DefaultCellStyle.ForeColor = Color.DarkGreen;
            dataGridViewLedger.Columns["CreditAmount"].DefaultCellStyle.ForeColor = Color.DarkRed;
        }
    }

    // Format DataGridView
    // Format DataGridView with proper column names
    // Filter Events
    private void dateFrom_ValueChanged(object sender, EventArgs e) { LoadLedgerData(); lblCashBalance.Text = GetCashInHand(dateFrom.Value, dateTo.Value).ToString(); }
    private void dateTo_ValueChanged(object sender, EventArgs e) => LoadLedgerData();
    private void comboParty_SelectedIndexChanged(object sender, EventArgs e) => LoadLedgerData();
    private void comboAccount_SelectedIndexChanged(object sender, EventArgs e) => LoadLedgerData();
    private void comboVoucherType_SelectedIndexChanged(object sender, EventArgs e) => LoadLedgerData();
    private void textSearch_TextChanged(object sender, EventArgs e) => LoadLedgerData();
    // Refresh Button
    private void btnRefresh_Click(object sender, EventArgs e)
    {
        //LoadFilterOptions();
        LoadLedgerData();
        dateFrom.Value = DateTime.Now.AddMonths(-1);
        dateTo.Value = DateTime.Now;
        comboParty.SelectedIndex = 0;
        comboParty.Enabled = false;
        comboVoucherType.SelectedIndex = 0;
    }
    // Export to Excel (Optional)
    private void btnExport_Click(object sender, EventArgs e)
    {
        try
        {
                   // Get current totals
            decimal openingBalance = GetOpeningBalance();
            decimal totalDebit = 0;
            decimal totalCredit = 0;
            decimal closingBalance = 0;

            // Calculate current totals from DataGridView
            foreach (DataGridViewRow row in dataGridViewLedger.Rows)
            {
                if (!row.IsNewRow && row.Visible)
                {
                    if (row.Cells["CreditAmount"].Value != null &&
                        decimal.TryParse(row.Cells["CreditAmount"].Value.ToString(), out decimal credit))
                    {
                        totalCredit += credit;
                    }

                    if (row.Cells["DebitAmount"].Value != null &&
                        decimal.TryParse(row.Cells["DebitAmount"].Value.ToString(), out decimal debit))
                    {
                        totalDebit += debit;
                    }
                }
            }

            closingBalance = openingBalance + totalDebit - totalCredit;

            // Generate PDF
            PDFLedgerGenerator pdfGenerator = new PDFLedgerGenerator();
            pdfGenerator.GenerateLedgerPDF(dataGridViewLedger, dateFrom.Value, dateTo.Value,
                                         openingBalance, totalDebit, totalCredit, closingBalance);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error generating PDF: {ex.Message}", "Error",
                          MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
    private void frmLedger_Load_2(object sender, EventArgs e)
    {
        LoadFilterOptions();
        LoadLedgerData();
        
        dateFrom.Value = DateTime.Now.AddMonths(-1);
        dateTo.Value = DateTime.Now;
    }
    private void button1_Click(object sender, EventArgs e)
    {
        Expense expense = new Expense();
        expense.Show();
    }
}