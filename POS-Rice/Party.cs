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
    public partial class Party : Form
    {
        public Party()
        {
            InitializeComponent();
        }


        private void ClearForm()
        {
            //textPartyID.Clear();
            textPartyName.Clear();
            textPhoneNo.Clear();
            textPartyName.Focus();
        }
        private void AutoGeneratePartyID()
        {
            string connectionString = @"Server=.\SQLEXPRESS;Database=Rice;Integrated Security=true;";
            SqlConnection conn = new SqlConnection(connectionString);
            try
            {

                conn.Open();
                SqlCommand cmd = new SqlCommand("Select ISNULL(MAX(PartyID), 0) + 1 from Party", conn);
                int nextID = Convert.ToInt32(cmd.ExecuteScalar());
                textPartyID.Text = nextID.ToString();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error generating Party ID: " + ex.Message);
            }
            finally
            {
                conn.Close();
                textPartyName.Focus();
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
                string query = "SELECT PartyID, PartyName, PhoneNo FROM Party WHERE PartyName LIKE @search Or PhoneNo LIKE @search";

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
                dataGridView1.Columns["PartyID"].HeaderText = "Party ID";
                dataGridView1.Columns["PartyName"].HeaderText = "Party Name";
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

        private void LoadPartyData()
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
                string query = "SELECT PartyID, PartyName, PhoneNo FROM Party";

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
                    dataGridView1.Columns["PartyID"].HeaderText = "Party ID";
                    dataGridView1.Columns["PartyName"].HeaderText = "Party Name";
                    dataGridView1.Columns["PhoneNo"].HeaderText = "Phone No";

                    // Optional: Auto-size columns
                    dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

                    // Add checkbox column after data binding
                    //AddCheckBoxColumn();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading data: {ex.Message}", "Database Error",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void textBox3_TextChanged(object sender, EventArgs e)
        {

        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            try
            {
                // Validate form fields
                if (string.IsNullOrWhiteSpace(textPartyName.Text) || string.IsNullOrWhiteSpace(textPhoneNo.Text) || string.IsNullOrWhiteSpace(textPartyID.Text))
                {
                    MessageBox.Show("Please fill in all fields", "Validation Error",
                                  MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Create connection string - update with your database details
                string connectionString = @"Server=.\SQLEXPRESS;Database=Rice;Integrated Security=true;";

                // SQL insert query
                string query = "INSERT INTO Party (PartyID, PartyName, PhoneNo) VALUES (@PartyID, @PartyName, @PhoneNo)";

                // Create and open connection
                using (SqlConnection connection = new SqlConnection(connectionString))
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    // Add parameters to prevent SQL injection
                    command.Parameters.AddWithValue("@PartyID", textPartyID.Text.Trim());
                    command.Parameters.AddWithValue("@PartyName", textPartyName.Text.Trim());
                    command.Parameters.AddWithValue("@PhoneNo", textPhoneNo.Text.Trim());

                    // Open connection and execute query
                    connection.Open();
                    int rowsAffected = command.ExecuteNonQuery();

                    // Check if insert was successful
                    if (rowsAffected > 0)
                    {
                        MessageBox.Show("Party added successfully!", "Success",
                                      MessageBoxButtons.OK, MessageBoxIcon.Information);

                        // Clear form fields after successful insert
                        ClearForm();
                        LoadPartyData();
                        AutoGeneratePartyID();
                        // Set focus back to first field
                    }
                    else
                    {
                        MessageBox.Show("Failed to add Party", "Error",
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

        private void Party_Load(object sender, EventArgs e)
        {
            LoadPartyData();
            AutoGeneratePartyID();
            ClearForm();
        }

        private void textSearch_TextChanged(object sender, EventArgs e)
        {
            LoadData(textSearch.Text.Trim());
        }

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            LoadPartyData();
            AutoGeneratePartyID();
            ClearForm();


        }

        private void dataGridView1_SelectionChanged(object sender, EventArgs e)
        {
            
        }

        private void btnUpdate_Click(object sender, EventArgs e)
        {
            try
            {
                // Check if a row is selected in DataGridView
                if (dataGridView1.CurrentRow == null)
                {
                    MessageBox.Show("Please select a Party to update", "Selection Required",
                                  MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Get the selected product ID from DataGridView
                int partyID = Convert.ToInt32(dataGridView1.CurrentRow.Cells["PartyID"].Value);

                // Validate form fields
                if (string.IsNullOrWhiteSpace(textPartyName.Text) || string.IsNullOrWhiteSpace(textPhoneNo.Text))
                {
                    MessageBox.Show("Please fill in all fields", "Validation Error",
                                  MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Create connection string
                string connectionString = @"Server=.\SQLEXPRESS;Database=Rice;Integrated Security=true;";

                // SQL update query
                string query = "UPDATE Party SET PartyName = @PartyName, PhoneNo = @PhoneNo WHERE PartyID = @PartyID";

                using (SqlConnection connection = new SqlConnection(connectionString))
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    // Add parameters
                    command.Parameters.AddWithValue("@PartyName", textPartyName.Text.Trim());
                    command.Parameters.AddWithValue("@PhoneNo", textPhoneNo.Text.Trim());
                    command.Parameters.AddWithValue("@PartyID", partyID);

                    // Open connection and execute query
                    connection.Open();
                    int rowsAffected = command.ExecuteNonQuery();

                    if (rowsAffected > 0)
                    {
                        MessageBox.Show("Party updated successfully!", "Success",
                                      MessageBoxButtons.OK, MessageBoxIcon.Information);

                        // Refresh DataGridView to show updated data
                        LoadPartyData();

                        // Clear form fields
                        ClearForm();
                        AutoGeneratePartyID();
                    }
                    else
                    {
                        MessageBox.Show("Failed to update party", "Error",
                                      MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error updating party: {ex.Message}", "Database Error",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void deleteParty_FormClosed(object sender, FormClosedEventArgs e)
        {
            // Refresh data when remove form is closed
            LoadPartyData();
            AutoGeneratePartyID();
            ClearForm();
        }



        private void btnRemove_Click(object sender, EventArgs e)
        {
            deleteParty deltParty = new deleteParty();
            deltParty.FormClosed += deleteParty_FormClosed;

            deltParty.Show();
        }

        private void dataGridView1_CellContentDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                // Check if a row is selected and has data
                if (dataGridView1.CurrentRow != null && dataGridView1.CurrentRow.Cells["PartyID"].Value != null)
                {
                    DataGridViewRow row = dataGridView1.CurrentRow;

                    // Populate form fields with selected row data
                    textPartyID.Text = row.Cells["PartyID"].Value.ToString();
                    textPartyName.Text = row.Cells["PartyName"].Value?.ToString() ?? "";
                    textPhoneNo.Text = row.Cells["PhoneNo"].Value?.ToString() ?? "";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error selecting Party: {ex.Message}", "Error",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
