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
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace POS_Rice
{
    public partial class deleteProduct : Form
    {
        public deleteProduct()
        {
            InitializeComponent();
        }

        private void deleteProduct_Load(object sender, EventArgs e)
        {
            // Configure DataGridView properties
            dataGridView1.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dataGridView1.MultiSelect = false; // Single row selection
            dataGridView1.ReadOnly = false; // Make data cells read-only
            dataGridView1.RowHeadersVisible = true; // Show row headers for better selection

            // Make only checkbox column editable
            if (dataGridView1.Columns.Contains("CheckBoxColumn"))
            {
                dataGridView1.Columns["CheckBoxColumn"].ReadOnly = false;
            }

            AddCheckBoxColumn();
            LoadProductsData();
        }

        private void ClearForm()
        {
           

            // Clear all checkboxes
            foreach (DataGridViewRow row in dataGridView1.Rows)
            {
                row.Cells["CheckBoxColumn"].Value = false;
            }

            // Clear select all checkbox if exists
            if (chkSelectAll != null)
            {
                chkSelectAll.Checked = false;
            }

            textSearch.Focus();

        }

        private void AddCheckBoxColumn()
        {
            // Remove existing checkbox column to avoid duplicates
            if (dataGridView1.Columns.Contains("CheckBoxColumn"))
            {
                dataGridView1.Columns.Remove("CheckBoxColumn");
            }

            // Create checkbox column
            DataGridViewCheckBoxColumn checkBoxColumn = new DataGridViewCheckBoxColumn();
            checkBoxColumn.HeaderText = "Select";
            checkBoxColumn.Name = "CheckBoxColumn";
            checkBoxColumn.Width = 50;
            checkBoxColumn.ReadOnly = false; // Allow checking/unchecking

            // Add checkbox column as first column
            dataGridView1.Columns.Insert(0, checkBoxColumn);
        }

        private void LoadProductsData()
        {
            try
            {
                if (dataGridView1.Columns.Contains("CheckBoxColumn"))
                {
                    dataGridView1.Columns.Remove("CheckBoxColumn");
                }

                // Create connection string - update with your database details
                string connectionString = @"Server=.\SQLEXPRESS;Database=Rice;Integrated Security=true;";

                // SQL select query
                string query = "SELECT ProductID, ProductName, Brand FROM Product";

                // Create connection and data adapter
                using (SqlConnection connection = new SqlConnection(connectionString))
                using (SqlDataAdapter dataAdapter = new SqlDataAdapter(query, connection))
                {
                    // Create DataTable to hold the data
                    DataTable dataTable = new DataTable();

                    // Fill DataTable with data from database
                    dataAdapter.Fill(dataTable);

                    // Set DataGridView data source
                    dataGridView1.DataSource = dataTable;

                    // Optional: Configure column headers
                    dataGridView1.Columns["ProductID"].HeaderText = "Product ID";
                    dataGridView1.Columns["ProductName"].HeaderText = "Product Name";
                    dataGridView1.Columns["Brand"].HeaderText = "Brand";

                    // Optional: Auto-size columns
                    dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

                    // Add checkbox column after data binding
                    AddCheckBoxColumn();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading data: {ex.Message}", "Database Error",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                // Get selected rows
                var selectedRows = dataGridView1.Rows
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
                            int productId = Convert.ToInt32(row.Cells["ProductID"].Value);

                            string query = "DELETE FROM Product WHERE ProductID = @ProductID";

                            using (SqlCommand command = new SqlCommand(query, connection))
                            {
                                command.Parameters.AddWithValue("@ProductID", productId);
                                command.ExecuteNonQuery();
                            }
                        }

                        MessageBox.Show($"{selectedRows.Count} product(s) deleted successfully!", "Success",
                                      MessageBoxButtons.OK, MessageBoxIcon.Information);

                        // Refresh DataGridView
                        LoadProductsData();

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

        private void chkSelectAll_CheckedChanged_1(object sender, EventArgs e)
        {
            bool isChecked = chkSelectAll.Checked;

            foreach (DataGridViewRow row in dataGridView1.Rows)
            {
                row.Cells["CheckBoxColumn"].Value = isChecked;
            }
        }

        private void LoadData(string search = "")
        {
            string connectionString = @"Server=.\SQLEXPRESS;Database=Rice;Integrated Security=true;";
            SqlConnection conn = new SqlConnection(connectionString);

            try
            {

                conn.Open();


                // SQL select query
                string query = "SELECT ProductID, ProductName, Brand FROM Product WHERE ProductName LIKE @search Or Brand LIKE @search";

                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@search", "%" + search + "%");

                // Create connection and data adapter               
                SqlDataAdapter dataAdapter = new SqlDataAdapter(cmd);


                // Create DataTable to hold the data
                DataTable dataTable = new DataTable();

                // Fill DataTable with data from database
                dataAdapter.Fill(dataTable);

                // Set DataGridView data source
                dataGridView1.DataSource = dataTable;

                // Optional: Configure column headers
                dataGridView1.Columns["ProductID"].HeaderText = "Product ID";
                dataGridView1.Columns["ProductName"].HeaderText = "Product Name";
                dataGridView1.Columns["Brand"].HeaderText = "Brand";

                // Optional: Auto-size columns
                dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

                // Add checkbox column after data binding
                //AddCheckBoxColumn();

            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading data: {ex.Message}", "Database Error",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

        }

        private void txtSrchPrdct_TextChanged(object sender, EventArgs e)
        {
            LoadData(textSearch.Text.Trim());
        }
    }
}
