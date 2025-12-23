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
    public partial class RemoveExp : Form
    {
        private string connectionString = @"Server=.\SQLEXPRESS;Database=Rice;Integrated Security=true;";
        public RemoveExp()
        {
            InitializeComponent();
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

        private void AddCheckBoxColumn()
        {
            // Remove existing checkbox column to avoid duplicates
            if (dataGridViewExpenses.Columns.Contains("CheckBoxColumn"))
            {
                dataGridViewExpenses.Columns.Remove("CheckBoxColumn");
            }

            // Create checkbox column
            DataGridViewCheckBoxColumn checkBoxColumn = new DataGridViewCheckBoxColumn();
            checkBoxColumn.HeaderText = "Select";
            checkBoxColumn.Name = "CheckBoxColumn";
            checkBoxColumn.Width = 50;
            checkBoxColumn.ReadOnly = false; // Allow checking/unchecking

            // Add checkbox column as first column
            dataGridViewExpenses.Columns.Insert(0, checkBoxColumn);
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
        private void RemoveExp_Load(object sender, EventArgs e)
        {
            // Configure DataGridView properties
            dataGridViewExpenses.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dataGridViewExpenses.MultiSelect = false; // Single row selection
            dataGridViewExpenses.ReadOnly = false; // Make data cells read-only
            dataGridViewExpenses.RowHeadersVisible = true; // Show row headers for better selection

            // Make only checkbox column editable
            if (dataGridViewExpenses.Columns.Contains("CheckBoxColumn"))
            {
                dataGridViewExpenses.Columns["CheckBoxColumn"].ReadOnly = false;
            }
            AddCheckBoxColumn();
            RefreshExpData();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                // Get selected rows
                var selectedRows = dataGridViewExpenses.Rows
                    .Cast<DataGridViewRow>()
                    .Where(row => Convert.ToBoolean(row.Cells["CheckBoxColumn"].Value) == true)
                    .ToList();

                if (selectedRows.Count == 0)
                {
                    MessageBox.Show("Please select at least one product to delete", "Selection Required",
                                  MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Confirm deletion
                DialogResult result = MessageBox.Show($"Are you sure you want to delete {selectedRows.Count} product(s)?",
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
                            int localExpID = Convert.ToInt32(row.Cells["Local_Exp_ID"].Value);

                            string query = "DELETE FROM LocalExpense WHERE Local_Exp_ID = @Local_Exp_ID";

                            using (SqlCommand command = new SqlCommand(query, connection))
                            {
                                command.Parameters.AddWithValue("@Local_Exp_ID", localExpID);
                                command.ExecuteNonQuery();
                            }
                        }

                        MessageBox.Show($"{selectedRows.Count} product(s) deleted successfully!", "Success",
                                      MessageBoxButtons.OK, MessageBoxIcon.Information);

                        // Refresh DataGridView
                        RefreshExpData();

                        // Clear form fields

                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error deleting products: {ex.Message}", "Database Error",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void chkSelectAll_CheckedChanged(object sender, EventArgs e)
        {
            bool isChecked = chkSelectAll.Checked;

            foreach (DataGridViewRow row in dataGridViewExpenses.Rows)
            {
                row.Cells["CheckBoxColumn"].Value = isChecked;
            }
        }

        private void textSearch_TextChanged(object sender, EventArgs e)
        {
            LoadExpData(textExpSearch.Text.Trim());
        }
    }
}
