using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace POS_Rice
{
    public partial class Expense : Form
    {
        int selectedActID = 0;
        string selectedActName = "";

        private string connectionString = @"Server=.\SQLEXPRESS;Database=Rice;Integrated Security=true;";
     

        public Expense()
        {
            InitializeComponent();
        }


        private void GenerateAutoID()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    string query = "SELECT ISNULL(MAX(ActID), 0) + 1 FROM Accounts";
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        conn.Open();
                        int nextID = Convert.ToInt32(cmd.ExecuteScalar());
                        txtActID.Text = nextID.ToString();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error generating ID: {ex.Message}", "Error",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void GenerateAutoLocalExpID()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    string query = "SELECT ISNULL(MAX(Local_Exp_ID), 0) + 1 FROM LocalExpense";
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        conn.Open();
                        int nextID = Convert.ToInt32(cmd.ExecuteScalar());
                        textEXID.Text = nextID.ToString();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error generating ID: {ex.Message}", "Error",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ClearFields()
        {
            txtActName.Clear();
            txtActName.Focus();
            btnActUpdate.Enabled = false;
            btnActAdd.Enabled = true;
        }

        // Refresh Data from Database
        private void RefreshData()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    string query = "SELECT ActID, Act_Name FROM Accounts ORDER BY ActID";
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        conn.Open();
                        DataTable dt = new DataTable();
                        using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                        {
                            da.Fill(dt);
                            dgvAccounts.DataSource = dt;
                        }
                    }
                }
                FormatDataGridView();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading data: {ex.Message}", "Error",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public void RefreshComboAccount()
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

                            // Clear and reload combobox
                            comboAccount.DataSource = null;
                            comboAccount.Items.Clear();

                            // Add default item
                            comboAccount.Items.Add("-- Select Account --");

                            // Add accounts
                            foreach (DataRow row in dt.Rows)
                            {
                                comboAccount.Items.Add(new KeyValuePair<int, string>(
                                    Convert.ToInt32(row["ActID"]),
                                    row["Act_Name"].ToString()
                                ));
                            }

                            // Set display and value members
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

        // Search Accounts
        private void SearchAccounts(string searchText)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    string query = @"SELECT ActID, Act_Name FROM Accounts 
                                   WHERE Act_Name LIKE @SearchText 
                                   OR CAST(ActID AS NVARCHAR(10)) LIKE @SearchText
                                   ORDER BY ActID";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@SearchText", $"%{searchText}%");

                        conn.Open();
                        DataTable dt = new DataTable();
                        using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                        {
                            da.Fill(dt);
                            dgvAccounts.DataSource = dt;
                        }
                    }
                }
                FormatDataGridView();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error searching: {ex.Message}", "Error",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Format DataGridView
        private void FormatDataGridView()
        {
            if (dgvAccounts.Columns.Count > 0)
            {
                dgvAccounts.Columns["ActID"].HeaderText = "Account ID";
                //dgvAccounts.Columns["ActID"].Width = 80;
                dgvAccounts.Columns["Act_Name"].HeaderText = "Account Name";
                //dgvAccounts.Columns["Act_Name"].Width = 200;

                //dgvAccounts.RowHeadersVisible = false;
                //dgvAccounts.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
                //dgvAccounts.ReadOnly = true;
            }
        }
        private void Expense_Load(object sender, EventArgs e)
        {

            //account data on load

            RefreshData();
            ClearFields();
            GenerateAutoID();

            // Local Expense data on load
            RefreshExpData();
            //ClearExpFields();
            GenerateAutoLocalExpID();
            RefreshComboAccount();

            // Party Entry
            GenerateAutoPaymentID();
            RefreshComboParty();
            RefreshPartyPayData();

            // Customer Payment
            LoadCustomers();
             LoadCashBookData();
            //ClearForm();
            AutoGenerateCashID();
        }

        private void btnActAdd_Click(object sender, EventArgs e)
        {
            if (comboCustPay.SelectedIndex == 0) 
            {
                MessageBox.Show("Please select status from (Credit / Debit)!", "Validation Error",
                              MessageBoxButtons.OK, MessageBoxIcon.Warning);
                comboCustPay.Focus();
                return;
            }

            if (string.IsNullOrEmpty(txtActName.Text.Trim()))
            {
                MessageBox.Show("Please enter Account Name!", "Validation Error",
                              MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtActName.Focus();
                return;
            }

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    string query = "INSERT INTO Accounts (ActID,Act_Name) VALUES (@ActID,@Act_Name)";
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@ActID", txtActID.Text.Trim());
                        cmd.Parameters.AddWithValue("@Act_Name", txtActName.Text.Trim());

                        conn.Open();
                        int result = cmd.ExecuteNonQuery();

                        if (result > 0)
                        {
                            MessageBox.Show("Account added successfully!", "Success",
                                          MessageBoxButtons.OK, MessageBoxIcon.Information);
                            ClearFields();
                            RefreshData();
                            GenerateAutoID();
                        }
                    }
                }
            }
            catch (SqlException ex)
            {
                if (ex.Number == 2627) // Unique constraint violation
                {
                    MessageBox.Show("Account name already exists!", "Duplicate Error",
                                  MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else
                {
                    MessageBox.Show($"Database error: {ex.Message}", "Error",
                                  MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "Error",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnActUpdate_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtActID.Text) || txtActID.Text == "0")
            {
                MessageBox.Show("Please select an account to update!", "Validation Error",
                              MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (string.IsNullOrEmpty(txtActName.Text.Trim()))
            {
                MessageBox.Show("Please enter Account Name!", "Validation Error",
                              MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtActName.Focus();
                return;
            }

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    string query = "UPDATE Accounts SET Act_Name = @Act_Name WHERE ActID = @ActID";
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@Act_Name", txtActName.Text.Trim());
                        cmd.Parameters.AddWithValue("@ActID", Convert.ToInt32(txtActID.Text));

                        conn.Open();
                        int result = cmd.ExecuteNonQuery();

                        if (result > 0)
                        {
                            MessageBox.Show("Account updated successfully!", "Success",
                                          MessageBoxButtons.OK, MessageBoxIcon.Information);
                            ClearFields();
                            RefreshData();
                            GenerateAutoID();
                        }
                        else
                        {
                            MessageBox.Show("Account not found!", "Error",
                                          MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
            }
            catch (SqlException ex)
            {
                if (ex.Number == 2627)
                {
                    MessageBox.Show("Account name already exists!", "Duplicate Error",
                                  MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else
                {
                    MessageBox.Show($"Database error: {ex.Message}", "Error",
                                  MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "Error",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnActRefresh_Click(object sender, EventArgs e)
        {
            RefreshData();
            ClearFields();
            GenerateAutoID();
        }

        private void textActSearch_TextChanged(object sender, EventArgs e)
        {
            SearchAccounts(txtSearch.Text.Trim());
        }

        private void dataGridView2_CellContentDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && dgvAccounts.Rows[e.RowIndex].Cells["ActID"].Value != null)
            {
                DataGridViewRow row = dgvAccounts.Rows[e.RowIndex];
                txtActID.Text = row.Cells["ActID"].Value.ToString();
                txtActName.Text = row.Cells["Act_Name"].Value.ToString();

                btnActUpdate.Enabled = true;
                btnActAdd.Enabled = false;
            }
        }

        private void tabControl1_Enter(object sender, EventArgs e)
        {
            RefreshComboAccount();
            GenerateAutoLocalExpID();
        }

        private void tabControl1_Click(object sender, EventArgs e)
        {
            RefreshComboAccount();
            GenerateAutoLocalExpID();
        }

        private void comboAccount_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboAccount.SelectedItem != null)
            {
                if (comboAccount.SelectedItem is KeyValuePair<int, string> selectedItem)
                {
                    int selectedActID = selectedItem.Key;
                    string selectedActName = selectedItem.Value;

                    // Use the values
                    if (selectedActID != 0) // Not the default item
                    {
                        // Your code here
                    }
                }
            }
        }

        private void RefreshExpData()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    string query = @"SELECT Local_Exp_ID, ActID, ActName, Local_Exp_Date, Local_Exp_Amount 
                               FROM LocalExpense 
                               ORDER BY Local_Exp_Date DESC, Local_Exp_ID DESC";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        conn.Open();
                        DataTable dt = new DataTable();
                        using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                        {
                            da.Fill(dt);
                            dataGridViewExpenses.DataSource = dt;
                        }
                    }
                }
                FormatDataExpGridView();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading data: {ex.Message}", "Error",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private bool ValidateExpInputs()
        {
            // Check account selection
            if (comboAccount.SelectedIndex <= 0)
            {
                MessageBox.Show("Please select an account!", "Validation Error",
                              MessageBoxButtons.OK, MessageBoxIcon.Warning);
                comboAccount.Focus();
                return false;
            }

            // Check amount
            if (string.IsNullOrEmpty(textAmount.Text.Trim()))
            {
                MessageBox.Show("Please enter amount!", "Validation Error",
                              MessageBoxButtons.OK, MessageBoxIcon.Warning);
                textAmount.Focus();
                return false;
            }

            // Validate amount is numeric
            if (!decimal.TryParse(textAmount.Text.Trim(), out decimal amount) || amount <= 0)
            {
                MessageBox.Show("Please enter a valid positive amount!", "Validation Error",
                              MessageBoxButtons.OK, MessageBoxIcon.Warning);
                textAmount.Focus();
                textAmount.SelectAll();
                return false;
            }

            return true;
        }

        private void ClearExpFields()
        {
            textAmount.Clear();
            dateTimeExpDate.Value = DateTime.Now;
            comboAccount.SelectedIndex = 0;
            textAmount.Focus();
        }

        private void FormatDataExpGridView()
        {
            if (dataGridViewExpenses.Columns.Count > 0)
            {
                dataGridViewExpenses.Columns["Local_Exp_ID"].HeaderText = "Expense ID";
                //dataGridViewExpenses.Columns["Local_Exp_ID"].Width = 80;

                dataGridViewExpenses.Columns["ActID"].HeaderText = "Account ID";
                //dataGridViewExpenses.Columns["ActID"].Width = 80;

                dataGridViewExpenses.Columns["ActName"].HeaderText = "Account Name";
                //dataGridViewExpenses.Columns["ActName"].Width = 150;

                dataGridViewExpenses.Columns["Local_Exp_Date"].HeaderText = "Date";
                //dataGridViewExpenses.Columns["Local_Exp_Date"].Width = 100;
                dataGridViewExpenses.Columns["Local_Exp_Date"].DefaultCellStyle.Format = "dd/MM/yyyy";

                dataGridViewExpenses.Columns["Local_Exp_Amount"].HeaderText = "Amount";
                //dataGridViewExpenses.Columns["Local_Exp_Amount"].Width = 100;
                dataGridViewExpenses.Columns["Local_Exp_Amount"].DefaultCellStyle.Format = "N2";

                //dataGridViewExpenses.RowHeadersVisible = false;
                //dataGridViewExpenses.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
                //dataGridViewExpenses.ReadOnly = true;
            }
        }
        private void button12_Click(object sender, EventArgs e)
        {
            if (!ValidateExpInputs())
                return;

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    string query = @"INSERT INTO LocalExpense (Local_Exp_ID,ActID, ActName, Local_Exp_Date, Local_Exp_Amount) 
                               VALUES (@Local_Exp_ID,@ActID, @ActName, @ExpDate, @Amount)";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        // Get selected account details
                        if (comboAccount.SelectedItem is KeyValuePair<int, string> selectedAccount)
                        {
                            cmd.Parameters.AddWithValue("@Local_Exp_ID", textEXID.Text.Trim());
                            cmd.Parameters.AddWithValue("@ActID", selectedAccount.Key);
                            cmd.Parameters.AddWithValue("@ActName", selectedAccount.Value);
                            cmd.Parameters.AddWithValue("@ExpDate", dateTimeExpDate.Value.Date);
                            cmd.Parameters.AddWithValue("@Amount", Convert.ToDecimal(textAmount.Text.Trim()));

                            conn.Open();
                            int result = cmd.ExecuteNonQuery();

                            if (result > 0)
                            {
                                MessageBox.Show("Expense added successfully!", "Success",
                                              MessageBoxButtons.OK, MessageBoxIcon.Information);
                                ClearExpFields();
                                RefreshExpData();
                                GenerateAutoLocalExpID();
                            }
                        }
                        else
                        {
                            MessageBox.Show("Please select a valid account!", "Validation Error",
                                          MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        }
                    }
                }
            }
            catch (FormatException)
            {
                MessageBox.Show("Please enter a valid amount!", "Validation Error",
                              MessageBoxButtons.OK, MessageBoxIcon.Warning);
                textAmount.Focus();
            }
            catch (SqlException ex)
            {
                MessageBox.Show($"Database error: {ex.Message}", "Error",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "Error",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void SetComboBoxAccount(string actName)
        {
            for (int i = 0; i < comboAccount.Items.Count; i++)
            {
                if (comboAccount.Items[i] is KeyValuePair<int, string> item)
                {
                    if (item.Value == actName)
                    {
                        comboAccount.SelectedIndex = i;
                        return;
                    }
                }
            }
            comboAccount.SelectedIndex = 0;
        }

        private void dataGridViewExpenses_CellContentDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && dataGridViewExpenses.Rows[e.RowIndex].Cells["Local_Exp_ID"].Value != null)
            {
                DataGridViewRow row = dataGridViewExpenses.Rows[e.RowIndex];

                textEXID.Text = row.Cells["Local_Exp_ID"].Value.ToString();

                // Set account in combobox
                string actName = row.Cells["ActName"].Value.ToString();
                SetComboBoxAccount(actName);

                // Set date
                if (DateTime.TryParse(row.Cells["Local_Exp_Date"].Value.ToString(), out DateTime expDate))
                {
                    dateTimeExpDate.Value = expDate;
                }

                // Set amount
                textAmount.Text = row.Cells["Local_Exp_Amount"].Value.ToString();
            }
        }

        private void button9_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(textEXID.Text) || textEXID.Text == "0")
            {
                MessageBox.Show("Please select an expense to update!", "Validation Error",
                              MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!ValidateExpInputs())
                return;

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    string query = @"UPDATE LocalExpense 
                               SET ActID = @ActID, 
                                   ActName = @ActName, 
                                   Local_Exp_Date = @ExpDate, 
                                   Local_Exp_Amount = @Amount 
                               WHERE Local_Exp_ID = @ExpID";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        if (comboAccount.SelectedItem is KeyValuePair<int, string> selectedAccount)
                        {
                            cmd.Parameters.AddWithValue("@ExpID", Convert.ToInt32(textEXID.Text));
                            cmd.Parameters.AddWithValue("@ActID", selectedAccount.Key);
                            cmd.Parameters.AddWithValue("@ActName", selectedAccount.Value);
                            cmd.Parameters.AddWithValue("@ExpDate", dateTimeExpDate.Value.Date);
                            cmd.Parameters.AddWithValue("@Amount", Convert.ToDecimal(textAmount.Text.Trim()));

                            conn.Open();
                            int result = cmd.ExecuteNonQuery();

                            if (result > 0)
                            {
                                MessageBox.Show("Expense updated successfully!", "Success",
                                              MessageBoxButtons.OK, MessageBoxIcon.Information);
                                ClearExpFields();
                                RefreshExpData();
                                GenerateAutoLocalExpID();
                            }
                            else
                            {
                                MessageBox.Show("Expense not found!", "Error",
                                              MessageBoxButtons.OK, MessageBoxIcon.Error);
                            }
                        }
                        else
                        {
                            MessageBox.Show("Please select a valid account!", "Validation Error",
                                          MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        }
                    }
                }
            }
            catch (FormatException)
            {
                MessageBox.Show("Please enter a valid amount!", "Validation Error",
                              MessageBoxButtons.OK, MessageBoxIcon.Warning);
                textAmount.Focus();
            }
            catch (SqlException ex)
            {
                MessageBox.Show($"Database error: {ex.Message}", "Error",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "Error",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void button11_Click(object sender, EventArgs e)
        {
            RefreshExpData();
            ClearExpFields();
            GenerateAutoLocalExpID();
            RefreshComboAccount();
        }

        private void ClearPartyPayFields()
        {
            textPaymentID.Clear();
            textAmountEntry.Clear();
            textPartyBalance.Clear();
            dateTimeEntryDate.Value = DateTime.Now;
            comboPartyPayStatus.SelectedIndex = 0;
            textPartyPayDes.Clear();
            comboParty.SelectedIndex = 0;
            textAmountEntry.Focus();
            button1.Enabled = true;
            button3.Enabled = false;
        }
        private void RefreshComboParty() 
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

                            // Clear and reload combobox
                            comboParty.DataSource = null;
                            comboParty.Items.Clear();

                            // Add default item
                            comboParty.Items.Add("-- Select Account --");

                            // Add accounts
                            foreach (DataRow row in dt.Rows)
                            {
                                comboParty.Items.Add(new KeyValuePair<int, string>(
                                    Convert.ToInt32(row["PartyID"]),
                                    row["PartyName"].ToString()
                                ));
                            }

                            // Set display and value members
                            comboParty.DisplayMember = "Value";
                            comboParty.ValueMember = "Key";
                            comboParty.SelectedIndex = 0;
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
        private void FormatDataPartyPayGridView()
        {
            if (dataGridViewPartyPay.Columns.Count > 0)
            {
                dataGridViewPartyPay.Columns["P_PaymentID"].HeaderText = "Payment ID";
                //dataGridViewPartyPay.Columns["PaymentID"].Width = 80;

                dataGridViewPartyPay.Columns["PartyID"].HeaderText = "Party ID";
                //dataGridViewPartyPay.Columns["PartyID"].Width = 70;

                dataGridViewPartyPay.Columns["PartyName"].HeaderText = "Party Name";
                //dataGridViewPartyPay.Columns["PartyName"].Width = 150;

                dataGridViewPartyPay.Columns["Pay_Date"].HeaderText = "Entry Date";
                //dataGridViewPartyPay.Columns["Pay_Date"].Width = 100;
                dataGridViewPartyPay.Columns["Pay_Date"].DefaultCellStyle.Format = "dd/MM/yyyy";

                dataGridViewPartyPay.Columns["Entry_Amount"].HeaderText = "Amount";
                //dataGridViewPartyPay.Columns["Entry_Amount"].Width = 100;
                dataGridViewPartyPay.Columns["Entry_Amount"].DefaultCellStyle.Format = "N2";

                dataGridViewPartyPay.Columns["AmountDue"].HeaderText = "Balance";
                //dataGridViewPartyPay.Columns["AmountDue"].Width = 100;
                dataGridViewPartyPay.Columns["AmountDue"].DefaultCellStyle.Format = "N2";

                if (dataGridViewPartyPay.Columns.Contains("CreatedDate"))
                {
                    dataGridViewPartyPay.Columns["CreatedDate"].Visible = false;
                }

                dataGridViewPartyPay.RowHeadersVisible = false;
                dataGridViewPartyPay.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
                dataGridViewPartyPay.ReadOnly = true;
            }
        }
        private bool ValidatePartyPayInputs()
        {
            // Check party selection
            if (comboParty.SelectedIndex <= 0)
            {
                MessageBox.Show("Please select a party!", "Validation Error",
                              MessageBoxButtons.OK, MessageBoxIcon.Warning);
                comboParty.Focus();
                return false;
            }

            // Check amount
            if (string.IsNullOrEmpty(textAmountEntry.Text.Trim()))
            {
                MessageBox.Show("Please enter amount!", "Validation Error",
                              MessageBoxButtons.OK, MessageBoxIcon.Warning);
                textAmountEntry.Focus();
                return false;
            }

            // Validate amount is numeric and positive
            if (!decimal.TryParse(textAmountEntry.Text.Trim(), out decimal amount) || amount <= 0)
            {
                MessageBox.Show("Please enter a valid positive amount!", "Validation Error",
                              MessageBoxButtons.OK, MessageBoxIcon.Warning);
                textAmountEntry.Focus();
                textAmountEntry.SelectAll();
                return false;
            }

            return true;
        }
        private void RefreshPartyPayData()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    string query = @"SELECT P_PaymentID, PartyID, PartyName, Pay_Date, Entry_Amount, AmountDue, Status, Description
                               FROM PartyPayment 
                               ORDER BY Pay_Date DESC, P_PaymentID DESC";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        conn.Open();
                        DataTable dt = new DataTable();
                        using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                        {
                            da.Fill(dt);
                            dataGridViewPartyPay.DataSource = dt;
                        }
                    }
                }
                FormatDataPartyPayGridView();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading data: {ex.Message}", "Error",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private decimal GetAmountDue(int partyId)
        {
            /*
            try
            {
                string query = @"
                                 SELECT 
                                    COALESCE(pi.DueAmount, 0) 
                                                             +
                                                              COALESCE(
                                                                       SUM(
                                                                            CASE
                                                                                WHEN pp.Status ='Debit (Naame)')
                                                                                 THEN pp.Entry_Amount
                                                                                    WHEN pp.Status = 'Credit'
                                                                                    THEN -pp.Paid
                                                                                            ELSE 0
                                                                                                END
                                                                                                        ), 0
                                                                                                            ) AS AmountDue
                                                                                                FROM ProInventory pi 
                                                                                                LEFT JOIN PartyPayment pp ON pi.PartyID = pp.PartyID
                                                                                                WHERE pi.PartyID = @PartyID
                                                                                                Group BY pi.DueAmount";

                using (SqlConnection conn = new SqlConnection(connectionString))
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@PartyID", partyId);
                    conn.Open();

                    object result = cmd.ExecuteScalar();
                    return result != null ? Convert.ToDecimal(result) : 0;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error calculating amount due: {ex.Message}", "Error",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
                return 0;
            }
            */
            
            try
            {
                string query = @"
            SELECT 
                COALESCE(pi.DueAmount, 0) - COALESCE(pp.Paid, 0) AS AmountDue
            FROM 
                (SELECT SUM(Entry_Amount) AS Paid FROM partyPayment WHERE PartyID = @PartyID) AS pp
            CROSS JOIN 
                (SELECT SUM(credit) AS DueAmount FROM ProInventory WHERE partyId = @PartyID) AS pi";

                using (SqlConnection conn = new SqlConnection(connectionString))
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@PartyID", partyId);
                    conn.Open();

                    object result = cmd.ExecuteScalar();
                    return result != null ? Convert.ToDecimal(result) : 0;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error calculating amount due: {ex.Message}", "Error",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
                return 0;
            }
            
        }
        decimal amountDue = 0;
        private void comboParty_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboParty.SelectedIndex > 0 && comboParty.SelectedItem is KeyValuePair<int, string> selectedParty)
            {
                int partyId = selectedParty.Key;
                string partyName = selectedParty.Value;

                // Display Amount Due
                amountDue = GetAmountDue(partyId);
                textPartyBalance.Text = amountDue.ToString("N2");

                // Optional: Change color based on amount due
                if (amountDue > 0)
                {
                    textPartyBalance.ForeColor = Color.Red; // Party owes money
                    textPartyBalance.Text = amountDue.ToString("N2") + " (Due)";
                }
                else if (amountDue < 0)
                {
                    textPartyBalance.ForeColor = Color.Green; // Party has credit
                    textPartyBalance.Text = Math.Abs(amountDue).ToString("N2") + " (Advance)";
                }
                else
                {
                    textPartyBalance.ForeColor = Color.Black; // No balance
                    textPartyBalance.Text = "0.00 (Settled)";
                }
            }
            else
            {
                textPartyBalance.Clear();
                textPartyBalance.ForeColor = Color.Black;
            }
        }

        private void GenerateAutoPaymentID()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    string query = "SELECT ISNULL(MAX(P_PaymentID), 0) + 1 FROM PartyPayment";
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        conn.Open();
                        int nextID = Convert.ToInt32(cmd.ExecuteScalar());
                        textPaymentID.Text = nextID.ToString();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error generating Payment ID: {ex.Message}", "Error",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (!ValidatePartyPayInputs())
                return;

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    string query = @"INSERT INTO PartyPayment (P_PaymentID,PartyID, PartyName,AmountDue, Entry_Amount, Pay_Date, Status, Description) 
                               VALUES (@P_PaymentID,@PartyID, @PartyName,@AmountDue,@Entry_Amount,@Pay_Date,@Status, @Description)";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        string PartyPayStatus = comboPartyPayStatus.SelectedItem.ToString();
                        if (comboParty.SelectedItem is KeyValuePair<int, string> selectedParty)
                        {
                            int partyId = selectedParty.Key;
                            string partyName = selectedParty.Value;
                            decimal entryAmount = Convert.ToDecimal(textAmountEntry.Text.Trim());
                            //decimal currentBalance = CalculatePartyBalance(partyId);
                            //decimal newBalance = currentBalance - entryAmount;
                            cmd.Parameters.AddWithValue("@P_PaymentID", textPaymentID.Text.Trim());
                            cmd.Parameters.AddWithValue("@PartyID", partyId);
                            cmd.Parameters.AddWithValue("@PartyName", partyName);
                            cmd.Parameters.AddWithValue("@AmountDue", amountDue);
                            cmd.Parameters.AddWithValue("@Entry_Amount", entryAmount);
                            cmd.Parameters.AddWithValue("@Pay_Date", dateTimeEntryDate.Value.Date);
                            cmd.Parameters.AddWithValue("@Status", PartyPayStatus);
                            cmd.Parameters.AddWithValue("@Description", textPartyPayDes.Text);
                            //cmd.Parameters.AddWithValue("@Balance", newBalance);

                            conn.Open();
                            int result = cmd.ExecuteNonQuery();

                            if (result > 0)
                            {
                                MessageBox.Show("Payment added successfully!", "Success",
                                              MessageBoxButtons.OK, MessageBoxIcon.Information);
                                ClearPartyPayFields();
                                RefreshPartyPayData();
                                GenerateAutoPaymentID();
                            }
                        }
                    }
                }
            }
            catch (FormatException)
            {
                MessageBox.Show("Please enter a valid amount!", "Validation Error",
                              MessageBoxButtons.OK, MessageBoxIcon.Warning);
                textAmountEntry.Focus();
            }
            catch (SqlException ex)
            {
                MessageBox.Show($"Database error: {ex.Message}", "Error",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "Error",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void dataGridViewPartyPay_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {



        }

        private void button2_Click(object sender, EventArgs e)
        {
            ClearPartyPayFields();
            RefreshPartyPayData();
            RefreshComboParty();
            GenerateAutoPaymentID();
        }


        private void SetComboBoxSelection(ComboBox comboBox, int id, string name)
        {
            // Clear current selection
            comboBox.SelectedItem = null;

            // Search through combo box items
            for (int i = 0; i < comboBox.Items.Count; i++)
            {
                if (comboBox.Items[i] is KeyValuePair<int, string> item)
                {
                    if (item.Key == id && item.Value == name)
                    {
                        comboBox.SelectedIndex = i;
                        return;
                    }
                }
            }

            // If not found, you could add it or show a message
            MessageBox.Show("Party not found in the list!", "Warning",
                          MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        private void dataGridViewPartyPay_CellContentDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                // Check if the click is on a valid row (not header)
                if (e.RowIndex >= 0)
                {
                    // Get the selected row
                    DataGridViewRow row = dataGridViewPartyPay.Rows[e.RowIndex];

                    // Load Payment ID
                    if (row.Cells["P_PaymentID"].Value != null)
                    {
                        textPaymentID.Text = row.Cells["P_PaymentID"].Value.ToString();
                    }

                    // Load Party ID and Name - Assuming you have both ID and Name in grid
                    // Method 1: If you have PartyID in a hidden column
                    if (row.Cells["PartyID"].Value != null && row.Cells["PartyName"].Value != null)
                    {
                        int partyId = Convert.ToInt32(row.Cells["PartyID"].Value);
                        string partyName = row.Cells["PartyName"].Value.ToString();

                        // Set the combo box selection
                        SetComboBoxSelection(comboParty, partyId, partyName);
                    }

                    // Load Party Balance (AmountDue)
                    if (row.Cells["AmountDue"].Value != null)
                    {
                        textPartyBalance.Text = row.Cells["AmountDue"].Value.ToString();
                    }

                    // Load Entry Date (Pay_Date)
                    if (row.Cells["Pay_Date"].Value != null && DateTime.TryParse(row.Cells["Pay_Date"].Value.ToString(), out DateTime payDate))
                    {
                        dateTimeEntryDate.Value = payDate;
                    }
                    else
                    {
                        dateTimeEntryDate.Value = DateTime.Today;
                    }

                    // Load Entry Amount
                    if (row.Cells["Entry_Amount"].Value != null)
                    {
                        textAmountEntry.Text = row.Cells["Entry_Amount"].Value.ToString();
                    }

                    if (row.Cells["Status"].Value != null)
                    {
                        string value = row.Cells["Status"].Value.ToString();

                        if (value == "Credit") 
                        {
                            comboPartyPayStatus.SelectedItem = "Credit";
                        }
                        else 
                        {
                            comboPartyPayStatus.SelectedItem = "Debit";
                        }

                    }
                    else 
                    {
                        comboPartyPayStatus.SelectedIndex = 0;
                    }

                    if (row.Cells["Description"].Value != null)
                    {
                        textPartyPayDes.Text = row.Cells["Description"].Value.ToString();
                    }
                            
                    // Optional: Change button text to "Update" if you have a save/update button
                    // btnSave.Text = "Update";
                    // isUpdateMode = true;
                }
                button1.Enabled = false;
                button3.Enabled = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading data: {ex.Message}", "Error",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void label4_Click(object sender, EventArgs e)
        {

        }

        private void ClearForm()
        {
            textCustID.Text = "0";
            textCashEntry.Text = "0";
            //textCustID.Clear();
            txtSearch.Clear();
            textCustPayDes.Clear();
            dataGridViewCashBook.ClearSelection();
            comboCustPay.SelectedIndex = 0;
            textCashEntry.Focus();
            button6.Enabled = false;
            button7.Enabled = true;
        }
        private void SearchOrders(string searchTerm)
        {
            if (string.IsNullOrEmpty(searchTerm))
            {
                listViewResults.Items.Clear();
                return;
            }

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    // AGGREGATED VERSION: If you want totals per customer
                    string query = @"
                    SELECT 
                             C.CustID,
                             C.Customer_Name
                            -- ISNULL(SUM(O.Credit), 0) as TotalCredit,
                            -- ISNULL(SUM(O.Debit), 0) as TotalDebit, 
                            -- ISNULL(SUM(O.Cash), 0) as TotalCash, 
                            -- ISNULL(SUM(O.Balance), 0) as TotalBalance
                             FROM CustomerAct C
                             LEFT JOIN Orders O ON C.CustID = O.CustID 
                             WHERE C.Customer_Name LIKE @SearchTerm
                             GROUP BY C.CustID, C.Customer_Name
                             ORDER BY C.Customer_Name";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@SearchTerm", "%" + searchTerm + "%");

                        conn.Open();
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            listViewResults.Items.Clear();

                            while (reader.Read())
                            {
                                ListViewItem item = new ListViewItem(reader["CustID"].ToString());
                                item.SubItems.Add(reader["Customer_Name"].ToString());

                                // Use the aggregated column names
                                //decimal credit = reader["TotalCredit"] != DBNull.Value ? Convert.ToDecimal(reader["TotalCredit"]) : 0;
                                //decimal debit = reader["TotalDebit"] != DBNull.Value ? Convert.ToDecimal(reader["TotalDebit"]) : 0;
                                //decimal cash = reader["TotalCash"] != DBNull.Value ? Convert.ToDecimal(reader["TotalCash"]) : 0;
                                //decimal balance = reader["TotalBalance"] != DBNull.Value ? Convert.ToDecimal(reader["TotalBalance"]) : 0;

                                //item.SubItems.Add(credit.ToString("N2"));
                                //item.SubItems.Add(debit.ToString("N2"));
                                //item.SubItems.Add(cash.ToString("N2"));
                                //item.SubItems.Add(balance.ToString("N2"));

                                listViewResults.Items.Add(item);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error searching orders: {ex.Message}", "Search Error",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadOrderDetails(string orderID)
        {
            // Implement your order loading logic here
            MessageBox.Show($"Loading Order: {orderID}", "Order Selected",
                          MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        private void AutoGenerateCashID()
        {
            string connectionString = @"Server=.\SQLEXPRESS;Database=Rice;Integrated Security=true;";
            SqlConnection conn = new SqlConnection(connectionString);
            try
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand("SELECT ISNULL(MAX(CashID), 0) + 1 FROM CashBook", conn);
                int nextID = Convert.ToInt32(cmd.ExecuteScalar());
                lblCashID.Text = nextID.ToString();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error generating Cash ID: " + ex.Message);
            }
            finally
            {
                conn.Close();
            }
        }
        private void LoadCashBookData()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    string query = @"SELECT 
                            cb.CashID,
                            cb.Cash_Entry,
                            cb.Date,
                            ca.CustID,
                            ca.Customer_Name,
                            cb.Status,
                            cb.Description
                        FROM CashBook cb
                        INNER join CustomerAct ca on cb.CustID = ca.CustID
                        ORDER BY CashID DESC";

                    using (SqlDataAdapter da = new SqlDataAdapter(query, conn))
                    {
                        DataTable dt = new DataTable();
                        da.Fill(dt);
                        dataGridViewCashBook.DataSource = dt;

                        // Format columns
                        dataGridViewCashBook.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
                        //dataGridViewCashBook.Columns["CashID"].HeaderText = "Cash No";
                        dataGridViewCashBook.Columns["Cash_Entry"].HeaderText = "Cash Amount";
                        dataGridViewCashBook.Columns["Date"].HeaderText = "Cash Date";
                        dataGridViewCashBook.Columns["CustID"].HeaderText = "Customer No";
                        dataGridViewCashBook.Columns["Customer_Name"].HeaderText = "Customer Name";
                        dataGridViewCashBook.Columns["Status"].HeaderText = "Status";
                        dataGridViewCashBook.Columns["Description"].HeaderText = "Description";

                        // Format numeric column
                        if (dataGridViewCashBook.Columns["Cash_Entry"] != null)
                            dataGridViewCashBook.Columns["Cash_Entry"].DefaultCellStyle.Format = "N2";
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading cash book data: {ex.Message}");
            }
        }

        private void button7_Click(object sender, EventArgs e)
        {
            // Validate inputs
            if (string.IsNullOrEmpty(textCustID.Text) || textCustID.Text == "0")
            {
                MessageBox.Show("Please enter a valid Customer ID!");
                return;
            }

            if (string.IsNullOrEmpty(textCashEntry.Text) || !decimal.TryParse(textCashEntry.Text, out _))
            {
                MessageBox.Show("Please enter a valid cash amount!");
                return;
            }

            try
            {
                string CustStatus = comboCustPay.SelectedItem.ToString();
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    string query = @"INSERT INTO CashBook 
                   (CashID, Cash_Entry, CustID, Date, Status, Description) 
                   VALUES 
                   (@CashID, @Cash_Entry, @CustID, @Date, @Status, @Description)";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@CashID", Convert.ToInt32(lblCashID.Text));
                        cmd.Parameters.AddWithValue("@Cash_Entry", Convert.ToDecimal(textCashEntry.Text));
                        cmd.Parameters.AddWithValue("@CustID", Convert.ToInt32(textCustID.Text));
                        cmd.Parameters.AddWithValue("@Date", dateTimeDate.Value);
                        cmd.Parameters.AddWithValue("@Status", CustStatus);
                        cmd.Parameters.AddWithValue("@Description", textCustPayDes.Text.Trim());

                        conn.Open();
                        int result = cmd.ExecuteNonQuery();

                        if (result > 0)
                        {
                            MessageBox.Show("Cash book record added successfully!");

                            // Refresh data and clear form
                            ClearForm();
                            LoadCashBookData();
                            AutoGenerateCashID();
                        }
                        else
                        {
                            MessageBox.Show("Failed to add cash book record!");
                        }
                    }
                }
                
            }

            catch (Exception ex)
            {
                MessageBox.Show("Error adding cash book record: " + ex.Message);
            }
        }

        private void LoadCustomers() 
        {
            /*
            if (string.IsNullOrEmpty(searchTerm))
            {
                listViewResults.Items.Clear();
                return;
            }
            */

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    string query = @"select CustID, Customer_Name, PhoneNo from CustomerAct";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        //cmd.Parameters.AddWithValue("@SearchTerm", "%" + searchTerm + "%");

                        conn.Open();
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            listViewResults.Items.Clear();

                            while (reader.Read())
                            {
                                ListViewItem item = new ListViewItem(reader["CustID"].ToString());
                                // item.SubItems.Add(Convert.ToInt32(reader["CustID"]).ToString("N2"));
                                item.SubItems.Add(reader["Customer_Name"].ToString());
                                //item.SubItems.Add(Convert.ToDateTime(reader["OrderDate"]).ToString("dd/MM/yyyy"));
                                item.SubItems.Add(reader["PhoneNo"].ToString());

                                listViewResults.Items.Add(item);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error searching orders: {ex.Message}", "Search Error",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        

        private void SearchCustomer(string searchTerm)
        {
            if (string.IsNullOrEmpty(searchTerm))
            {
                listViewResults.Items.Clear();
                return;
            }

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    string query = @"select CustID, Customer_Name, PhoneNo from CustomerAct Where Customer_Name LIKE @SearchTerm or PhoneNo LIKE @SearchTerm";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@SearchTerm", "%" + searchTerm + "%");

                        conn.Open();
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            listViewResults.Items.Clear();

                            while (reader.Read())
                            {
                                ListViewItem item = new ListViewItem(reader["CustID"].ToString());
                                // item.SubItems.Add(Convert.ToInt32(reader["CustID"]).ToString("N2"));
                                item.SubItems.Add(reader["Customer_Name"].ToString());
                                //item.SubItems.Add(Convert.ToDateTime(reader["OrderDate"]).ToString("dd/MM/yyyy"));
                                item.SubItems.Add(reader["PhoneNo"].ToString());

                                listViewResults.Items.Add(item);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error searching orders: {ex.Message}", "Search Error",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void textSearchCust_TextChanged(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(textSearchCust.Text)) 
            {
                LoadCustomers();
            }
            else 
            {
                SearchOrders(textSearchCust.Text.Trim());
            }
                
        }

        private void button6_Click(object sender, EventArgs e)
        {

            // Validate inputs
            if (string.IsNullOrEmpty(lblCashID.Text) || lblCashID.Text == "0")
            {
                MessageBox.Show("Please select a record to update!");
                return;
            }

            if (string.IsNullOrEmpty(textCustID.Text) || textCustID.Text == "0")
            {
                MessageBox.Show("Please enter a valid Customer ID!");
                return;
            }

            if (string.IsNullOrEmpty(textCashEntry.Text) || !decimal.TryParse(textCashEntry.Text, out _))
            {
                MessageBox.Show("Please enter a valid cash amount!");
                return;
            }

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    string value = comboCustPay.SelectedItem.ToString();
                    string query = @"UPDATE CashBook 
                   SET 
                    Cash_Entry = @Cash_Entry,
                    Status = @Status,
                    Description = @Description,
                    Date = @Date,
                    CustID = @CustID
                   WHERE CashID = @CashID";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@CashID", Convert.ToInt32(lblCashID.Text));
                        cmd.Parameters.AddWithValue("@Cash_Entry", Convert.ToDecimal(textCashEntry.Text));
                        cmd.Parameters.AddWithValue("@CustID", Convert.ToInt32(textCustID.Text));
                        cmd.Parameters.AddWithValue("@Status", value);
                        cmd.Parameters.AddWithValue("@Description", textCustPayDes.Text.Trim());
                        cmd.Parameters.AddWithValue("@Date", dateTimeDate.Value);
                        conn.Open();
                        int result = cmd.ExecuteNonQuery();

                        if (result > 0)
                        {
                            MessageBox.Show("Cash book record updated successfully!");

                            // Refresh data and clear form
                            ClearForm();
                            LoadCashBookData();
                            AutoGenerateCashID();
                        }
                        else
                        {
                            MessageBox.Show("Failed to update cash book record! Record not found.");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error updating cash book record: " + ex.Message);
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            ClearForm();
            LoadCustomers();
            LoadCashBookData();
            AutoGenerateCashID();
        }

        private void listViewResults_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listViewResults.SelectedItems.Count > 0)
            {
                ListViewItem selectedItem = listViewResults.SelectedItems[0];
                string custID = selectedItem.SubItems[0].Text;
                //string customerName = selectedItem.SubItems[1].Text;
                //string amount = selectedItem.SubItems[3].Text;

                // You can use these values to load the full order details
                textCustID.Text = custID;
                //textCashEntry.Text = "0";

                //LoadOrderDetails(orderID);
            }
        }

        private string GetSafeString(object value)
        {
            return value?.ToString() ?? "";
        }
        private void dataGridViewCashBook_CellContentDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;
            try
            {
                DataGridViewRow row = dataGridViewCashBook.Rows[e.RowIndex];

                // CashID (Label)
                lblCashID.Text = GetSafeString(row.Cells["CashID"].Value);

                // Customer ID
                textCustID.Text = GetSafeString(row.Cells["CustID"].Value);

                textCustPayDes.Text = GetSafeString(row.Cells["Description"].Value);
                // Cash Entry (format as currency if needed)
                object cashValue = row.Cells["Cash_Entry"].Value;
                if (cashValue != null)
                {
                    if (decimal.TryParse(cashValue.ToString(), out decimal cashAmount))
                    {
                        textCashEntry.Text = cashAmount.ToString("N2"); // Format with 2 decimal places
                    }
                    else
                    {
                        textCashEntry.Text = cashValue.ToString();
                    }
                }
                else
                {
                    textCashEntry.Text = "";
                }

                // Date
                object dateValue = row.Cells["Date"].Value;
                if (dateValue != null)
                {
                    if (dateValue is DateTime dt)
                    {
                        dateTimeDate.Value = dt;
                    }
                    else if (DateTime.TryParse(dateValue.ToString(), out DateTime parsedDate))
                    {
                        dateTimeDate.Value = parsedDate;
                    }
                    else
                    {
                        dateTimeDate.Value = DateTime.Today;
                    }
                }
                else
                {
                    dateTimeDate.Value = DateTime.Today;
                }
                
                if (row.Cells["Status"].Value != null)                
                {
                    string value = row.Cells["Status"].Value.ToString();

                    if (value == "Credit")
                    {
                        comboCustPay.SelectedItem = "Credit";
                    }
                    else
                    {
                        comboCustPay.SelectedItem = "Debit";
                    }
                }
                else 
                {
                    comboCustPay.SelectedIndex = 0;
                }

                button6.Enabled = true;
                button7.Enabled = false;
                // Optional: Focus on first editable field
                // textCustID.Focus();

            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading record: {ex.Message}", "Load Error",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnActRemove_Click(object sender, EventArgs e)
        {
            RemoveAccounts remvAct = new RemoveAccounts();
            remvAct.FormClosed += RemoveAccounts_FormClosed;
            remvAct.Show();
        }

        private void RemoveAccounts_FormClosed(object sender, FormClosedEventArgs e)
        {
            RefreshData();
            ClearFields();
            GenerateAutoID();
        }

        private void button10_Click(object sender, EventArgs e)
        {
            RemoveExp remExp = new RemoveExp();
            remExp.FormClosed += RemoveExp_FormClosed;
            remExp.Show();
        }

        private void RemoveExp_FormClosed(object sender, FormClosedEventArgs e)
        {
            // Refresh data when remove form is closed
            RefreshExpData();
            //ClearExpFields();
            GenerateAutoLocalExpID();
            RefreshComboAccount();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (!ValidatePartyPayInputs())
                return;

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    string query = @"UPDATE PartyPayment 
                         SET PartyID = @PartyID, 
                             PartyName = @PartyName, 
                             Entry_Amount = @Entry_Amount, 
                             Pay_Date = @Pay_Date,
                             Status = @Status,
                             Description = @Description
                         WHERE P_PaymentID = @P_PaymentID";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        if (comboParty.SelectedItem is KeyValuePair<int, string> selectedParty)
                        {
                            int partyId = selectedParty.Key;
                            string partyName = selectedParty.Value;
                            string status = comboPartyPayStatus.SelectedItem.ToString();
                            decimal entryAmount = Convert.ToDecimal(textAmountEntry.Text.Trim());

                            cmd.Parameters.AddWithValue("@P_PaymentID", textPaymentID.Text.Trim());
                            cmd.Parameters.AddWithValue("@PartyID", partyId);
                            cmd.Parameters.AddWithValue("@PartyName", partyName);
                            cmd.Parameters.AddWithValue("@Entry_Amount", entryAmount);
                            cmd.Parameters.AddWithValue("@Description", textPartyPayDes.Text.Trim());
                            cmd.Parameters.AddWithValue("@Pay_Date", dateTimeEntryDate.Value.Date);
                            cmd.Parameters.AddWithValue("@Status",status);
                            conn.Open();
                            int result = cmd.ExecuteNonQuery();

                            if (result > 0)
                            {
                                MessageBox.Show("Payment updated successfully!", "Success",
                                              MessageBoxButtons.OK, MessageBoxIcon.Information);
                                ClearPartyPayFields();
                                RefreshPartyPayData();
                                GenerateAutoPaymentID(); // Note: Consider if you need this for update
                            }
                            else
                            {
                                MessageBox.Show("No payment record found with the given ID!", "Not Found",
                                              MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            }
                        }
                        else
                        {
                            MessageBox.Show("Please select a valid party!", "Validation Error",
                                          MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        }
                    }
                }
            }
            catch (FormatException)
            {
                MessageBox.Show("Please enter a valid amount!", "Validation Error",
                              MessageBoxButtons.OK, MessageBoxIcon.Warning);
                textAmountEntry.Focus();
            }
            catch (SqlException ex)
            {
                MessageBox.Show($"Database error: {ex.Message}", "Error",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "Error",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadExpData(string search = "")
        {
            //string connectionString = @"Server=.\SQLEXPRESS;Database=Rice;Integrated Security=true;";
            SqlConnection conn = new SqlConnection(connectionString);

            try
            {

                conn.Open();


                // SQL select query
                string query = "SELECT Local_Exp_ID, ActID, ActName, Local_Exp_Date, Local_Exp_Amount FROM LocalExpense WHERE ActName LIKE @search";

                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@search", "%" + search + "%");

                // Create connection and data adapter               
                SqlDataAdapter dataAdapter = new SqlDataAdapter(cmd);


                // Create DataTable to hold the data
                DataTable dataTable = new DataTable();

                // Fill DataTable with data from database
                dataAdapter.Fill(dataTable);

                // Set DataGridView data source
                dataGridViewExpenses.DataSource = dataTable;

                // Optional: Configure column headers
                dataGridViewExpenses.Columns["Local_Exp_ID"].HeaderText = "Expense ID";
                dataGridViewExpenses.Columns["ActID"].HeaderText = "Account ID";
                dataGridViewExpenses.Columns["ActName"].HeaderText = "Account Name";
                dataGridViewExpenses.Columns["Local_Exp_Date"].HeaderText = "Date";
                dataGridViewExpenses.Columns["Local_Exp_ID"].HeaderText = "Expense Amount";

                // Optional: Auto-size columns
                dataGridViewExpenses.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

                // Add checkbox column after data binding
                //AddCheckBoxColumn();

            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading data: {ex.Message}", "Database Error",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

        }
        private void textExpSearch_TextChanged(object sender, EventArgs e)
        {
            LoadExpData(textExpSearch.Text.Trim());
        }

        private void LoadPartyPayData(string search = "")
        {
            //string connectionString = @"Server=.\SQLEXPRESS;Database=Rice;Integrated Security=true;";
            SqlConnection conn = new SqlConnection(connectionString);

            try
            {

                conn.Open();


                // SQL select query
                string query = "SELECT P_PaymentID, PartyID, PartyName, AmountDue, Entry_Amount, Pay_Date FROM PartyPayment WHERE PartyName LIKE @search";

                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@search", "%" + search + "%");

                // Create connection and data adapter               
                SqlDataAdapter dataAdapter = new SqlDataAdapter(cmd);


                // Create DataTable to hold the data
                DataTable dataTable = new DataTable();

                // Fill DataTable with data from database
                dataAdapter.Fill(dataTable);

                // Set DataGridView data source
                dataGridViewPartyPay.DataSource = dataTable;

                // Optional: Configure column headers
                dataGridViewPartyPay.Columns["P_PaymentID"].HeaderText = "Payment ID";
                dataGridViewPartyPay.Columns["PartyID"].HeaderText = "Party ID";
                dataGridViewPartyPay.Columns["PartyName"].HeaderText = "Party Name";
                dataGridViewPartyPay.Columns["AmountDue"].HeaderText = "Balance";
                dataGridViewPartyPay.Columns["Entry_Amount"].HeaderText = "Entry Amount";
                dataGridViewPartyPay.Columns["Pay_Date"].HeaderText = "Pay Date";

                // Optional: Auto-size columns
                dataGridViewPartyPay.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

                // Add checkbox column after data binding
                //AddCheckBoxColumn();

            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading data: {ex.Message}", "Database Error",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

        }
        private void textSearchP_Payment_TextChanged(object sender, EventArgs e)
        {
            LoadPartyPayData(textSearchP_Payment.Text.Trim());
        }

        private void button8_Click(object sender, EventArgs e)
        {
            RemovePayment remPay = new RemovePayment();
            remPay.FormClosed += RemovePayment_FormClosed;
            remPay.Show();
        }
        private void RemovePayment_FormClosed(object sender, FormClosedEventArgs e)
        {
            // Refresh data when remove form is closed
            GenerateAutoPaymentID();
            RefreshComboParty();
            RefreshPartyPayData();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            RemoveCustCash remCustCash = new RemoveCustCash();
            remCustCash.FormClosed += RemoveCustCash_FormClosed;
            remCustCash.Show();
        }
        private void RemoveCustCash_FormClosed(object sender, FormClosedEventArgs e)
        {
            // Refresh data when remove form is closed
            LoadCashBookData();
            //ClearForm();
            AutoGenerateCashID();
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void dataGridViewCashBook_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }
    }
}
