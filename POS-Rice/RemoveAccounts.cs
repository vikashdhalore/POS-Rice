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

    public partial class RemoveAccounts : Form
    {
        private string connectionString = @"Server=.\SQLEXPRESS;Database=Rice;Integrated Security=true;";
        public RemoveAccounts()
        {
            InitializeComponent();
        }
        
        private void AddCheckBoxColumn()
        {
            // Remove existing checkbox column to avoid duplicates
            if (dgvAccounts.Columns.Contains("CheckBoxColumn"))
            {
                dgvAccounts.Columns.Remove("CheckBoxColumn");
            }

            // Create checkbox column
            DataGridViewCheckBoxColumn checkBoxColumn = new DataGridViewCheckBoxColumn();
            checkBoxColumn.HeaderText = "Select";
            checkBoxColumn.Name = "CheckBoxColumn";
            checkBoxColumn.Width = 50;
            checkBoxColumn.ReadOnly = false; // Allow checking/unchecking

            // Add checkbox column as first column
            dgvAccounts.Columns.Insert(0, checkBoxColumn);
        }
        private void LoadData(string search = "")
        {
            string connectionString = @"Server=.\SQLEXPRESS;Database=Rice;Integrated Security=true;";
            SqlConnection conn = new SqlConnection(connectionString);

            try
            {

                conn.Open();


                // SQL select query
                string query = "SELECT ActID, Act_Name FROM Accounts WHERE Act_Name LIKE @search";

                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@search", "%" + search + "%");

                // Create connection and data adapter               
                SqlDataAdapter dataAdapter = new SqlDataAdapter(cmd);


                // Create DataTable to hold the data
                DataTable dataTable = new DataTable();

                // Fill DataTable with data from database
                dataAdapter.Fill(dataTable);

                // Set DataGridView data source
                dgvAccounts.DataSource = dataTable;

                // Optional: Configure column headers
                dgvAccounts.Columns["ActID"].HeaderText = "Account ID";
                dgvAccounts.Columns["Act_Name"].HeaderText = "Account Name";
                //dgvAccounts.Columns["Brand"].HeaderText = "Brand";

                // Optional: Auto-size columns
                dgvAccounts.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

                // Add checkbox column after data binding
                //AddCheckBoxColumn();

            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading data: {ex.Message}", "Database Error",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

        }
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
        private void RefreshData()
        {

            try
            {
                if (dgvAccounts.Columns.Contains("CheckBoxColumn"))
                {
                    dgvAccounts.Columns.Remove("CheckBoxColumn");
                }

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
                AddCheckBoxColumn();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading data: {ex.Message}", "Error",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void RemoveAccounts_Load(object sender, EventArgs e)
        {
            RefreshData();
        }

        private void dataGridViewRemoveAct_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void btnRemoveAct_Click(object sender, EventArgs e)
        {
            try
            {
                // Get selected rows
                var selectedRows = dgvAccounts.Rows
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
                    string connectionString = @"Server=.\SQLEXPRESS;Database=Rice;Integrated Security=true;";

                    using (SqlConnection connection = new SqlConnection(connectionString))
                    {
                        connection.Open();

                        foreach (DataGridViewRow row in selectedRows)
                        {
                            int actId = Convert.ToInt32(row.Cells["ActID"].Value);

                            string query = "DELETE FROM Accounts WHERE ActID = @ActID";

                            using (SqlCommand command = new SqlCommand(query, connection))
                            {
                                command.Parameters.AddWithValue("@ActID", actId);
                                command.ExecuteNonQuery();
                            }
                        }

                        MessageBox.Show($"{selectedRows.Count} product(s) deleted successfully!", "Success",
                                      MessageBoxButtons.OK, MessageBoxIcon.Information);

                        // Refresh DataGridView
                        RefreshData();

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

            foreach (DataGridViewRow row in dgvAccounts.Rows)
            {
                row.Cells["CheckBoxColumn"].Value = isChecked;
            }
        }

        private void txtSearchRemovAct_TextChanged(object sender, EventArgs e)
        {
            LoadData(txtSearchRemovAct.Text.Trim());
        }
    }
    
}
