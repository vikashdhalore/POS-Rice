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
    public partial class RemoveCustomer : Form
    {
        public RemoveCustomer()
        {
            InitializeComponent();
        }

        private void SearchCustomerData(string search = "")
        {

            string connectionString = @"Server=.\SQLEXPRESS;Database=Rice;Integrated Security=true;";
            SqlConnection conn = new SqlConnection(connectionString);

            try
            {

                conn.Open();


                // SQL select query
                string query = "SELECT CustID, Customer_Name, PhoneNo FROM CustomerAct WHERE Customer_Name LIKE @search OR PhoneNo LIKE @search";

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
                dataGridView1.Columns["CustID"].HeaderText = "Customer ID";
                dataGridView1.Columns["Customer_Name"].HeaderText = "Customer Name";
                dataGridView1.Columns["PhoneNo"].HeaderText = "Phone No";

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


            /*
            string connectionString = @"Server=.\SQLEXPRESS;Database=Rice;Integrated Security=true;";
            SqlConnection connection = new SqlConnection(connectionString);
            try
            {
                // SQL select query
                string query = "SELECT CustID, Customer_Name, PhoneNo FROM CustomerAct WHERE Customer_Name LIKE @search AND PhoneNo LIKE @search";
                connection.Open();

                if (dataGridView1.Columns.Contains("CheckBoxColumn"))
                {
                    dataGridView1.Columns.Remove("CheckBoxColumn");
                }

                // Create connection string - update with your database details
                

                

                // Create connection and data adapter


                    SqlCommand cmd = new SqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@search", "%" + search + "%");

                SqlDataAdapter dataAdapter = new SqlDataAdapter(cmd);
                {
                    
                    // Create DataTable to hold the data
                    DataTable dataTable = new DataTable();

                    // Fill DataTable with data from database
                    dataAdapter.Fill(dataTable);

                    // Set DataGridView data source
                    dataGridView1.DataSource = dataTable;

                    // Optional: Configure column headers
                    dataGridView1.Columns["CustID"].HeaderText = "Customer ID";
                    dataGridView1.Columns["Customer_Name"].HeaderText = "Customer Name";
                    dataGridView1.Columns["PhoneNo"].HeaderText = "Phone No";

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
            */




        }

        private void LoadCustomerData()
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
                string query = "SELECT CustID, Customer_Name, PhoneNo FROM CustomerAct";

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
                    dataGridView1.Columns["CustID"].HeaderText = "Customer ID";
                    dataGridView1.Columns["Customer_Name"].HeaderText = "Customer Name";
                    dataGridView1.Columns["PhoneNo"].HeaderText = "Phone No";

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

        private void RemoveCustomer_Load(object sender, EventArgs e)
        {
            LoadCustomerData();
        }

        private void button4_Click(object sender, EventArgs e)
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
                    MessageBox.Show("Please select at least one customer to delete", "Selection Required",
                                  MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Confirm deletion
                DialogResult result = MessageBox.Show($"Are you sure you want to delete {selectedRows.Count} customer(s)?",
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
                            int customerId = Convert.ToInt32(row.Cells["CustID"].Value);

                            string query = "DELETE FROM CustomerAct WHERE CustID = @CustID";

                            using (SqlCommand command = new SqlCommand(query, connection))
                            {
                                command.Parameters.AddWithValue("@CustID", customerId);
                                command.ExecuteNonQuery();
                            }
                        }

                        MessageBox.Show($"{selectedRows.Count} customer(s) deleted successfully!", "Success",
                                      MessageBoxButtons.OK, MessageBoxIcon.Information);

                        // Refresh DataGridView
                        LoadCustomerData();

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

        private void texSearch_TextChanged(object sender, EventArgs e)
        {
            SearchCustomerData(texSearch.Text.Trim());
        }
    }
}
