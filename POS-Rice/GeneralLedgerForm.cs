using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Drawing.Printing;
using System.Windows.Forms;

namespace POS_Rice
{
    public partial class GeneralLedgerForm : Form
    {
        // Add these controls as class members
        private ListView listView1;
        private DataGridView dataGridView1;
        private Label lblAccountName;
        private Label lblBalance;
        private Label lblTotalDebit, lblAllDebit;
        private Label lblTotalCredit, lblAllCredit;
        private DateTimePicker dateFrom;
        private DateTimePicker dateTo;
        private Button btnFilter, btnPrint, btnExport;

        private string connectionString = @"Server=.\SQLEXPRESS;Database=Rice;Integrated Security=true;";
        private int selectedAccountID = 0;
        private string selectedAccountType = "";
        private string selectedAccountName = "";

        public GeneralLedgerForm()
        {
            InitializeComponent();
            SetupForm();

            if (dateFrom == null)
            {
                dateFrom = new DateTimePicker();
                dateTo = new DateTimePicker();
            }
        }

        private void SetupForm()
        {
            this.Text = "General Ledger";
            this.Size = new Size(1200, 700);
            this.StartPosition = FormStartPosition.CenterScreen;
            
            CreateControls();
            LoadAllAccounts();
            CalculateAndDisplayTotals();
        }
        private void btnExport_Click(object sender, EventArgs e)
        {
            try
            {
                SaveFileDialog saveFileDialog = new SaveFileDialog();
                saveFileDialog.Filter = "CSV files (*.csv)|*.csv";
                saveFileDialog.FileName = $"{selectedAccountName}_Ledger_{DateTime.Now:yyyyMMdd}.csv";

                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    using (System.IO.StreamWriter sw = new System.IO.StreamWriter(saveFileDialog.FileName))
                    {
                        // Write headers
                        sw.WriteLine("Date,Voucher No,Description,Debit,Credit,Balance,Source");

                        // Write data
                        foreach (DataGridViewRow row in dataGridView1.Rows)
                        {
                            if (!row.IsNewRow)
                            {
                                sw.WriteLine($"\"{row.Cells["TransactionDate"].Value}\"," +
                                            $"\"{row.Cells["VoucherNo"].Value}\"," +
                                            $"\"{row.Cells["Description"].Value}\"," +
                                            $"\"{row.Cells["DebitAmount"].Value}\"," +
                                            $"\"{row.Cells["CreditAmount"].Value}\"," +
                                            $"\"{row.Cells["Balance"].Value}\"," +
                                            $"\"{row.Cells["Source"].Value}\"");
                            }
                        }
                    }

                    MessageBox.Show($"Ledger exported to {saveFileDialog.FileName}", "Success",
                                  MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)

            {
                MessageBox.Show($"Error exporting: {ex.Message}", "Error",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private decimal GetBalanceFromLabel()
        {
            if (lblBalance.Text.Contains("RS"))
            {
                string balanceText = lblBalance.Text.Split('S')[1].Trim();
                if (decimal.TryParse(balanceText, out decimal balance))
                {
                    return balance;
                }
            }
            return 0;
        }

        private decimal GetTotalDebit()
        {
            if (lblTotalDebit.Text.Contains("RS"))
            {
                string debitText = lblTotalDebit.Text.Split('S')[1].Trim();
                if (decimal.TryParse(debitText, out decimal debit))
                {
                    return debit;
                }
            }
            return 0;
        }

        private decimal GetTotalCredit()
        {
            if (lblTotalCredit.Text.Contains("RS"))
            {
                string creditText = lblTotalCredit.Text.Split('S')[1].Trim();
                if (decimal.TryParse(creditText, out decimal credit))
                {
                    return credit;
                }
            }
            return 0;
        }
        private void btnPrint_Click(object sender, EventArgs e)
        {
            try
            {
                // Check if date controls are initialized
                if (dateFrom == null || dateTo == null)
                {
                    MessageBox.Show("Date controls are not initialized. Please restart the form.", "Error",
                                  MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // Check if there's any data to print
                if (dataGridView1 == null || dataGridView1.Rows.Count == 0)
                {
                    MessageBox.Show("No data available to generate PDF.", "Information",
                                  MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                // Check if an account is selected
                if (string.IsNullOrEmpty(selectedAccountName))
                {
                    MessageBox.Show("Please select an account first.", "Information",
                                  MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                // Get the values from your form
                string accountName = selectedAccountName;
                decimal currentBalance = GetBalanceFromLabel();
                decimal totalDebit = GetTotalDebit();
                decimal totalCredit = GetTotalCredit();

                // Call the PDF generator
                PDFGenerator.GenerateLedgerPDF(dataGridView1, accountName,
                                              dateFrom.Value, dateTo.Value,
                                              currentBalance, totalDebit, totalCredit);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error generating PDF: {ex.Message}", "Error",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

        }
        private void BtnFilter_Click(object sender, EventArgs e)
        {
            LoadAccountTransactions();
        }
        private void CreateControls()
        {
            // Search Panel
            Panel searchPanel = new Panel
            {
                Location = new Point(20, 20),
                Size = new Size(380, 40),
                BorderStyle = BorderStyle.FixedSingle
            };

            Label lblSearch = new Label
            {
                Text = "Search Account:",
                Location = new Point(10, 10),
                Size = new Size(110, 20)
            };

            TextBox txtSearch = new TextBox
            {
                Location = new Point(120, 10),
                Size = new Size(240, 20),
                Name = "txtSearch"
            };

            searchPanel.Controls.Add(lblSearch);
            searchPanel.Controls.Add(txtSearch);

            // ListView for Accounts
            listView1 = new ListView
            {
                Location = new Point(20, 70),
                Size = new Size(380, 550),
                View = View.Details,
                FullRowSelect = true,
                Name = "listView1",
                
            };

            listView1.Columns.Add("Account Type", 100);
            listView1.Columns.Add("Account Name", 150);
            listView1.Columns.Add("Balance", 80);
            listView1.Columns.Add("Status",50);
            listView1.Columns[2].TextAlign = HorizontalAlignment.Right;
            // GridView for Transactions
            dataGridView1 = new DataGridView
            {
                Location = new Point(440, 70),
                Size = new Size(900, 550),
                Name = "dataGridView1",
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                RowHeadersVisible = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            };

            // Summary Panel
            Panel summaryPanel = new Panel
            {
                Location = new Point(440, 20),
                Size = new Size(900, 40),
                BorderStyle = BorderStyle.FixedSingle
            };

            lblAccountName = new Label
            {
                Text = "Selected Account: None",
                Location = new Point(10, 10),
                Size = new Size(300, 20),
                Font = new Font("Arial", 10, FontStyle.Bold),
                Name = "lblAccountName"
            };

            lblBalance = new Label
            {
                Text = "Current Balance: RS 0.00",
                Location = new Point(320, 10),
                Size = new Size(250, 20),
                Font = new Font("Arial", 10, FontStyle.Bold),
                ForeColor = Color.DarkGreen,
                Name = "lblBalance"
            };

            lblTotalDebit = new Label
            {
                Text = "Total Debit: RS 0.00",
                Location = new Point(570, 10),
                Size = new Size(150, 20),
                Font = new Font("Arial", 9),
                Name = "lblTotalDebit"
            };

            lblTotalCredit = new Label
            {
                Text = "Total Credit: RS 0.00",
                Location = new Point(720, 10),
                Size = new Size(170, 20),
                Font = new Font("Arial", 9),
                Name = "lblTotalCredit"
            };

            summaryPanel.Controls.Add(lblAccountName);
            summaryPanel.Controls.Add(lblBalance);
            summaryPanel.Controls.Add(lblTotalDebit);
            summaryPanel.Controls.Add(lblTotalCredit);

            Panel TotsummaryPanel = new Panel
            {
                Location = new Point(20, 640),
                Size = new Size(380, 40),
                BorderStyle = BorderStyle.FixedSingle
            };

            lblAllCredit = new Label
            {
                Text = "All Credit: Rs: 120000.00",
                Location = new Point(10, 10),
                Size = new Size(160, 20),
                Font = new Font("Arial", 9),
                Name = "lblAllCredit",
                BackColor = Color.DarkSeaGreen,
                ForeColor = Color.White
            };

            lblAllDebit = new Label
            {
                Text = "All Debit: Rs: 120000.00",
                Location = new Point(200, 10),
                Size = new Size(160, 20),
                Font = new Font("Arial", 9),
                Name = "lblAllDebit",
                BackColor = Color.DarkRed,
                ForeColor = Color.White
            };

            TotsummaryPanel.Controls.Add(lblAllCredit);
            TotsummaryPanel.Controls.Add(lblAllDebit);

            dateFrom = new DateTimePicker
            {
                Location = new Point(440, 640),
                Size = new Size(100, 20),
                Format = DateTimePickerFormat.Short,
                Name = "dateFrom"
            };

            dateTo = new DateTimePicker
            {
                Location = new Point(550, 640),
                Size = new Size(100, 20),
                Format = DateTimePickerFormat.Short,
                Name = "dateTo",
                Value = DateTime.Today
            };

            

            Button btnFilter = new Button
            {
                Text = "Filter",
                Location = new Point(660, 640),
                Size = new Size(80, 25),
                Name = "btnFilter"
            };

            Button btnPrint = new Button
            {
                Text = "Print",
                Location = new Point(760, 640),
                Size = new Size(80, 25),
                Name = "btnPrint",
                BackColor = Color.DodgerBlue,
                ForeColor = Color.Black
            };

            Button btnExport = new Button
            {
                Text = "Export",
                Location = new Point(860, 640),
                Size = new Size(80, 25),
                Name = "btnExport",
                BackColor = Color.DarkSeaGreen,
                ForeColor = Color.Black
            };


            // Add controls to form
            this.Controls.Add(searchPanel);
            this.Controls.Add(listView1);
            this.Controls.Add(dataGridView1);
            this.Controls.Add(summaryPanel);
            this.Controls.Add(TotsummaryPanel);
            this.Controls.Add(dateFrom);
            this.Controls.Add(dateTo);
            this.Controls.Add(btnFilter);
            this.Controls.Add(btnPrint);
            this.Controls.Add(btnExport);


            // Event handlers
            btnPrint.Click += btnPrint_Click;
            btnFilter.Click += BtnFilter_Click;
            txtSearch.TextChanged += TxtSearch_TextChanged;
            listView1.SelectedIndexChanged += ListView1_SelectedIndexChanged;
            listView1.DoubleClick += ListView1_DoubleClick;
        }
        private void CalculateAndDisplayTotals()
        {
            try
            {
                decimal totalCredit = 0;
                decimal totalDebit = 0;

                foreach (ListViewItem item in listView1.Items)
                {
                    if (item.SubItems.Count > 3)
                    {
                        if (decimal.TryParse(item.SubItems[2].Text, out decimal balance))
                        {
                            // Determine based on account type
                            string accountType = item.SubItems[0].Text;
                            string status = item.SubItems[3].Text;
                            
                            switch (accountType)
                            {
                                case "Party":
                                    // For parties: Positive balance = they owe us (Credit)
                                    // Negative balance = we owe them (Debit)
                                    if (status == "Credit")
                                        totalCredit += balance;

                                    else if(status == "Debit")
                                        totalDebit += Math.Abs(balance); 
                                        break;

                                case "Customer":
                                    // For customers: Positive balance = they owe us (Credit)
                                    // Negative balance = we owe them (Debit)
                                    if (status == "Credit")
                                        totalCredit += balance;

                                    else if (status == "Debit")
                                        totalDebit += Math.Abs(balance);
                                    break;

                                case "Expense Account":
                                    // Expense accounts are always Debit
                                    totalDebit += Math.Abs(balance);
                                    break;

                                default:
                                    // Default logic
                                    if (balance >= 0)
                                        totalCredit += balance;
                                    else
                                        totalDebit += Math.Abs(balance);
                                    break;
                            }
                        }
                    }
                }

                // Update labels
                lblAllCredit.Text = $"All Credit: RS {totalCredit:N2}";
                lblAllDebit.Text = $"All Debit: RS {totalDebit:N2}";

                // Color code
                //lblAllCredit.ForeColor = totalCredit > 0 ? Color.DarkGreen : Color.DarkGray;
                //lblAllDebit.ForeColor = totalDebit > 0 ? Color.DarkRed : Color.DarkGray;

                // Optional: Show net difference
                decimal netDifference = totalCredit - totalDebit;
                // You could add another label for this
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error calculating totals: {ex.Message}", "Error",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void LoadAllAccounts()
        {
            try
            {
                listView1.Items.Clear();

                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    // Load Parties with their balances
                    LoadParties(conn);

                    // Load Customers with their balances
                    LoadCustomers(conn);

                    // Load Local Expense Accounts
                    LoadExpenseAccounts(conn);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading accounts: {ex.Message}", "Error",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void LoadParties(SqlConnection conn)
        {
            string query = @"
            SELECT 
    p.PartyID,
    p.PartyName,
    ISNULL(pp.TotalPaid, 0) AS TotalPaid,
    ISNULL(pp.TotalReturn, 0) AS TotalReturn,
    ISNULL(pi.TotalLaatReturn, 0) AS TotalLaatReturn,
    ISNULL(pi.TotalCreditPurchases, 0) AS TotalCreditPurchases,
    ISNULL(pi.TotalCashPurchases, 0) AS TotalCashPurchases
FROM Party p
LEFT JOIN (
    SELECT 
        PartyID,
        SUM(CASE WHEN Status = 'Debit' THEN Entry_Amount ELSE 0 END) AS TotalPaid,
        SUM(CASE WHEN Status = 'Credit' THEN Entry_Amount ELSE 0 END) AS TotalReturn
    FROM PartyPayment
    GROUP BY PartyID
) pp ON p.PartyID = pp.PartyID
LEFT JOIN (
    SELECT 
        PartyID,
        SUM(Debit) AS TotalLaatReturn,
        SUM(Credit) AS TotalCreditPurchases,
        SUM(Cash) AS TotalCashPurchases
    FROM ProInventory
    GROUP BY PartyID
) pi ON p.PartyID = pi.PartyID
ORDER BY p.PartyName";

            using (SqlCommand cmd = new SqlCommand(query, conn))
            using (SqlDataReader reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    int partyId = reader.GetInt32(0);
                    string partyName = reader.GetString(1);
                    decimal totalPaid = reader.GetDecimal(2);
                    decimal totalReturn = reader.GetDecimal(3);
                    decimal totalLaatReturn = reader.GetDecimal(4);
                    decimal totalCreditPurchases = reader.GetDecimal(5);
                    decimal totalCashPurchases = reader.GetDecimal(6);

                    // Calculate balance: Purchases - Returns - Payments + Sales (if applicable)
                    decimal balance = (totalCreditPurchases + totalReturn) - totalPaid - totalLaatReturn;

                    ListViewItem item = new ListViewItem("Party");
                    item.SubItems.Add(partyName);
                    item.SubItems.Add(Math.Abs(balance).ToString("N2"));
                    if (balance < 0)
                    {
                        item.SubItems.Add("Debit");
                    }
                    else if (balance == 0)
                    {
                        item.SubItems.Add("N/A");
                    }
                    else
                    {
                        item.SubItems.Add("Credit");
                    }
                    item.Tag = $"PARTY_{partyId}"; // Store as PARTY_{ID}

                    listView1.Items.Add(item);
                }
            }
        }
        private void LoadCustomers(SqlConnection conn)
        {
            string query = @"
            SELECT 
    c.CustID,
    c.Customer_Name,
    ISNULL(o.TotalCash, 0) AS TotalCash,
    ISNULL(o.TotalCredit, 0) AS TotalCredit,
    ISNULL(o.TotalDebit, 0) AS TotalDebit,
    ISNULL(cb.CashIn, 0) AS CashIn,
    ISNULL(cb.CashOut, 0) AS CashOut,
    ISNULL(o.TotalBalance, 0) AS TotalBalance
FROM CustomerAct c
LEFT JOIN (
    SELECT 
        CustID,
        SUM(Cash) AS TotalCash,
        SUM(Credit) AS TotalCredit,
        SUM(Debit) AS TotalDebit,
        SUM(Balance) AS TotalBalance
    FROM Orders
    GROUP BY CustID
) o ON c.CustID = o.CustID
LEFT JOIN (
    SELECT 
        CustID,
        SUM(CASE WHEN Status = 'Credit' THEN Cash_Entry ELSE 0 END) AS CashIn,
        SUM(CASE WHEN Status = 'Debit' THEN Cash_Entry ELSE 0 END) AS CashOut
    FROM Cashbook
    GROUP BY CustID
) cb ON c.CustID = cb.CustID
ORDER BY c.Customer_Name";

            using (SqlCommand cmd = new SqlCommand(query, conn))
            using (SqlDataReader reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    int custId = reader.GetInt32(0);
                    string customerName = reader.GetString(1);
                    decimal totalCash = reader.GetDecimal(2);       //ORDER CASH 
                    decimal totalCredit = reader.GetDecimal(3);     //ORDER CREDIT
                    decimal totalDebit = reader.GetDecimal(4);      //ORDER DEBIT
                    decimal cbCashIn = reader.GetDecimal(5);        //ORDER CREDIT
                    decimal cbCashOut = reader.GetDecimal(6);       //ORDER DEBIT
                    //decimal totalBalance = reader.GetDecimal(5);  

                    // Customer balance is what they owe (Credit + Debit sales)
                    decimal balance = totalCredit + cbCashOut - cbCashIn;

                    ListViewItem item = new ListViewItem("Customer");
                    item.SubItems.Add(customerName);
                    item.SubItems.Add(Math.Abs(balance).ToString("N2"));
                    if (balance < 0) 
                    {
                        item.SubItems.Add("Credit");
                    }
                    else if (balance == 0)
                    {
                        item.SubItems.Add("N/A");
                    }
                    else
                    {
                        item.SubItems.Add("Debit");
                    }
                        item.Tag = $"CUSTOMER_{custId}";

                    listView1.Items.Add(item);
                }
            }
        }
        private void LoadExpenseAccounts(SqlConnection conn)
        {
            string query = @"
            SELECT DISTINCT 
                ActName,
                ISNULL(SUM(Local_Exp_Amount), 0) AS TotalExpense
            FROM LocalExpense
            WHERE ActName IS NOT NULL AND ActName != ''
            GROUP BY ActName";

            using (SqlCommand cmd = new SqlCommand(query, conn))
            using (SqlDataReader reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    string accountName = reader.GetString(0);
                    decimal totalExpense = reader.GetDecimal(1);

                    ListViewItem item = new ListViewItem("Expense Account");
                    item.SubItems.Add(accountName);
                    item.SubItems.Add(totalExpense.ToString("N2"));
                    item.Tag = $"EXPENSE_{accountName}";

                    listView1.Items.Add(item);
                }
            }

            // Also add a general expense account
            string generalQuery = "SELECT ISNULL(SUM(Local_Exp_Amount), 0) FROM LocalExpense WHERE ActName IS NULL OR ActName = ''";
            using (SqlCommand cmd = new SqlCommand(generalQuery, conn))
            {
                object result = cmd.ExecuteScalar();
                decimal generalExpense = result != DBNull.Value ? Convert.ToDecimal(result) : 0;

                if (generalExpense > 0)
                {
                    ListViewItem item = new ListViewItem("Expense Account");
                    item.SubItems.Add("General Expenses");
                    item.SubItems.Add(generalExpense.ToString("N2"));
                    if (generalExpense < 0)
                    {
                        item.SubItems.Add("Debit");
                    }
                    else if(generalExpense == 0) 
                    {
                        item.SubItems.Add("N/A");
                    }
                    else
                    {
                        item.SubItems.Add("Credit");
                    }

                    item.Tag = $"EXPENSE_General";
                    listView1.Items.Add(item);
                }
            }
        }
        private void TxtSearch_TextChanged(object sender, EventArgs e)
        {
            TextBox txtSearch = sender as TextBox;
            if (txtSearch == null) return;

            string searchText = txtSearch.Text.Trim().ToLower();

            if (string.IsNullOrEmpty(searchText))
            {
                // Show all items
                foreach (ListViewItem item in listView1.Items)
                {
                    item.ForeColor = SystemColors.WindowText;
                    item.BackColor = SystemColors.Window;
                }
                return;
            }

            foreach (ListViewItem item in listView1.Items)
            {
                string accountName = item.SubItems[1].Text.ToLower();
                string accountType = item.SubItems[0].Text.ToLower();

                if (accountName.Contains(searchText) || accountType.Contains(searchText))
                {
                    item.ForeColor = Color.White;
                    item.BackColor = Color.SteelBlue;
                    item.Selected = false;
                }
                else
                {
                    item.ForeColor = SystemColors.WindowText;
                    item.BackColor = SystemColors.Window;
                }
            }
        }
        private void ListView1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count == 0) return;

            ListViewItem selectedItem = listView1.SelectedItems[0];

            // Parse account information from Tag
            string tag = selectedItem.Tag?.ToString() ?? "";

            if (tag.StartsWith("PARTY_"))
            {
                selectedAccountType = "Party";
                selectedAccountID = int.Parse(tag.Replace("PARTY_", ""));
                selectedAccountName = selectedItem.SubItems[1].Text;
            }
            else if (tag.StartsWith("CUSTOMER_"))
            {
                selectedAccountType = "Customer";
                selectedAccountID = int.Parse(tag.Replace("CUSTOMER_", ""));
                selectedAccountName = selectedItem.SubItems[1].Text;
            }
            else if (tag.StartsWith("EXPENSE_"))
            {
                selectedAccountType = "Expense";
                selectedAccountName = tag.Replace("EXPENSE_", "");
                if (selectedAccountName == "General") selectedAccountName = "General Expenses";
                selectedAccountID = 0;
            }

            // Update summary labels
            lblAccountName.Text = $"Selected Account: {selectedAccountName} ({selectedAccountType})";

            decimal balance = decimal.Parse(selectedItem.SubItems[2].Text);
            lblBalance.Text = $"Current Balance: RS {balance:N2}";
            lblBalance.ForeColor = balance >= 0 ? Color.DarkGreen : Color.DarkRed;

            // Load account transactions
            LoadAccountTransactions();
        }
        private void ListView1_DoubleClick(object sender, EventArgs e)
        {
            // Same as selection changed
            ListView1_SelectedIndexChanged(sender, e);
        }
        private void LoadAccountTransactions()
        {
            try
            {
                dataGridView1.DataSource = null;
                dataGridView1.Rows.Clear();
                dataGridView1.Columns.Clear();

                // Configure DataGridView
                dataGridView1.Columns.Add("TransactionDate", "Date");
                dataGridView1.Columns.Add("VoucherNo", "Voucher No");
                dataGridView1.Columns.Add("Description", "Description");
                dataGridView1.Columns.Add("DebitAmount", "Debit");
                dataGridView1.Columns.Add("CreditAmount", "Credit");
                dataGridView1.Columns.Add("Balance", "Balance");
                dataGridView1.Columns.Add("Source", "Source");

                // Format columns
                dataGridView1.Columns["TransactionDate"].DefaultCellStyle.Format = "dd/MM/yyyy";
                dataGridView1.Columns["DebitAmount"].DefaultCellStyle.Format = "N2";
                dataGridView1.Columns["DebitAmount"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
                dataGridView1.Columns["CreditAmount"].DefaultCellStyle.Format = "N2";
                dataGridView1.Columns["CreditAmount"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
                dataGridView1.Columns["Balance"].DefaultCellStyle.Format = "N2";
                dataGridView1.Columns["Balance"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;

                dataGridView1.Columns["DebitAmount"].DefaultCellStyle.ForeColor = Color.White;
                dataGridView1.Columns["DebitAmount"].DefaultCellStyle.BackColor = Color.DarkRed;
                dataGridView1.Columns["CreditAmount"].DefaultCellStyle.ForeColor = Color.White;
                dataGridView1.Columns["CreditAmount"].DefaultCellStyle.BackColor = Color.DarkGreen;

                decimal runningBalance = 0;
                decimal totalDebit = 0;
                decimal totalCredit = 0;

                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    DataTable transactions = new DataTable();

                    switch (selectedAccountType)
                    {
                        case "Party":
                            transactions = GetPartyTransactions(conn, selectedAccountID);
                            break;
                        case "Customer":
                            transactions = GetCustomerTransactions(conn, selectedAccountID);
                            break;
                        case "Expense":
                            transactions = GetExpenseTransactions(conn, selectedAccountName);
                            break;
                    }

                    foreach (DataRow row in transactions.Rows)
                    {
                        DateTime transDate = Convert.ToDateTime(row["TransactionDate"]);
                        string voucherNo = row["VoucherNo"].ToString();
                        string description = row["Description"].ToString();
                        decimal debit = Convert.ToDecimal(row["DebitAmount"]);
                        decimal credit = Convert.ToDecimal(row["CreditAmount"]);
                        string source = row["Source"].ToString();

                        // Calculate running balance
                        if (selectedAccountType == "Customer")
                        {
                            // For customers: Debit is increases balance (We owe them), Credit decreases
                            runningBalance += debit - credit;
                        }
                        else
                        {
                            // For parties and expenses: Credit increases balance (we owe them), Debit decreases
                            
                            runningBalance += credit - debit;
                        }

                        // Add to grid
                        dataGridView1.Rows.Add(
                            transDate,
                            voucherNo,
                            description,
                            debit > 0 ? debit.ToString("N2") : "",
                            credit > 0 ? credit.ToString("N2") : "",
                            Math.Abs(runningBalance).ToString("N2"),
                            source
                        );

                        totalDebit += debit;
                        totalCredit += credit;
                    }
                }

                // Update summary labels
                lblTotalDebit.Text = $"Total Debit: RS {totalDebit:N2}";
                lblTotalCredit.Text = $"Total Credit: RS {totalCredit:N2}";

                // Color code the balance column
                foreach (DataGridViewRow row in dataGridView1.Rows)
                {
                    if (!row.IsNewRow)
                    {
                        decimal balance = Convert.ToDecimal(row.Cells["Balance"].Value);
                        row.Cells["Balance"].Style.ForeColor = balance >= 0 ? Color.DarkGreen : Color.DarkRed;
                    }
                }

                // Auto-size columns
                dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
                dataGridView1.Columns["Description"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading transactions: {ex.Message}", "Error",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private DataTable GetPartyTransactions(SqlConnection conn, int partyId)
        {
            DataTable dt = new DataTable();

            string query = @"
            SELECT * FROM (
                -- Party Payments (Credit - we pay them)
                SELECT 
                    Pay_Date AS TransactionDate,
                    'PY-' + CAST(P_PaymentID AS NVARCHAR(10)) AS VoucherNo,
                    PartyName + ' - Payment' + Description AS Description,
                    CASE WHEN Status = 'Debit' THEN Entry_Amount ELSE 0 END AS DebitAmount,
                    CASE WHEN Status = 'Credit' THEN Entry_Amount ELSE 0 END AS CreditAmount,
                    'PartyPayment' AS Source
                FROM PartyPayment 
                WHERE PartyID = @AccountID

                UNION ALL

                -- ProInventory Purchases (Debit - we buy from them)
                SELECT 
                    InvDate AS TransactionDate,
                    'INV-' + CAST(InventoryID AS NVARCHAR(10)) AS VoucherNo,
                    'Purchase - ' + ISNULL(P.ProductName, 'Product') AS Description,
                    Debit AS DebitAmount,
                    0 AS CreditAmount,
                    'ProInventory' AS Source
                FROM ProInventory PI
                LEFT JOIN Product P ON PI.ProductID = P.ProductID
                WHERE PI.PartyID = @AccountID AND ISNULL(Debit, 0) > 0

                UNION ALL

                -- ProInventory Returns (Credit - we return to them)
                SELECT 
                    InvDate AS TransactionDate,
                    'INV-' + CAST(InventoryID AS NVARCHAR(10)) AS VoucherNo,
                    'Return - ' + ISNULL(P.ProductName, 'Product') AS Description,
                    0 AS DebitAmount,
                    Credit AS CreditAmount,
                    'ProInventory' AS Source
                FROM ProInventory PI
                LEFT JOIN Product P ON PI.ProductID = P.ProductID
                WHERE PI.PartyID = @AccountID AND ISNULL(Credit, 0) > 0
            ) AS Transactions
            ORDER BY TransactionDate, VoucherNo";

            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@AccountID", partyId);
                using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                {
                    da.Fill(dt);
                }
            }

            return dt;
        }
        private DataTable GetCustomerTransactions(SqlConnection conn, int customerId)
        {
            DataTable dt = new DataTable();

            string query = @"
            SELECT * FROM (
    -- Cash Sales (Debit - customer pays cash)
    SELECT 
        OrderDate AS TransactionDate,
        'SO-' + CAST(OrderID AS NVARCHAR(10)) AS VoucherNo,
        'Cash Sale' AS Description,
        0 AS DebitAmount,
        Cash AS CreditAmount,
        'Orders' AS Source
    FROM Orders 
    WHERE CustID = @AccountID AND ISNULL(Cash, 0) > 0

    UNION ALL

    -- Credit Sales (Credit - customer buys on credit)
    SELECT 
        OrderDate AS TransactionDate,
        'SO-' + CAST(OrderID AS NVARCHAR(10)) AS VoucherNo,
        'Credit Sale' AS Description,
        Credit AS DebitAmount,
        0 AS CreditAmount,
        'Orders' AS Source
    FROM Orders 
    WHERE CustID = @AccountID AND ISNULL(Credit, 0) > 0

    UNION ALL

    -- Debit Card Sales (Debit - customer pays via debit)
    SELECT 
        OrderDate AS TransactionDate,
        'SO-' + CAST(OrderID AS NVARCHAR(10)) AS VoucherNo,
        'Debit Card Sale' AS Description,
        Debit AS DebitAmount,
        0 AS CreditAmount,
        'Orders' AS Source
    FROM Orders 
    WHERE CustID = @AccountID AND ISNULL(Debit, 0) > 0

    UNION ALL

    -- Cashbook - Customer Payments (Credit - customer pays us)
    SELECT 
        Date AS TransactionDate,
        'CB-' + CAST(CashID AS NVARCHAR(10)) AS VoucherNo,
        'Customer Payment - ' + ISNULL(Description, 'Payment') AS Description,
        0 AS DebitAmount,
        Cash_Entry AS CreditAmount,
        'Cashbook' AS Source
    FROM Cashbook 
    WHERE CustID = @AccountID AND Status = 'Credit' AND ISNULL(Cash_Entry, 0) > 0

    UNION ALL

    -- Cashbook - We Pay Customer (Debit - we pay customer)
    SELECT 
        Date AS TransactionDate,
        'CB-' + CAST(CashID AS NVARCHAR(10)) AS VoucherNo,
        'Payment to Customer - ' + ISNULL(Description, 'Payment') AS Description,
        Cash_Entry AS DebitAmount,
        0 AS CreditAmount,
        'Cashbook' AS Source
    FROM Cashbook 
    WHERE CustID = @AccountID AND Status = 'Debit' AND ISNULL(Cash_Entry, 0) > 0

) AS Transactions
ORDER BY TransactionDate, VoucherNo";

            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@AccountID", customerId);
                using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                {
                    da.Fill(dt);
                }
            }

            return dt;
        }
        private DataTable GetExpenseTransactions(SqlConnection conn, string accountName)
        {
            DataTable dt = new DataTable();

            string query = @"
            SELECT 
                Local_Exp_Date AS TransactionDate,
                'EXP-' + CAST(Local_Exp_ID AS NVARCHAR(10)) AS VoucherNo,
                'Expense - ' + ISNULL(ActName, 'General') AS Description,
                Local_Exp_Amount AS DebitAmount,
                0 AS CreditAmount,
                'LocalExpense' AS Source
            FROM LocalExpense 
            WHERE (@AccountName = 'General Expenses' AND (ActName IS NULL OR ActName = ''))
               OR ActName = @AccountName
            ORDER BY TransactionDate, VoucherNo";

            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@AccountName",
                    accountName == "General Expenses" ? "" : accountName);
                using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                {
                    da.Fill(dt);
                }
            }

            return dt;
        }

    }
}