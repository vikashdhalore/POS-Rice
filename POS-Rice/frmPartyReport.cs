using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace POS_Rice
{
    public partial class frmPartyReport : Form
    {
        private string connectionString = @"Server=.\SQLEXPRESS;Database=Rice;Integrated Security=true;";

        // Controls
        private DateTimePicker dateFrom, dateTo;
        private ComboBox comboParty, comboReportType;
        private Button btnGenerate, btnExport, btnPrint;
        private DataGridView dataGridViewPartyReport;
        private Panel panelSummary;
        private Label lblTotalCredit, lblTotalDebit, lblNetBalance, lblTotalPayments;
        private Label lblActiveParties, lblTotalTransactions, lblHighestBalance, lblAvgTransaction;

        public frmPartyReport()
        {
            InitializeComponent();
            SetupForm();
        }

        private void SetupForm()
        {
            // Form settings
            this.Text = "Party Report";
            this.Size = new Size(1200, 750);
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
            var lblParty = new Label { Text = "Party:", Location = new Point(420, 20), Size = new Size(40, 20), Font = new Font("Arial", 9, FontStyle.Bold) };
            var lblReportType = new Label { Text = "Report Type:", Location = new Point(620, 20), Size = new Size(80, 20), Font = new Font("Arial", 9, FontStyle.Bold) };

            // Date Pickers
            dateFrom = new DateTimePicker { Location = new Point(90, 20), Size = new Size(120, 20), Format = DateTimePickerFormat.Short };
            dateTo = new DateTimePicker { Location = new Point(290, 20), Size = new Size(120, 20), Format = DateTimePickerFormat.Short };

            // Set default dates (last 30 days)
            dateFrom.Value = DateTime.Now.AddDays(-30);
            dateTo.Value = DateTime.Now;

            // ComboBoxes
            comboParty = new ComboBox { Location = new Point(460, 20), Size = new Size(150, 20), DropDownStyle = ComboBoxStyle.DropDownList };
            comboReportType = new ComboBox { Location = new Point(700, 20), Size = new Size(180, 20), DropDownStyle = ComboBoxStyle.DropDownList };

            // Buttons
            btnGenerate = new Button { Text = "Generate Report", Location = new Point(890, 18), Size = new Size(120, 25), BackColor = Color.SteelBlue, ForeColor = Color.White };
            btnExport = new Button { Text = "Export Excel", Location = new Point(1020, 18), Size = new Size(100, 25), BackColor = Color.Green, ForeColor = Color.White };
            btnPrint = new Button { Text = "Print", Location = new Point(1130, 18), Size = new Size(80, 25), BackColor = Color.Orange, ForeColor = Color.White };

            // DataGridView
            dataGridViewPartyReport = new DataGridView { Location = new Point(20, 60), Size = new Size(1150, 400) };

            // Summary Panel
            panelSummary = new Panel { Location = new Point(20, 470), Size = new Size(1150, 100), BorderStyle = BorderStyle.FixedSingle, BackColor = Color.LightYellow };

            // Main Summary Labels (Top Row)
            lblTotalCredit = new Label { Text = "Total Credit: ₹0.00", Location = new Point(20, 15), Size = new Size(150, 25), Font = new Font("Arial", 11, FontStyle.Bold), ForeColor = Color.DarkRed };
            lblTotalDebit = new Label { Text = "Total Debit: ₹0.00", Location = new Point(190, 15), Size = new Size(150, 25), Font = new Font("Arial", 11, FontStyle.Bold), ForeColor = Color.DarkGreen };
            lblNetBalance = new Label { Text = "Net Balance: ₹0.00", Location = new Point(360, 15), Size = new Size(150, 25), Font = new Font("Arial", 11, FontStyle.Bold), ForeColor = Color.DarkBlue };
            lblTotalPayments = new Label { Text = "Total Payments: ₹0.00", Location = new Point(530, 15), Size = new Size(150, 25), Font = new Font("Arial", 11, FontStyle.Bold), ForeColor = Color.Purple };

            // Secondary Summary Labels (Bottom Row)
            lblActiveParties = new Label { Text = "Active Parties: 0", Location = new Point(20, 50), Size = new Size(120, 20), Font = new Font("Arial", 9, FontStyle.Bold) };
            lblTotalTransactions = new Label { Text = "Total Transactions: 0", Location = new Point(160, 50), Size = new Size(130, 20), Font = new Font("Arial", 9, FontStyle.Bold) };
            lblHighestBalance = new Label { Text = "Highest Balance: ₹0.00", Location = new Point(310, 50), Size = new Size(140, 20), Font = new Font("Arial", 9, FontStyle.Bold) };
            lblAvgTransaction = new Label { Text = "Avg Transaction: ₹0.00", Location = new Point(470, 50), Size = new Size(140, 20), Font = new Font("Arial", 9, FontStyle.Bold) };

            // Additional Metrics
            var lblCreditParties = new Label { Text = "Parties with Credit: 0", Location = new Point(630, 50), Size = new Size(140, 20), Font = new Font("Arial", 9, FontStyle.Bold) };
            var lblDebitParties = new Label { Text = "Parties with Debit: 0", Location = new Point(790, 50), Size = new Size(140, 20), Font = new Font("Arial", 9, FontStyle.Bold) };

            // Add controls to panel
            panelSummary.Controls.AddRange(new Control[] {
                lblTotalCredit, lblTotalDebit, lblNetBalance, lblTotalPayments,
                lblActiveParties, lblTotalTransactions, lblHighestBalance, lblAvgTransaction,
                lblCreditParties, lblDebitParties
            });

            // Add controls to form
            this.Controls.AddRange(new Control[] {
                lblFrom, lblTo, lblParty, lblReportType,
                dateFrom, dateTo, comboParty, comboReportType,
                btnGenerate, btnExport, btnPrint,
                dataGridViewPartyReport, panelSummary
            });

            // Event handlers
            btnGenerate.Click += btnGenerate_Click;
            btnExport.Click += btnExport_Click;
            btnPrint.Click += btnPrint_Click;
            dataGridViewPartyReport.RowPostPaint += DataGridViewPartyReport_RowPostPaint;
        }

        private void LoadFilterOptions()
        {
            // Load Parties
            LoadParties();

            // Report Types
            comboReportType.Items.AddRange(new string[] {
                "Party Balance Summary",
                "Party Transaction Details",
                "Party Ledger Statement",
                "Credit/Debit Analysis",
                "Payment History"
            });
            comboReportType.SelectedIndex = 0;
        }

        private void LoadParties()
        {
            try
            {
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
                            comboParty.Items.Add("All Parties");

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
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading parties: {ex.Message}", "Error",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnGenerate_Click(object sender, EventArgs e)
        {
            GeneratePartyReport();
        }

        private void GeneratePartyReport()
        {
            try
            {
                string reportType = comboReportType.SelectedItem.ToString();
                DataTable dt = new DataTable();

                switch (reportType)
                {
                    case "Party Balance Summary":
                        dt = GetPartyBalanceSummary();
                        break;
                    case "Party Transaction Details":
                        dt = GetPartyTransactionDetails();
                        break;
                    case "Party Ledger Statement":
                        dt = GetPartyLedgerStatement();
                        break;
                    case "Credit/Debit Analysis":
                        dt = GetCreditDebitAnalysis();
                        break;
                    case "Payment History":
                        dt = GetPaymentHistory();
                        break;
                }

                dataGridViewPartyReport.DataSource = dt;
                FormatDataGridView();
                CalculatePartySummary();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error generating report: {ex.Message}", "Error",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private DataTable GetPartyBalanceSummary()
        {
            DataTable dt = new DataTable();

            string query = @"
            ;WITH PartyCredits AS (
                SELECT 
                    p.PartyID,
                    p.PartyName,
                    p.PhoneNo,
                    SUM(ISNULL(pi.Credit, 0)) AS TotalCredit
                FROM Party p
                LEFT JOIN ProInventory pi ON p.PartyID = pi.PartyID
                WHERE pi.InvDate BETWEEN @FromDate AND @ToDate
                GROUP BY p.PartyID, p.PartyName, p.PhoneNo
            ),
            PartyDebits AS (
                SELECT 
                    p.PartyID,
                    SUM(ISNULL(pi.Debit, 0)) AS TotalDebit
                FROM Party p
                LEFT JOIN ProInventory pi ON p.PartyID = pi.PartyID
                WHERE pi.InvDate BETWEEN @FromDate AND @ToDate
                GROUP BY p.PartyID
            ),
            PartyPayments AS (
                SELECT 
                    PartyID,
                    SUM(ISNULL(Entry_Amount, 0)) AS TotalPayments,
                    COUNT(P_PaymentID) AS PaymentCount
                FROM PartyPayment
                WHERE Pay_Date BETWEEN @FromDate AND @ToDate
                GROUP BY PartyID
            )
            SELECT 
                pc.PartyID,
                pc.PartyName,
                pc.PhoneNo,
                ISNULL(pc.TotalCredit, 0) AS TotalCredit,
                ISNULL(pd.TotalDebit, 0) AS TotalDebit,
                ISNULL(pp.TotalPayments, 0) AS TotalPayments,
                ISNULL(pc.TotalCredit, 0) - ISNULL(pd.TotalDebit, 0) - ISNULL(pp.TotalPayments, 0) AS NetBalance,
                ISNULL(pp.PaymentCount, 0) AS PaymentCount,
                CASE 
                    WHEN (ISNULL(pc.TotalCredit, 0) - ISNULL(pd.TotalDebit, 0) - ISNULL(pp.TotalPayments, 0)) > 0 
                    THEN 'Credit Balance'
                    WHEN (ISNULL(pc.TotalCredit, 0) - ISNULL(pd.TotalDebit, 0) - ISNULL(pp.TotalPayments, 0)) < 0 
                    THEN 'Debit Balance'
                    ELSE 'Settled'
                END AS BalanceStatus
            FROM PartyCredits pc
            LEFT JOIN PartyDebits pd ON pc.PartyID = pd.PartyID
            LEFT JOIN PartyPayments pp ON pc.PartyID = pp.PartyID
            WHERE ISNULL(pc.TotalCredit, 0) != 0 OR ISNULL(pd.TotalDebit, 0) != 0 OR ISNULL(pp.TotalPayments, 0) != 0";

            // Add party filter if specific party selected
            if (comboParty.SelectedIndex > 0 && comboParty.SelectedItem is KeyValuePair<int, string> selectedParty)
            {
                query += " AND pc.PartyID = @PartyID";
            }

            query += " ORDER BY NetBalance DESC";

            using (SqlConnection conn = new SqlConnection(connectionString))
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@FromDate", dateFrom.Value.Date);
                cmd.Parameters.AddWithValue("@ToDate", dateTo.Value.Date);

                if (comboParty.SelectedIndex > 0 && comboParty.SelectedItem is KeyValuePair<int, string>sselectedParty)
                {
                    
                    cmd.Parameters.AddWithValue("@PartyID", sselectedParty.Key);
                }

                conn.Open();
                using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                {
                    da.Fill(dt);
                }
            }

            return dt;
        }

        private DataTable GetPartyTransactionDetails()
        {
            DataTable dt = new DataTable();

            string query = @"
            SELECT 
                'Inventory Credit' AS TransactionType,
                pi.InventoryID AS ReferenceID,
                p.PartyName,
                pi.LaatNo AS Description,
                pi.InvDate AS TransactionDate,
                pi.Credit AS Amount,
                0 AS Debit,
                '' AS PaymentMethod
            FROM ProInventory pi
            INNER JOIN Party p ON pi.PartyID = p.PartyID
            WHERE pi.InvDate BETWEEN @FromDate AND @ToDate
            AND ISNULL(pi.Credit, 0) > 0

            UNION ALL

            SELECT 
                'Inventory Debit' AS TransactionType,
                pi.InventoryID AS ReferenceID,
                p.PartyName,
                pi.LaatNo AS Description,
                pi.InvDate AS TransactionDate,
                0 AS Amount,
                pi.Debit AS Debit,
                '' AS PaymentMethod
            FROM ProInventory pi
            INNER JOIN Party p ON pi.PartyID = p.PartyID
            WHERE pi.InvDate BETWEEN @FromDate AND @ToDate
            AND ISNULL(pi.Debit, 0) > 0

            UNION ALL

            SELECT 
                'Party Payment' AS TransactionType,
                pp.P_PaymentID AS ReferenceID,
                p.PartyName,
                'Payment Received' AS Description,
                pp.Pay_Date AS TransactionDate,
                0 AS Amount,
                pp.Entry_Amount AS Debit,
                'Cash' AS PaymentMethod
            FROM PartyPayment pp
            INNER JOIN Party p ON pp.PartyID = p.PartyID
            WHERE pp.Pay_Date BETWEEN @FromDate AND @ToDate

            ORDER BY TransactionDate DESC, PartyName";

            // Add party filter if specific party selected
            if (comboParty.SelectedIndex > 0 && comboParty.SelectedItem is KeyValuePair<int, string> selectedParty)
            {
                query = query.Replace("WHERE pi.InvDate", "WHERE pi.PartyID = @PartyID AND pi.InvDate")
                            .Replace("WHERE pp.Pay_Date", "WHERE pp.PartyID = @PartyID AND pp.Pay_Date");
            }

            using (SqlConnection conn = new SqlConnection(connectionString))
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@FromDate", dateFrom.Value.Date);
                cmd.Parameters.AddWithValue("@ToDate", dateTo.Value.Date);

                if (comboParty.SelectedIndex > 0 && comboParty.SelectedItem is KeyValuePair<int, string> sselectedParty)
                {
                    cmd.Parameters.AddWithValue("@PartyID", sselectedParty.Key);
                }

                conn.Open();
                using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                {
                    da.Fill(dt);
                }
            }

            return dt;
        }

        private DataTable GetPartyLedgerStatement()
        {
            DataTable dt = new DataTable();

            string query = @"
            ;WITH PartyTransactions AS (
                SELECT 
                    p.PartyID,
                    p.PartyName,
                    pi.InvDate AS TransactionDate,
                    'CREDIT' AS TransactionType,
                    pi.LaatNo AS Description,
                    pi.Credit AS Credit,
                    0 AS Debit,
                    NULL AS PaymentID
                FROM ProInventory pi
                INNER JOIN Party p ON pi.PartyID = p.PartyID
                WHERE pi.InvDate BETWEEN @FromDate AND @ToDate
                AND ISNULL(pi.Credit, 0) > 0

                UNION ALL

                SELECT 
                    p.PartyID,
                    p.PartyName,
                    pi.InvDate AS TransactionDate,
                    'DEBIT' AS TransactionType,
                    pi.LaatNo AS Description,
                    0 AS Credit,
                    pi.Debit AS Debit,
                    NULL AS PaymentID
                FROM ProInventory pi
                INNER JOIN Party p ON pi.PartyID = p.PartyID
                WHERE pi.InvDate BETWEEN @FromDate AND @ToDate
                AND ISNULL(pi.Debit, 0) > 0

                UNION ALL

                SELECT 
                    p.PartyID,
                    p.PartyName,
                    pp.Pay_Date AS TransactionDate,
                    'PAYMENT' AS TransactionType,
                    'Payment Received' AS Description,
                    0 AS Credit,
                    pp.Entry_Amount AS Debit,
                    pp.P_PaymentID AS PaymentID
                FROM PartyPayment pp
                INNER JOIN Party p ON pp.PartyID = p.PartyID
                WHERE pp.Pay_Date BETWEEN @FromDate AND @ToDate
            )
            SELECT 
                PartyName,
                TransactionDate,
                TransactionType,
                Description,
                Credit,
                Debit,
                SUM(Credit - Debit) OVER (PARTITION BY PartyID ORDER BY TransactionDate, PaymentID) AS RunningBalance
            FROM PartyTransactions
            WHERE 1=1";

            // Add party filter if specific party selected
            if (comboParty.SelectedIndex > 0 && comboParty.SelectedItem is KeyValuePair<int, string> selectedParty)
            {
                query += " AND PartyID = @PartyID";
            }

            query += " ORDER BY PartyName, TransactionDate, TransactionType";

            using (SqlConnection conn = new SqlConnection(connectionString))
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@FromDate", dateFrom.Value.Date);
                cmd.Parameters.AddWithValue("@ToDate", dateTo.Value.Date);

                if (comboParty.SelectedIndex > 0 && comboParty.SelectedItem is KeyValuePair<int, string> sselectedParty)
                {
                    cmd.Parameters.AddWithValue("@PartyID", sselectedParty.Key);
                }

                conn.Open();
                using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                {
                    da.Fill(dt);
                }
            }

            return dt;
        }

        private DataTable GetCreditDebitAnalysis()
        {
            DataTable dt = new DataTable();

            string query = @"
            SELECT 
                p.PartyName,
                COUNT(DISTINCT pi.InventoryID) AS TotalTransactions,
                SUM(ISNULL(pi.Credit, 0)) AS TotalCredit,
                SUM(ISNULL(pi.Debit, 0)) AS TotalDebit,
                COUNT(DISTINCT pp.P_PaymentID) AS PaymentCount,
                SUM(ISNULL(pp.Entry_Amount, 0)) AS TotalPayments,
                AVG(ISNULL(pi.Credit, 0)) AS AvgCredit,
                AVG(ISNULL(pi.Debit, 0)) AS AvgDebit,
                MAX(ISNULL(pi.Credit, 0)) AS MaxCredit,
                MAX(ISNULL(pi.Debit, 0)) AS MaxDebit,
                CASE 
                    WHEN SUM(ISNULL(pi.Credit, 0)) > SUM(ISNULL(pi.Debit, 0)) + SUM(ISNULL(pp.Entry_Amount, 0))
                    THEN 'Net Creditor'
                    WHEN SUM(ISNULL(pi.Credit, 0)) < SUM(ISNULL(pi.Debit, 0)) + SUM(ISNULL(pp.Entry_Amount, 0))
                    THEN 'Net Debtor'
                    ELSE 'Balanced'
                END AS FinancialStatus
            FROM Party p
            LEFT JOIN ProInventory pi ON p.PartyID = pi.PartyID AND pi.InvDate BETWEEN @FromDate AND @ToDate
            LEFT JOIN PartyPayment pp ON p.PartyID = pp.PartyID AND pp.Pay_Date BETWEEN @FromDate AND @ToDate
            WHERE ISNULL(pi.Credit, 0) != 0 OR ISNULL(pi.Debit, 0) != 0 OR ISNULL(pp.Entry_Amount, 0) != 0";

            // Add party filter if specific party selected
            if (comboParty.SelectedIndex > 0 && comboParty.SelectedItem is KeyValuePair<int, string> selectedParty)
            {
                query += " AND p.PartyID = @PartyID";
            }

            query += @"
            GROUP BY p.PartyID, p.PartyName
            ORDER BY TotalCredit DESC, TotalDebit DESC";

            using (SqlConnection conn = new SqlConnection(connectionString))
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@FromDate", dateFrom.Value.Date);
                cmd.Parameters.AddWithValue("@ToDate", dateTo.Value.Date);

                if (comboParty.SelectedIndex > 0 && comboParty.SelectedItem is KeyValuePair<int, string> sselectedParty)
                {
                    cmd.Parameters.AddWithValue("@PartyID", sselectedParty.Key);
                }

                conn.Open();
                using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                {
                    da.Fill(dt);
                }
            }

            return dt;
        }

        private DataTable GetPaymentHistory()
        {
            DataTable dt = new DataTable();

            string query = @"
            SELECT 
                p.PartyName,
                pp.P_PaymentID AS PaymentID,
                pp.Pay_Date AS PaymentDate,
                pp.Entry_Amount AS Amount,
                pp.PartyName AS ReceivedFrom,
                DATEDIFF(DAY, 
                    (SELECT MAX(InvDate) FROM ProInventory WHERE PartyID = p.PartyID AND InvDate <= pp.Pay_Date),
                    pp.Pay_Date
                ) AS DaysSinceLastTransaction
            FROM PartyPayment pp
            INNER JOIN Party p ON pp.PartyID = p.PartyID
            WHERE pp.Pay_Date BETWEEN @FromDate AND @ToDate";

            // Add party filter if specific party selected
            if (comboParty.SelectedIndex > 0 && comboParty.SelectedItem is KeyValuePair<int, string> selectedParty)
            {
                query += " AND pp.PartyID = @PartyID";
            }

            query += " ORDER BY pp.Pay_Date DESC";

            using (SqlConnection conn = new SqlConnection(connectionString))
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@FromDate", dateFrom.Value.Date);
                cmd.Parameters.AddWithValue("@ToDate", dateTo.Value.Date);

                if (comboParty.SelectedIndex > 0 && comboParty.SelectedItem is KeyValuePair<int, string> sselectedParty)
                {
                    cmd.Parameters.AddWithValue("@PartyID", sselectedParty.Key);
                }

                conn.Open();
                using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                {
                    da.Fill(dt);
                }
            }

            return dt;
        }

        private void FormatDataGridView()
        {
            if (dataGridViewPartyReport.Columns.Count == 0) return;

            // Add row number column
            if (!dataGridViewPartyReport.Columns.Contains("RowNumber"))
            {
                DataGridViewTextBoxColumn rowNumberColumn = new DataGridViewTextBoxColumn();
                rowNumberColumn.Name = "RowNumber";
                rowNumberColumn.HeaderText = "Sr. No.";
                rowNumberColumn.Width = 60;
                rowNumberColumn.ReadOnly = true;
                rowNumberColumn.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                rowNumberColumn.DefaultCellStyle.BackColor = Color.LightGray;
                rowNumberColumn.DefaultCellStyle.Font = new Font("Arial", 8, FontStyle.Bold);

                dataGridViewPartyReport.Columns.Insert(0, rowNumberColumn);
            }

            dataGridViewPartyReport.RowHeadersVisible = false;
            dataGridViewPartyReport.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dataGridViewPartyReport.ReadOnly = true;
            dataGridViewPartyReport.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

            // Format headers
            dataGridViewPartyReport.EnableHeadersVisualStyles = false;
            dataGridViewPartyReport.ColumnHeadersDefaultCellStyle.BackColor = Color.SteelBlue;
            dataGridViewPartyReport.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dataGridViewPartyReport.ColumnHeadersDefaultCellStyle.Font = new Font("Arial", 9, FontStyle.Bold);
            dataGridViewPartyReport.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;

            // Format columns based on content
            foreach (DataGridViewColumn column in dataGridViewPartyReport.Columns)
            {
                if (column.Name == "RowNumber") continue;

                // Color code based on column content
                if (column.Name.Contains("Credit") || column.Name.Contains("Amount") && !column.Name.Contains("Debit"))
                {
                    column.DefaultCellStyle.ForeColor = Color.DarkRed;
                    column.DefaultCellStyle.Format = "N2";
                    column.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
                }
                else if (column.Name.Contains("Debit") || column.Name.Contains("Payment"))
                {
                    column.DefaultCellStyle.ForeColor = Color.DarkGreen;
                    column.DefaultCellStyle.Format = "N2";
                    column.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
                }
                else if (column.Name.Contains("Balance") || column.Name.Contains("RunningBalance"))
                {
                    column.DefaultCellStyle.ForeColor = Color.DarkBlue;
                    column.DefaultCellStyle.Format = "N2";
                    column.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
                }
                else if (column.Name.Contains("Date"))
                {
                    column.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                }
                else
                {
                    column.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
                }

                // Format numeric columns
                if (column.ValueType == typeof(decimal) || column.Name.Contains("Amount") ||
                    column.Name.Contains("Credit") || column.Name.Contains("Debit") ||
                    column.Name.Contains("Balance"))
                {
                    column.DefaultCellStyle.Format = "N2";
                    column.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
                }
            }

            // Populate row numbers
            for (int i = 0; i < dataGridViewPartyReport.Rows.Count; i++)
            {
                if (!dataGridViewPartyReport.Rows[i].IsNewRow)
                {
                    dataGridViewPartyReport.Rows[i].Cells["RowNumber"].Value = (i + 1).ToString();
                }
            }

            dataGridViewPartyReport.AlternatingRowsDefaultCellStyle.BackColor = Color.LightCyan;
        }

        private void CalculatePartySummary()
        {
            try
            {
                decimal totalCredit = 0;
                decimal totalDebit = 0;
                decimal totalPayments = 0;
                int activeParties = 0;
                int totalTransactions = 0;
                decimal highestBalance = 0;
                int creditParties = 0;
                int debitParties = 0;

                foreach (DataGridViewRow row in dataGridViewPartyReport.Rows)
                {
                    if (!row.IsNewRow)
                    {
                        string reportType = comboReportType.SelectedItem.ToString();

                        switch (reportType)
                        {
                            case "Party Balance Summary":
                                totalCredit += GetCellDecimalValue(row, "TotalCredit");
                                totalDebit += GetCellDecimalValue(row, "TotalDebit");
                                totalPayments += GetCellDecimalValue(row, "TotalPayments");
                                activeParties++;
                                totalTransactions += GetCellIntValue(row, "PaymentCount");

                                decimal balance = GetCellDecimalValue(row, "NetBalance");
                                if (Math.Abs(balance) > Math.Abs(highestBalance))
                                    highestBalance = balance;

                                if (balance > 0) creditParties++;
                                if (balance < 0) debitParties++;
                                break;

                            case "Party Transaction Details":
                                totalCredit += GetCellDecimalValue(row, "Amount");
                                totalDebit += GetCellDecimalValue(row, "Debit");
                                totalTransactions++;
                                break;

                            case "Party Ledger Statement":
                                totalCredit += GetCellDecimalValue(row, "Credit");
                                totalDebit += GetCellDecimalValue(row, "Debit");
                                totalTransactions++;
                                break;

                            case "Credit/Debit Analysis":
                                totalCredit += GetCellDecimalValue(row, "TotalCredit");
                                totalDebit += GetCellDecimalValue(row, "TotalDebit");
                                totalPayments += GetCellDecimalValue(row, "TotalPayments");
                                activeParties++;
                                break;

                            case "Payment History":
                                totalPayments += GetCellDecimalValue(row, "Amount");
                                totalTransactions++;
                                break;
                        }
                    }
                }

                decimal netBalance = totalCredit - totalDebit - totalPayments;

                // Update main summary
                lblTotalCredit.Text = $"Total Credit: ₹{totalCredit:N2}";
                lblTotalDebit.Text = $"Total Debit: ₹{totalDebit:N2}";
                lblTotalPayments.Text = $"Total Payments: ₹{totalPayments:N2}";
                lblNetBalance.Text = $"Net Balance: ₹{netBalance:N2}";

                // Update secondary summary
                lblActiveParties.Text = $"Active Parties: {activeParties}";
                lblTotalTransactions.Text = $"Total Transactions: {totalTransactions}";
                lblHighestBalance.Text = $"Highest Balance: ₹{highestBalance:N2}";
                lblAvgTransaction.Text = $"Avg Transaction: ₹{(totalTransactions > 0 ? (totalCredit + totalDebit + totalPayments) / totalTransactions : 0):N2}";

                // Update additional metrics
                var lblCreditParties = panelSummary.Controls.OfType<Label>().FirstOrDefault(l => l.Text.StartsWith("Parties with Credit"));
                var lblDebitParties = panelSummary.Controls.OfType<Label>().FirstOrDefault(l => l.Text.StartsWith("Parties with Debit"));

                if (lblCreditParties != null) lblCreditParties.Text = $"Parties with Credit: {creditParties}";
                if (lblDebitParties != null) lblDebitParties.Text = $"Parties with Debit: {debitParties}";

                // Color code net balance
                lblNetBalance.ForeColor = netBalance >= 0 ? Color.DarkGreen : Color.DarkRed;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error calculating party summary: {ex.Message}", "Error",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private decimal GetCellDecimalValue(DataGridViewRow row, string columnName)
        {
            if (row.Cells[columnName]?.Value != null &&
                decimal.TryParse(row.Cells[columnName].Value.ToString(), out decimal value))
            {
                return value;
            }
            return 0;
        }

        private int GetCellIntValue(DataGridViewRow row, string columnName)
        {
            if (row.Cells[columnName]?.Value != null &&
                int.TryParse(row.Cells[columnName].Value.ToString(), out int value))
            {
                return value;
            }
            return 0;
        }

        private void DataGridViewPartyReport_RowPostPaint(object sender, DataGridViewRowPostPaintEventArgs e)
        {
            // Optional: Additional row formatting
        }

        private void btnExport_Click(object sender, EventArgs e)
        {
            // Export functionality (same as previous forms)
            MessageBox.Show("Export functionality would be implemented here", "Export",
                          MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void btnPrint_Click(object sender, EventArgs e)
        {
            // Print functionality (same as previous forms)
            MessageBox.Show("Print functionality would be implemented here", "Print",
                          MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
}