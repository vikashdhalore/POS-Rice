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
    public partial class CustomerInfo : Form
    {
        int orderID = 0;
        public CustomerInfo()
        {
            InitializeComponent();
        }

        private void ClearForm()
        {
            //textCustID.Clear();
            textCustName.Clear();
            textPhone.Clear();
            dataGridView1.ClearSelection();
        }

        private void AutoGenerateCustID()
        {
            string connectionString = @"Server=.\SQLEXPRESS;Database=Rice;Integrated Security=true;";
            SqlConnection conn = new SqlConnection(connectionString);
            try
            {

                conn.Open();
                SqlCommand cmd = new SqlCommand("Select ISNULL(MAX(CustID), 0) + 1 from CustomerAct", conn);
                int nextID = Convert.ToInt32(cmd.ExecuteScalar());
                textCustID.Text = nextID.ToString();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error generating Customer ID: " + ex.Message);
            }
            finally
            {
                conn.Close();
                textCustName.Focus();
            }

        }

        private void LoadCustData(string search = "")
        {
            string connectionString = @"Server=.\SQLEXPRESS;Database=Rice;Integrated Security=true;";
            SqlConnection conn = new SqlConnection(connectionString);

            try
            {

                conn.Open();


                // SQL select query
                string query = "SELECT CustID, Customer_Name, PhoneNo FROM CustomerAct WHERE Customer_Name LIKE @search Or PhoneNo LIKE @search";

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

        }
        
        private void LoadCustDataGridView()
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
                    dataGridView1.Columns["CustID"].HeaderText = "Cust ID";
                    dataGridView1.Columns["Customer_Name"].HeaderText = "Cust Name";
                    dataGridView1.Columns["PhoneNo"].HeaderText = "Phone No";

                    // Optional: Auto-size columns
                    dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

                    // Add checkbox column after data binding
                    //AddCheckBoxColumn();
                    ClearForm();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading data: {ex.Message}", "Database Error",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void CustomerInfo_Load(object sender, EventArgs e)
        {
            AutoGenerateCustID();
            LoadCustDataGridView();
        }

        private void textSearch_TextChanged(object sender, EventArgs e)
        {
            LoadCustData(textSearch.Text);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            
            LoadCustDataGridView();
            ClearForm();
            AutoGenerateCustID();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                // Validate form fields
                if (string.IsNullOrWhiteSpace(textCustName.Text) || string.IsNullOrWhiteSpace(textPhone.Text) || string.IsNullOrWhiteSpace(textCustID.Text))
                {
                    MessageBox.Show("Please fill in all fields", "Validation Error",
                                  MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Create connection string - update with your database details
                string connectionString = @"Server=.\SQLEXPRESS;Database=Rice;Integrated Security=true;";

                // SQL insert query
                string query = @"INSERT INTO CustomerAct (CustID, Customer_Name, PhoneNo) 
                 SELECT @CustID, @Customer_Name, @PhoneNo
                 WHERE NOT EXISTS (SELECT 1 FROM CustomerAct WHERE Customer_Name = @Customer_Name)";

                // Create and open connection
                using (SqlConnection connection = new SqlConnection(connectionString))
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    // Add parameters to prevent SQL injection
                    command.Parameters.AddWithValue("@CustID", textCustID.Text.Trim());
                    command.Parameters.AddWithValue("@Customer_Name", textCustName.Text.Trim());
                    command.Parameters.AddWithValue("@PhoneNo", textPhone.Text.Trim());

                    // Open connection and execute query
                    connection.Open();
                    int rowsAffected = command.ExecuteNonQuery();

                    // Check if insert was successful
                    if (rowsAffected > 0)
                    {
                        MessageBox.Show("Customer added successfully!", "Success",
                                      MessageBoxButtons.OK, MessageBoxIcon.Information);

                        // Clear form fields after successful insert
                        ClearForm();
                        LoadCustDataGridView();
                        AutoGenerateCustID();
                        // Set focus back to first field
                    }
                    else
                    {
                        MessageBox.Show("Failed to add Customer", "Error",
                                      MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "Database Error",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            try
            {
                // Check if a row is selected in DataGridView
                if (dataGridView1.CurrentRow == null)
                {
                    MessageBox.Show("Please select a Customer to update", "Selection Required",
                                  MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Get the selected product ID from DataGridView
                int custID = Convert.ToInt32(dataGridView1.CurrentRow.Cells["CustID"].Value);

                // Validate form fields
                if (string.IsNullOrWhiteSpace(textCustName.Text) || string.IsNullOrWhiteSpace(textPhone.Text))
                {
                    MessageBox.Show("Please fill in all fields", "Validation Error",
                                  MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Create connection string
                string connectionString = @"Server=.\SQLEXPRESS;Database=Rice;Integrated Security=true;";

                // SQL update query
                string query = "UPDATE CustomerAct SET Customer_Name = @Customer_Name, PhoneNo = @PhoneNo WHERE CustID = @CustID";

                using (SqlConnection connection = new SqlConnection(connectionString))
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    // Add parameters
                    command.Parameters.AddWithValue("@Customer_Name", textCustName.Text.Trim());
                    command.Parameters.AddWithValue("@PhoneNo", textPhone.Text.Trim());
                    command.Parameters.AddWithValue("@CustID", custID);

                    // Open connection and execute query
                    connection.Open();
                    int rowsAffected = command.ExecuteNonQuery();

                    if (rowsAffected > 0)
                    {
                        MessageBox.Show("Customer updated successfully!", "Success",
                                      MessageBoxButtons.OK, MessageBoxIcon.Information);

                        // Refresh DataGridView to show updated data

                        ClearForm();
                        LoadCustData();

                        // Clear form fields
                      
                        AutoGenerateCustID();
                    }
                    else
                    {
                        MessageBox.Show("Failed to update customer", "Error",
                                      MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error updating customer: {ex.Message}", "Database Error",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void dataGridView1_SelectionChanged(object sender, EventArgs e)
        {
            
        }

        private void dataGridView1_CellContentDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                // Check if a row is selected and has data
                if (dataGridView1.CurrentRow != null && dataGridView1.CurrentRow.Cells["CustID"].Value != null)
                {
                    DataGridViewRow row = dataGridView1.CurrentRow;

                    // Populate form fields with selected row data
                    textCustID.Text = row.Cells["CustID"].Value.ToString();
                    textCustName.Text = row.Cells["Customer_Name"].Value?.ToString() ?? "";
                    textPhone.Text = row.Cells["PhoneNo"].Value?.ToString() ?? "";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error selecting Party: {ex.Message}", "Error",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

        }

        private void button4_Click(object sender, EventArgs e)
        {
            RemoveCustomer removeCust = new RemoveCustomer();
            removeCust.FormClosed += RemoveCustomer_FormClosed;
            removeCust.Show();
        }
        private void RemoveCustomer_FormClosed(object sender, FormClosedEventArgs e)
        {
            ClearForm();
            LoadCustData();

            // Clear form fields

            AutoGenerateCustID();
        }
    }
}
