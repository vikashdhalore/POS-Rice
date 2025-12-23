using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace POS_Rice
{
    public partial class RemovePayment : Form
    {
        private string connectionString = @"Server=.\SQLEXPRESS;Database=Rice;Integrated Security=true;";
        public RemovePayment()
        {
            InitializeComponent();
        }

        private void AddCheckBoxColumn()
        {
            // Remove existing checkbox column to avoid duplicates
            if (dataGridViewPayment.Columns.Contains("CheckBoxColumn"))
            {
                dataGridViewPayment.Columns.Remove("CheckBoxColumn");
            }

            // Create checkbox column
            DataGridViewCheckBoxColumn checkBoxColumn = new DataGridViewCheckBoxColumn();
            checkBoxColumn.HeaderText = "Select";
            checkBoxColumn.Name = "CheckBoxColumn";
            checkBoxColumn.Width = 50;
            checkBoxColumn.ReadOnly = false; // Allow checking/unchecking

            // Add checkbox column as first column
            dataGridViewPayment.Columns.Insert(0, checkBoxColumn);
        }

        private void RefreshPartyPayData()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    string query = @"SELECT P_PaymentID, PartyID, PartyName, Pay_Date, Entry_Amount, AmountDue
                               FROM PartyPayment 
                               ORDER BY Pay_Date DESC, P_PaymentID DESC";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        conn.Open();
                        DataTable dt = new DataTable();
                        using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                        {
                            da.Fill(dt);
                            dataGridViewPayment.DataSource = dt;
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

        private void FormatDataPartyPayGridView()
        {
            if (dataGridViewPayment.Columns.Count > 0)
            {
                dataGridViewPayment.Columns["P_PaymentID"].HeaderText = "Payment ID";
                //dataGridViewPartyPay.Columns["PaymentID"].Width = 80;

                dataGridViewPayment.Columns["PartyID"].HeaderText = "Party ID";
                //dataGridViewPartyPay.Columns["PartyID"].Width = 70;

                dataGridViewPayment.Columns["PartyName"].HeaderText = "Party Name";
                //dataGridViewPartyPay.Columns["PartyName"].Width = 150;

                dataGridViewPayment.Columns["Pay_Date"].HeaderText = "Entry Date";
                //dataGridViewPartyPay.Columns["Pay_Date"].Width = 100;
                dataGridViewPayment.Columns["Pay_Date"].DefaultCellStyle.Format = "dd/MM/yyyy";

                dataGridViewPayment.Columns["Entry_Amount"].HeaderText = "Amount";
                //dataGridViewPartyPay.Columns["Entry_Amount"].Width = 100;
                dataGridViewPayment.Columns["Entry_Amount"].DefaultCellStyle.Format = "N2";

                dataGridViewPayment.Columns["AmountDue"].HeaderText = "Balance";
                //dataGridViewPartyPay.Columns["AmountDue"].Width = 100;
                dataGridViewPayment.Columns["AmountDue"].DefaultCellStyle.Format = "N2";

                if (dataGridViewPayment.Columns.Contains("CreatedDate"))
                {
                    dataGridViewPayment.Columns["CreatedDate"].Visible = false;
                }

                dataGridViewPayment.RowHeadersVisible = false;
                dataGridViewPayment.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
                dataGridViewPayment.ReadOnly = false;
            }
        }

        private void RemovePayment_Load(object sender, EventArgs e)
        {
            // Configure DataGridView properties
            dataGridViewPayment.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dataGridViewPayment.MultiSelect = false; // Single row selection
            dataGridViewPayment.ReadOnly = false; // Make data cells read-only
            dataGridViewPayment.RowHeadersVisible = false; // Show row headers for better selection

            // Make only checkbox column editable
            if (dataGridViewPayment.Columns.Contains("CheckBoxColumn"))
            {
                dataGridViewPayment.Columns["CheckBoxColumn"].ReadOnly = false;
            }
            AddCheckBoxColumn();
            RefreshPartyPayData();
        }

        private void chkSelectAll_CheckedChanged(object sender, EventArgs e)
        {
            bool isChecked = chkSelectAll.Checked;

            foreach (DataGridViewRow row in dataGridViewPayment.Rows)
            {
                row.Cells["CheckBoxColumn"].Value = isChecked;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                // Get selected rows
                var selectedRows = dataGridViewPayment.Rows
                    .Cast<DataGridViewRow>()
                    .Where(row => Convert.ToBoolean(row.Cells["CheckBoxColumn"].Value) == true)
                    .ToList();

                if (selectedRows.Count == 0)
                {
                    MessageBox.Show("Please select at least one Party payment to delete", "Selection Required",
                                  MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Confirm deletion
                DialogResult result = MessageBox.Show($"Are you sure you want to delete {selectedRows.Count} party payment(s)?",
                                                    "Confirm Delete",
                                                    MessageBoxButtons.YesNo,
                                                    MessageBoxIcon.Question);

                if (result == DialogResult.Yes)
                {
                    // Create connection string
                    //string connectionString = @"Server=.\SQLEXPRESS;Database=Rice;Integrated Security=true;";

                    using (SqlConnection connection = new SqlConnection(connectionString))
                    {
                        connection.Open();

                        foreach (DataGridViewRow row in selectedRows)
                        {
                            int partyPayment = Convert.ToInt32(row.Cells["P_PaymentID"].Value);

                            string query = "DELETE FROM PartyPayment WHERE P_PaymentID = @P_PaymentID";

                            using (SqlCommand command = new SqlCommand(query, connection))
                            {
                                command.Parameters.AddWithValue("@P_PaymentID", partyPayment);
                                command.ExecuteNonQuery();
                            }
                        }

                        MessageBox.Show($"{selectedRows.Count} Party payment deleted successfully!", "Success",
                                      MessageBoxButtons.OK, MessageBoxIcon.Information);

                        // Refresh DataGridView
                        RefreshPartyPayData();

                        // Clear form fields

                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error deleting party: {ex.Message}", "Database Error",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
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
                dataGridViewPayment.DataSource = dataTable;

                // Optional: Configure column headers
                dataGridViewPayment.Columns["P_PaymentID"].HeaderText = "Payment ID";
                dataGridViewPayment.Columns["PartyID"].HeaderText = "Party ID";
                dataGridViewPayment.Columns["PartyName"].HeaderText = "Party Name";
                dataGridViewPayment.Columns["AmountDue"].HeaderText = "Balance";
                dataGridViewPayment.Columns["Entry_Amount"].HeaderText = "Entry Amount";
                dataGridViewPayment.Columns["Pay_Date"].HeaderText = "Pay Date";

                // Optional: Auto-size columns
                dataGridViewPayment.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

                // Add checkbox column after data binding
                AddCheckBoxColumn();

            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading data: {ex.Message}", "Database Error",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

        }
        private void textExpSearch_TextChanged(object sender, EventArgs e)
        {
            LoadPartyPayData(textSearchP_Payment.Text.Trim());
        }
    }
}
