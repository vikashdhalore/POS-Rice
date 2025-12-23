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
    public partial class deleteParty : Form
    {
        public deleteParty()
        {
            InitializeComponent();
        }

        private void LoadData(string search = "")
        {
            string connectionString = @"Server=DESKTOP-DL3IHEJ\SQLEXPRESS;Database=Rice;Integrated Security=true;";
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
        private void LoadPartyData()
        {
            try
            {
                if (dataGridView1.Columns.Contains("CheckBoxColumn"))
                {
                    dataGridView1.Columns.Remove("CheckBoxColumn");
                }

                // Create connection string - update with your database details
                string connectionString = @"Server=DESKTOP-DL3IHEJ\SQLEXPRESS;Database=Rice;Integrated Security=true;";

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
                    //ClearForm();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading data: {ex.Message}", "Database Error",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void groupBox2_Enter(object sender, EventArgs e)
        {

        }

        private void deleteParty_Load(object sender, EventArgs e)
        {
           // Server=.\SQLEXPRESS;Database=Rice;Integrated Security=true;
            dataGridView1.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dataGridView1.MultiSelect = false; // Single row selection
            dataGridView1.ReadOnly = false; // Make data cells read-only
            dataGridView1.RowHeadersVisible = false; // Show row headers for better selection

            // Make only checkbox column editable
            if (dataGridView1.Columns.Contains("CheckBoxColumn"))
            {
                dataGridView1.Columns["CheckBoxColumn"].ReadOnly = false;
            }

            LoadPartyData();
            AddCheckBoxColumn();
        }

        private void textSearch_TextChanged(object sender, EventArgs e)
        {
            LoadData(textSearch.Text.Trim());
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            bool isChecked = chkSelectAll.Checked;

            foreach (DataGridViewRow row in dataGridView1.Rows)
            {
                row.Cells["CheckBoxColumn"].Value = isChecked;
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
                DialogResult result = MessageBox.Show($"Are you sure you want to delete {selectedRows.Count} party(s)?",
                                                    "Confirm Delete",
                                                    MessageBoxButtons.YesNo,
                                                    MessageBoxIcon.Question);

                if (result == DialogResult.Yes)
                {
                    // Create connection string
                    string connectionString = @"Server=DESKTOP-DL3IHEJ\SQLEXPRESS;Database=Rice;Integrated Security=true;";

                    using (SqlConnection connection = new SqlConnection(connectionString))
                    {
                        connection.Open();

                        foreach (DataGridViewRow row in selectedRows)
                        {
                            int partyId = Convert.ToInt32(row.Cells["PartyID"].Value);

                            string query = "DELETE FROM Party WHERE PartyID = @PartyID";

                            using (SqlCommand command = new SqlCommand(query, connection))
                            {
                                command.Parameters.AddWithValue("@PartyID", partyId);
                                command.ExecuteNonQuery();
                            }
                        }

                        MessageBox.Show($"{selectedRows.Count} party(s) deleted successfully!", "Success",
                                      MessageBoxButtons.OK, MessageBoxIcon.Information);

                        // Refresh DataGridView
                           LoadPartyData();

                        // Clear form fields

                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error deleting party(s): {ex.Message}", "Database Error",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

    }
}
